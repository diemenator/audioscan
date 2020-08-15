package com.zh.sql.tests

import java.time.Instant
import java.util.Locale

import akka.actor.ActorSystem
import akka.stream.ActorMaterializer
import akka.stream.scaladsl.Source
import com.google.protobuf.ByteString
import com.google.protobuf.timestamp.Timestamp
import com.zh.common.{Kafka, KafkaSettings}
import com.zh.proto.NewHash
import com.zh.sql.{BulkInsertMapper, SqlSettings}
import org.apache.kafka.clients.consumer.KafkaConsumer
import org.scalatest._
import org.scalatest.matchers.should.Matchers

import scala.collection.mutable.ArrayBuffer
import scala.concurrent.ExecutionContext.Implicits._
import scala.util.Random


class Test extends FlatSpec with Matchers {
  val sqlSettings =
    SqlSettings.apply()

  val tableName = "Input"
  val schema = "st_NewFileInSource"


  def generateMd5(): Array[Byte] = {
    val it = new Array[Byte](16)
    Random.nextBytes(it)
    it
  }


  def genNewHash(seed: Int): NewHash = {
    val instant = Instant.now()
    val ts: Timestamp = Timestamp.of(instant.getEpochSecond, instant.getNano)
    val sent_by = s"sent_by$seed"
    val filename = s"file_$seed.txt"

    NewHash
      .defaultInstance
      .withSourceId(seed % 255)
      .withMd5(ByteString.copyFrom(generateMd5()))
      .withTimestamp(ts)
      .withSentBy(sent_by)
      .withFileName(filename)
  }

  "Bulk insert statement" should "work" in {

    Locale.setDefault(new Locale("en", "US"))
    val customMappers: Seq[(String, NewHash => String)] = Seq(
      ("download_time", (z: NewHash) => z.timestamp.map(BulkInsertMapper.formatDateTime).getOrElse("null")),
      ("thread", (z: NewHash) => BulkInsertMapper.formatInt(13)),
      ("uri", (z: NewHash) => z.url.map(BulkInsertMapper.formatString).getOrElse("null"))
    )

    val buffer = ArrayBuffer[NewHash]()
    for (i <- 0 until 10000) {
      buffer.append(genNewHash(i))
    }

    BulkInsertMapper.insertProto(NewHash.messageCompanion, schema, tableName, sqlSettings, customMappers, buffer)

  }

  //"Consumer" should "work" in {
//
  //  val ac = ActorSystem.create()
  //  val mat = ActorMaterializer.create(ac)
//
  //  Source.unfold()
//
  //  val buffer = ArrayBuffer[NewHash]()
  //  for (i <- 0 until 10000) {
  //    buffer.append(genNewHash(i))
  //  }
  //  Kafka.produceList(buffer)
  //  val c = Kafka.createConsumer()
//
//
  //}
}
