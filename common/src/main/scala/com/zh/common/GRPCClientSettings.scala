package com.zh.common

import akka.actor.ClassicActorSystemProvider
import akka.grpc.GrpcClientSettings
import com.zh.common.TLS.TLSSettings

object GRPCClientSettings extends Settings {
  def apply(prefix: String)(implicit ac: ClassicActorSystemProvider): GrpcClientSettings = {
    val host = getOrDefault(s"${prefix}HOST", "localhost")
    val port = Integer.parseInt(getOrDefault(s"${prefix}PORT", "5003"))
    val useTls = getBoolean(s"${prefix}USE_TLS")
    val builder = GrpcClientSettings.connectToServiceAt(host, port)
    if (useTls) {
      val tls = TLSSettings.apply(prefix)
      val useCli = getBoolean(s"${prefix}USE_CLIENT_CERTIFICATE")
      val ctx =
        if (useCli) {
          TLS.Context.apply(settings = tls)
        } else {
          TLS.Context.applyClient(settings = tls)
        }
      builder.withTls(true).withSslContext(ctx)
    } else {
      builder.withTls(false)
    }
  }
}
