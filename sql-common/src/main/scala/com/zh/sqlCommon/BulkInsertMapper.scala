package com.zh.sqlCommon

import java.time.{Instant, ZoneOffset}
import java.time.format.DateTimeFormatter

import com.google.protobuf.ByteString
import com.google.protobuf.timestamp.Timestamp
import com.zh.common.Logging
import com.zh.common.io._
import com.zh.common.metrics.Metrics
import com.microsoft.sqlserver.jdbc.SQLServerConnection
import scalapb.descriptors.{FieldDescriptor, ScalaType}
import scalapb.{GeneratedMessage, GeneratedMessageCompanion}

import scala.collection.immutable

object BulkInsertMapper extends Logging {
  def getColumns[A <: GeneratedMessage](schema: String, tableName: String, settings: SqlSettings): Map[String, ColumnMetadata] = {
    SqlConnection
      .openConnectionString(settings).using(connection => ColumnMetadata.fromTable(connection, schema, tableName))
      .map(x => (x.name, x)).toMap
  }

  def formatBytes(bytes: Array[Byte]): String = {
    val sb = new StringBuilder
    for (b <- bytes) {
      sb.append(String.format("%02x", Byte.box(b)))
    }
    s"0x${sb.toString()}"
  }

  def formatByteString(bytes: ByteString): String = {
    formatBytes(bytes.toByteArray)
  }

  def formatString(s: String): String = {
    s"N'${s.replace("'", "''")}'"
  }

  def formatFloat(n: Number): String = {
    val s = n.toString
    s"'$s'"
  }

  def formatInt(n: Number): String = {
    val s = n.toString
    s
  }

  def formatBoolean(b: Boolean): String = {
    if (b) "1" else "0"
  }

  def dateTimeFormatter(): DateTimeFormatter = {
    DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSSSSSS")
  }

  def formatDateTime(ts: Timestamp): String = {
    s"'${Instant.ofEpochSecond(ts.seconds, ts.nanos).atZone(ZoneOffset.UTC).format(dateTimeFormatter())}'"
  }

  private def getter[A <: GeneratedMessage, T]
  (
    descriptor: FieldDescriptor,
    fieldFormatter: T => String
  ) = {
    (item: A) => {
      val protoValue =
        item.getClass.getDeclaredMethod(descriptor.scalaName).invoke(item).asInstanceOf[T]

      fieldFormatter(protoValue)
    }
  }

  private def optionGetter[A <: GeneratedMessage, T]
  (
    descriptor: FieldDescriptor,
    fieldFormatter: T => String
  ) = {
    (item: A) => {
      val protoValue =
        item.getClass.getDeclaredMethod(descriptor.scalaName).invoke(item).asInstanceOf[Option[T]]

      protoValue.map(fieldFormatter).getOrElse("null")
    }
  }

  private def ofDescriptor[A <: GeneratedMessage](descriptor: FieldDescriptor): Option[A => String] = {
    descriptor.scalaType match {
      case ScalaType.Message(msgDescriptor) if msgDescriptor.fullName == "google.protobuf.Timestamp" =>
        Some(getter[A, Timestamp](descriptor, formatDateTime))
      case ScalaType.Int | ScalaType.Long | ScalaType.Enum(_) =>
        Some(getter[A, Number](descriptor, formatInt))
      case ScalaType.Double | ScalaType.Float =>
        Some(getter[A, Number](descriptor, formatFloat))
      case ScalaType.Boolean =>
        Some(getter[A, Boolean](descriptor, formatBoolean))
      case ScalaType.String =>
        Some(getter[A, String](descriptor, formatString))
      case ScalaType.ByteString =>
        Some(getter[A, ByteString](descriptor, formatByteString))
      case ScalaType.Message(inner) if inner.fields.length == 1 =>
        inner.fields.head.scalaType match {
          case ScalaType.Message(msgDescriptor) if msgDescriptor.fullName == "google.protobuf.Timestamp" =>
            Some(optionGetter[A, Timestamp](descriptor, formatDateTime))
          case ScalaType.Int =>
            Some(optionGetter[A, Int](descriptor, formatInt(_)))
          case ScalaType.Double =>
            Some(optionGetter[A, Double](descriptor, formatFloat(_)))
          case ScalaType.Float =>
            Some(optionGetter[A, Float](descriptor, formatFloat(_)))
          case ScalaType.Long =>
            Some(optionGetter[A, Long](descriptor, formatInt(_)))
          case ScalaType.Enum(_) =>
            Some(optionGetter[A, Int](descriptor, formatInt(_)))
          case ScalaType.Boolean =>
            Some(optionGetter[A, Boolean](descriptor, formatBoolean))
          case ScalaType.String =>
            Some(optionGetter[A, String](descriptor, formatString))
          case ScalaType.ByteString =>
            Some(optionGetter[A, ByteString](descriptor, formatByteString))
          case _ => None
        }
      case _ => None
    }
  }

  def protoFormatters[A <: GeneratedMessage](companion: GeneratedMessageCompanion[A]): Map[String, A => String] = {
    companion.scalaDescriptor.fields.flatMap(x => ofDescriptor[A](x).map(f => (x.name, f))).toMap
  }

  def protoMappers[A <: GeneratedMessage]
  (
    companion: GeneratedMessageCompanion[A],
    schema: String,
    tableName: String,
    settings: SqlSettings,
    customMappings: Seq[(String, A => String)]
  ): immutable.Seq[(String, A => String)] = {
    val tableColumns = getColumns(schema, tableName, settings)
    val protoMappers = protoFormatters(companion)

    val tableKeys = tableColumns.keys.toSet

    val autoMaps =
      tableColumns
        .keys
        .toSet
        .intersect(protoMappers.keys.toSet)
        .toSeq
        .map(name => (name, protoMappers(name)))

    val cMap =
      customMappings.toMap

    val customMaps = cMap.keys.toSet
      .intersect(tableKeys).map(x => (x, cMap(x))).toSeq

    (autoMaps ++ customMaps)
      .toList
  }

  def insert[A]
  (
    data: Seq[A],
    mappings: Seq[(String, A => String)],
    settings: SqlSettings,
    schema: String,
    tableName: String
  ): Array[Int] = {
    val inow = Instant.now().toEpochMilli
    val (columns, mappers) = mappings.unzip
    var catalog = "_"
    try {
      val it =
        SqlConnection.openConnectionString(settings).using((c: SQLServerConnection) => {
          catalog = c.getCatalog
          c.createStatement().using(s => {
            data
              .grouped(1000)
              .map(batch =>
                batch.map(item => s"(${mappers.map(m => m(item)).mkString(" , ")} )").mkString(", \n")
              )
              .map(values =>
                "insert into [%s].[%s]\n(%s)\nvalues\n%s;"
                  .format(schema, tableName, columns.map(s => s"[$s]").mkString(" , "), values)
              )
              .foreach(st => s.addBatch(st))
            val it = s.executeBatch()

            val tags = Seq(
              s"buffer:[$schema].[$tableName]",
              s"db:$catalog",
              s"ok:true"
            )

            Metrics.histogram("db_insert_time_ms", Instant.now().toEpochMilli - inow, tags)
            Metrics.histogram("db_insert_rows", data.length, tags)

            it
          })
        })
      it
    } catch {
      case e: Throwable =>
        log.error(s"Failed to bulk insert into $schema.$tableName", e)
        val tags = Seq(
          s"buffer:[$schema].[$tableName]",
          s"db:$catalog",
          s"ok:false"
        )

        Metrics.histogram("db_insert_time_ms", Instant.now().toEpochMilli - inow, tags)
        Metrics.histogram("db_insert_rows", data.length, tags)

        throw e
    }
  }

  def insertProto[A <: GeneratedMessage]
  (
    companion: GeneratedMessageCompanion[A],
    schema: String,
    tableName: String,
    settings: SqlSettings,
    customMappings: Seq[(String, A => String)] = Seq.empty,
    data: Seq[A]
  ): Array[Int] = {
    val mappings = protoMappers[A](companion, schema, tableName, settings, customMappings)

    insert(data, mappings, settings, schema, tableName)
  }
}
