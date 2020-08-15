package com.zh.grpcToKafka
import akka.{NotUsed, actor}
import akka.actor.{Scheduler, typed}
import akka.actor.typed._
import akka.actor.typed.ActorRef
import akka.actor.typed.scaladsl.adapter._
import akka.actor.typed.Behavior
import akka.actor.typed.scaladsl.Behaviors
import akka.stream.{ActorMaterializer, ActorMaterializerSettings, Supervision, TLSClientAuth}
import akka.grpc.scaladsl.ServiceHandler
import akka.grpc.scaladsl.ServerReflection
import akka.util.Timeout
import com.typesafe.config.ConfigValue

import scala.concurrent.{Await, ExecutionContextExecutor}
//#server-reflection
import akka.http.scaladsl._
import com.zh.common._

import com.zh.proto._
import com.zh.common.actors.kafka.Producer
import com.typesafe.config.{Config, ConfigFactory}
import akka.actor.typed.scaladsl.AskPattern._
import scala.concurrent.duration._

object Main extends SafeApp {
  def apply(): Behavior[NotUsed] = Behaviors.setup { ctx =>
    implicit val sys: actor.ActorSystem = ctx.system.toClassic
    implicit val mat: ActorMaterializer = ActorMaterializer(ActorMaterializerSettings(sys).withSupervisionStrategy(Supervision.getRestartingDecider))
    implicit val ec: ExecutionContextExecutor = sys.dispatcher
    implicit val scheduler: typed.Scheduler = ctx.system.scheduler
    implicit val timeout: Timeout = Timeout.durationToTimeout(1.minute)
    val topic = KafkaSettings.apply().KAFKA_TGT_TOPIC
 

    val producer = ctx.system.systemActorOf(Producer.apply(1000, 2.minutes), "producerActor", Props.empty)

    val service = ProducerServiceHandler.partial(new ProducerServiceImpl(producer, topic))
    val reflection = ServerReflection.partial(List(ProducerService))

    Http2().bindAndHandleAsync(
      ServiceHandler.concatOrNotFound(service, reflection),
      interface = "0.0.0.0",
      port = 5003,
      connectionContext = getConnectionCtx(),
      parallelism = 50
    )

    Behaviors.receiveSignal {
      case (_, Terminated(_)) => Behaviors.stopped
    }
  }

  private def getConnectionCtx() : ConnectionContext = {
    val settings = GRPCSettings.apply()

    if (settings.GRPC_USE_TLS) {
      val tlsCtx = TLS.Context.apply(settings.TLS)
      new HttpsConnectionContext(tlsCtx, clientAuth = if (settings.GRPC_USE_CLIENT_CERTIFICATES) { Some(TLSClientAuth.need) } else None)
    } else {
      new HttpConnectionContext()
    }
  }

  val conf: Config = ConfigFactory
    .parseString(
      s"""akka {
        |  log-config-on-start = off
        |  loggers = ["akka.event.slf4j.Slf4jLogger"]
        |  logging-filter = "akka.event.slf4j.Slf4jLoggingFilter"
        |  http.server.preview.enable-http2 = on
        |  loglevel=${akkaLogLevel}
        |}""".stripMargin)
    .withFallback(ConfigFactory.defaultApplication().resolve())
  ActorSystem(Main(), "event-publisher-grpc", conf)
}
