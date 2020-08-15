package com.zh.kafkaToSql

import akka.actor.{Actor, ActorLogging, ActorSystem, Props}
import akka.kafka.ConsumerMessage.CommittableOffset
import akka.kafka.scaladsl.Consumer.Control
import akka.kafka.scaladsl.{Committer, Consumer}
import akka.kafka.{ConsumerSettings, _}
import akka.stream.scaladsl.{Flow, Source}
import akka.{Done, NotUsed}
import com.zh.common.{Kafka, KafkaSettings, Logging}
import org.apache.kafka.common.serialization.ByteArrayDeserializer

import scala.collection.immutable._
import scala.concurrent.duration._
import scala.concurrent.{ExecutionContext, ExecutionContextExecutor, Future, blocking}

object KafkaSource extends Logging {
  type D = (Array[Byte], Array[Byte])
  type DC = (D, CommittableOffset)

  object Utils {
    def commit(sys: ActorSystem): Flow[ConsumerMessage.Committable, Done, NotUsed] = {
      val settings =
        CommitterSettings(sys)
          .withMaxBatch(1000)
          .withCommitWhen(CommitWhen.nextOffsetObserved)

      Committer.flow(settings)
    }

    def shutdownConsumer(name: String)(materialized: (Consumer.Control, Future[Done]))(implicit ec: ExecutionContext): Future[Unit] =
      materialized match {
        case (control, done) =>
          done
            .recover { case _ => Done }
            .flatMap { _ =>
              log.info(s"Shutting down $name kafka consumer...")
              control.shutdown().map(_ => log.info(s"Consumer $name was shut down."))
            }
      }

    def process(batchSize: Int, duration: FiniteDuration, parallelism: Int)
               (processor: Seq[D] => Unit)
               (implicit sys: ActorSystem): Flow[((Array[Byte], Array[Byte]), CommittableOffset), Done, NotUsed] = {
      implicit val ec: ExecutionContextExecutor = sys.dispatcher
      Flow[DC]
        .groupedWithin(batchSize, duration)
        .mapAsync(parallelism)((items: Seq[(D, CommittableOffset)]) =>
          Future {
            blocking {
              val (data: Seq[(Array[Byte], Array[Byte])], offsets: Seq[CommittableOffset]) = items.unzip
              processor(data)
              offsets
            }
          }
        )
        .mapConcat(x => x)
        .via(commit(sys))
    }
  }

  class RebalanceListener extends Actor with ActorLogging {
    def receive: Receive = {
      case TopicPartitionsAssigned(_, topicPartitions) =>
        log.info("Assigned: {}", topicPartitions)

      case TopicPartitionsRevoked(_, topicPartitions) =>
        log.info("Revoked: {}", topicPartitions)
    }
  }

  def apply(topic: String, groupId: String, settings: KafkaSettings)(implicit actorSystem: ActorSystem):
  Source[((Array[Byte], Array[Byte]), CommittableOffset), Control] = {

    val consumerSettings = ConsumerSettings(actorSystem, new ByteArrayDeserializer, new ByteArrayDeserializer)
      .withBootstrapServers(settings.KAFKA_BOOTSTRAP_SERVERS)
      .withProperties(Kafka.consumerPropertiesMap(settings))
      .withGroupId(groupId)
    val rebalanceListener = actorSystem.actorOf(Props(new RebalanceListener))
    val subscription = Subscriptions.topics(topic).withRebalanceListener(rebalanceListener)

    Consumer
      .sourceWithOffsetContext(consumerSettings, subscription)
      .map(x => (x.key(), x.value()))
      .asSource
      .backpressureTimeout(60.seconds)
  }
}
