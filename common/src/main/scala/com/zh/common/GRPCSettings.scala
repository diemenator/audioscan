package com.zh.common

import com.zh.common.TLS.TLSSettings

case class GRPCSettings
(
  GRPC_USE_TLS: Boolean,
  GRPC_USE_CLIENT_CERTIFICATES: Boolean,
  TLS: TLSSettings
)

object GRPCSettings extends Settings {
  def apply() = {
    new GRPCSettings(
      getBoolean("GRPC_USE_TLS"),
      getBoolean("GRPC_USE_CLIENT_CERTIFICATES"),
      TLSSettings.apply("GRPC_")
    )
  }
}
