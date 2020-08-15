package com.zh.common

case class KafkaSettings
(
  KAFKA_BOOTSTRAP_SERVERS: String,
  KAFKA_SECURITY_PROTOCOL: String,
  KAFKA_TGT_TOPIC: String,
  TLSSettings: TLS.TLSSettings
)

object KafkaSettings extends Settings {
  def apply() = {
    new KafkaSettings(
      getOrFail("KAFKA_BOOTSTRAP_SERVERS"),
      getOrDefault("KAFKA_SECURITY_PROTOCOL", "PLAINTEXT"),
      getOrFail("KAFKA_TGT_TOPIC"),
      TLS.TLSSettings.apply("KAFKA_")
    )
  }
}