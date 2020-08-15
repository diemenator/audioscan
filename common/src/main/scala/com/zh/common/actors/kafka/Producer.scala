package com.zh.common.actors.kafka

import java.time.Instant
import java.util.Properties

import akka.Done
import akka.actor.typed.{Behavior, SupervisorStrategy}
import akka.actor.typed.scaladsl.Behaviors
import com.zh.common.Kafka.producerProperties
import com.zh.common.actors.{Sender, SenderBehavior, SenderCommand}
import com.zh.common.KafkaSettings
import com.zh.common.metrics.Metrics.statsd
import com.zh.common.metrics.Metrics
import org.apache.kafka.clients.producer.{KafkaProducer, ProducerRecord}

import scala.concurrent.{ExecutionContext, Future, blocking}
import scala.concurrent.duration._


object Producer {
  val produced = "kafka.produced"
  val delivered = "kafka.delivered"
  val timing = "kafka.produce.duration"

  type ProducerMessage = ProducerRecord[Array[Byte], Array[Byte]]

  class MessageSender extends Sender[ProducerMessage] {
    val settings: KafkaSettings = KafkaSettings.apply()
    val props: Properties = producerProperties(settings)
    val producer = new KafkaProducer[Array[Byte], Array[Byte]](props)

    override def send
    (
      message: ProducerMessage
    )(implicit ec: ExecutionContext): Future[Done] =
      Future {
        blocking {
          val tag = s"topic:${message.topic()}"
          Metrics.timeOf({
            statsd.increment(produced, tag)
            producer.send(message).get()
            statsd.increment(delivered, tag)
            Done.getInstance()
          }, timing, tag)
        }
      }

    override def close()(implicit ec: ExecutionContext): Future[Done] = Future {
      blocking {
        producer.close()
        Done.getInstance()
      }
    }
  }

  private def create()(implicit ec: ExecutionContext): Future[MessageSender] = Future {
    blocking {
      new MessageSender()
    }
  }

  def apply(bufferSize: Int, idleTimeout: FiniteDuration)(implicit ec: ExecutionContext): Behavior[SenderCommand[ProducerMessage]] = {
    val actor =
      SenderBehavior.apply[ProducerMessage](_ => create(), idleTimeout, bufferSize)
    Behaviors.supervise(actor).onFailure(SupervisorStrategy.restartWithBackoff(1.second, 1.minute, 0.2))
  }
}

