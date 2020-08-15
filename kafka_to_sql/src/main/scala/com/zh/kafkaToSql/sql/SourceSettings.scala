package com.zh.kafkaToSql.sql

case class SourceSettings
(
  priority: Byte,
  name: String,
  id: Int
)