package com.zh.common

import java.util.Properties

import com.zh.proto.Payload
import org.apache.kafka.clients.consumer.KafkaConsumer
import org.apache.kafka.clients.producer.{KafkaProducer, ProducerRecord, RecordMetadata}

import scala.collection.mutable
import scala.concurrent.{ExecutionContext, Future, blocking}
import scala.util.Failure

object Kafka extends Logging {

  def putClientProperties(settings: KafkaSettings, props: Properties): Unit = {
    props.put("bootstrap.servers", settings.KAFKA_BOOTSTRAP_SERVERS)
    settings.KAFKA_SECURITY_PROTOCOL.toUpperCase() match {
      case "PLAINTEXT" =>
        props.put("security.protocol", settings.KAFKA_SECURITY_PROTOCOL.toUpperCase)
      case "SSL" =>
        props.put("security.protocol", settings.KAFKA_SECURITY_PROTOCOL.toUpperCase)
        props.put("ssl.truststore.type", settings.TLSSettings.TRUSTSTORE_TYPE)
        props.put("ssl.truststore.location", settings.TLSSettings.TRUSTSTORE_PATH)
        props.put("ssl.truststore.password", settings.TLSSettings.TRUSTSTORE_PASSWORD)
        props.put("ssl.keystore.type", settings.TLSSettings.KEYSTORE_TYPE)
        props.put("ssl.keystore.location", settings.TLSSettings.KEYSTORE_PATH)
        props.put("ssl.keystore.password", settings.TLSSettings.KEYSTORE_PASSWORD)
        props.put("ssl.key.password", settings.TLSSettings.KEY_PASSWORD)
      case _ =>
        throw new Exception("Unknown KAFKA_SECURITY_PROTOCOL value")
    }
  }

  def consumerProperties(settings: KafkaSettings): Properties = {
    val props = new Properties()
    props.put("key.deserializer", "org.apache.kafka.common.serialization.ByteArrayDeserializer")
    props.put("value.deserializer", "org.apache.kafka.common.serialization.ByteArrayDeserializer")
    props.put("enable.auto.commit", "false")
    props.put("auto.offset.reset", "earliest")
    putClientProperties(settings, props)
    props
  }

  def consumerPropertiesMap(settings:KafkaSettings) = {
    val props = consumerProperties(settings)
    val map = new mutable.HashMap[String,String]()
    props.forEach({
      case (key:String, value:String) =>
        map.put(key,value)
      case _ =>
        ()
    })
    map.toMap
  }

  def producerProperties(settings: KafkaSettings): Properties = {
    val props = new Properties()
    props.put("acks", "all")
    props.put("key.serializer", "org.apache.kafka.common.serialization.ByteArraySerializer")
    props.put("value.serializer", "org.apache.kafka.common.serialization.ByteArraySerializer")
    props.put("retries", "2")
    props.put("linger.ms", "1000")
    props.put("request.timeout.ms", "10000")
    props.put("delivery.timeout.ms", "15000")
    putClientProperties(settings, props)
    props
  }

  def createProducer(): (KafkaProducer[Array[Byte], Array[Byte]], String) = {
    val settings = KafkaSettings.apply()
    val props = producerProperties(settings)
    (new KafkaProducer[Array[Byte], Array[Byte]](props), settings.KAFKA_TGT_TOPIC)
  }
}
