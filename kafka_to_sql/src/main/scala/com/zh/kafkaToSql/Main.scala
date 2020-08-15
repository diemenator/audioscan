package com.zh.kafkaToSql

import akka.actor.ActorSystem
import akka.management.scaladsl.AkkaManagement
import akka.stream.ActorMaterializer
import akka.stream.scaladsl.{RestartSource, Sink}
import com.zh.common.{SafeApp, Settings}
import com.typesafe.config.{Config, ConfigFactory}

import scala.concurrent.duration._

object Main extends SafeApp with Settings {
  val conf: Config = ConfigFactory
    .parseString(
      s"""akka {
         |  log-config-on-start = off
         |  loggers = ["akka.event.slf4j.Slf4jLogger"]
         |  logging-filter = "akka.event.slf4j.Slf4jLoggingFilter"
         |  loglevel=${akkaLogLevel}
         |  management {
         |    http {
         |      hostname = "0.0.0.0"
         |      port = 8558
         |    }
         |    health-checks {
         |      liveness-checks {
         |        actor-system-alive = "com.zh.common.HealthCheck"
         |      }
         |    }
         |  }
         |}""".stripMargin)
    .withFallback(ConfigFactory.defaultApplication().resolve())

  val sys: ActorSystem = ActorSystem.create("eventConsumer", conf)

  RestartSource
    .withBackoff(1.second, 1.minute, 0.2)(() => SqlFlow.newHashEvents()(sys))
    .to(Sink.ignore).run()(ActorMaterializer.create(sys))
  AkkaManagement(sys).start()
}
