package com.zh.grpcToKafka

import com.google.protobuf.ByteString
import com.zh.proto._

object Payload {
  def validate(in: Payload): Seq[String] = {
    Seq(
      if (
         in.sha256.getOrElse(ByteString.EMPTY).isEmpty
      ) {
        Some("'sha256' field must be specified")
      } else None,
      if (in.fileName.isDefined && in.fileName.get.trim.isEmpty) {
        Some("'filename' value must no be blank or empty.")
      } else None,
      if (in.url.isDefined && in.url.get.trim.isEmpty) {
        Some("'url' value must no be blank or empty.")
      } else None,
      if (in.timestamp.isEmpty) {
        Some("'timestamp' must be specified.")
      } else None,
      if (in.sentBy.trim.isEmpty) {
        Some("'sent_by' must not be blank or empty.")
      } else None,      
      if (in.sha256.map(x => x.size()).getOrElse(32) != 32) {
        Some(s"'sha256' invalid value length.")
      } else None
    ).flatten
  }

  def validateList(in: Seq[Payload]): Seq[String] = {
    in.map(validate).zipWithIndex.flatMap {
      case (errors, index) => errors.map(error => s"$index: $error")
    }
  }
}
