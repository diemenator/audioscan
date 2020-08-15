package com.zh.kafkaToSql


import akka.actor.ActorSystem
import akka.kafka.ConsumerMessage.CommittableOffset
import akka.kafka.scaladsl.Consumer.Control
import akka.stream.scaladsl.{Flow, Keep, Source}
import akka.{Done, NotUsed}
import com.zh.common.{KafkaSettings, Logging, Settings}
import com.zh.proto.Payload
import com.zh.sqlCommon.{BulkInsertMapper, SqlSettings}

import scala.collection.immutable._
import scala.concurrent.duration._
import scala.concurrent.{ExecutionContextExecutor, Future, blocking}


object SqlFlow extends Logging with Settings {
  def newHashEvents()(implicit actorSystem: ActorSystem): Source[Done, NotUsed] = {
    val tableName = "buffer"
    val schema = "dbo"
    val sqlSettings = SqlSettings.apply()
    val kafkaSettings = KafkaSettings.apply()
    val topic = kafkaSettings.KAFKA_TGT_TOPIC
    val consumerGroup = getOrDefault("KAFKA_CONSUMER_GROUP_ID", "com.zh.eventConsumer")

    val it = Future {
      blocking {


        val customMappers: Seq[(String, Payload => String)] = Seq(
          ("time", (z: Payload) => z.timestamp.map(BulkInsertMapper.formatDateTime).getOrElse("null"))
        )

        val mappers = BulkInsertMapper.protoMappers(Payload.messageCompanion, schema, tableName, sqlSettings, customMappers)

        val processor: Seq[(Array[Byte], Array[Byte])] => Unit =
          (data: Seq[(Array[Byte], Array[Byte])]) => {
            val it =
              BulkInsertMapper.insert(data.map({
                case (_, v) => Payload.parseFrom(v)
              }), mappers, sqlSettings, schema, tableName).toSeq.sum
            log.debug(s"database bulk insert returned sum = $it")
          }

        val flow: Flow[((Array[Byte], Array[Byte]), CommittableOffset), Done, Future[Done]] =
          KafkaSource.Utils.process(10000, 1.second, 4)(processor)(actorSystem)
            .watchTermination()(Keep.right)

        val source: Source[((Array[Byte], Array[Byte]), CommittableOffset), Control] =
          KafkaSource.apply(topic, consumerGroup, kafkaSettings)

        {
          implicit val ec: ExecutionContextExecutor = actorSystem.dispatcher
          source
            .viaMat(flow)(Keep.both)
            .mapMaterializedValue(KafkaSource.Utils.shutdownConsumer(topic))
        }
      }
    }(actorSystem.dispatcher)
    val it2: Source[Source[Done, Future[Unit]], NotUsed] = Source.future(it)
    val it3: Source[Done, NotUsed] = it2.flatMapConcat(x => x)
    it3
  }
}
