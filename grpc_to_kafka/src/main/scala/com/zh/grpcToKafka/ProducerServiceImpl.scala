package com.zh.grpcToKafka

import akka.Done
import akka.actor.typed.ActorRef
import akka.actor.typed.Scheduler
import akka.actor.typed.scaladsl.AskPattern._
import akka.actor.{ActorSystem}
import akka.util.Timeout
import com.zh.common.Logging
import com.zh.common.actors.{Send, SenderCommand}
import com.zh.common.metrics.Metrics.statsd
import com.zh.proto._
import org.apache.kafka.clients.producer.ProducerRecord
import com.zh.grpcToKafka.Payload
import scala.concurrent.{ExecutionContext, Future}

class ProducerServiceImpl
(
  producer: ActorRef[SenderCommand[ProducerRecord[Array[Byte], Array[Byte]]]],
  topic: String
)(
  implicit ec: ExecutionContext,
  sys: ActorSystem,
  timeout: Timeout,
  scheduler: Scheduler
) extends ProducerService with Logging {
  val counter = "grpc.events.in.newhash"
  val sentCounter = "grpc.payload.sent"
  val invalidCounter = "grpc.payload.invalid"
  val tag = "payload:xxx"

  private def toRecord(in: Payload) = {
    new ProducerRecord[Array[Byte], Array[Byte]](topic, null, in.toByteArray)
  }

  private def send(x: Payload): Future[Done] = {
    val done: Future[Done] = producer.ask[Done](it => Send(toRecord(x), it))
    done
      .map(y => {
        statsd.increment(sentCounter, tag, s"sent_by:${x.sentBy}")
        y
      })
  }

  override def produce(in: Payload): Future[Response] = {
    log.debug("Payload message in")

    statsd.increment(counter, tag)
    val errors = Payload.validate(in)
    if (errors.isEmpty) {
      val done: Future[Done] = send(in)
      done.map(_ => {
        log.debug("Payload message out")
        Response.defaultInstance.withOk(Empty.defaultInstance)
      }
      )
    } else {
      statsd.increment(invalidCounter, tag)
      Future {
        Response.defaultInstance.withInvalid(Invalid.defaultInstance.withMessages(errors))
      }
    }
  }

  override def produceList(in: PayloadList): Future[Response] = {
    log.debug("Payload message seq in")
    statsd.count(counter, in.items.length, tag)
    val errors = Payload.validateList(in.items)
    if (errors.isEmpty) {
      val futures =
        in.items.map(
          x => {
            send(x)
          }
        )
      Future.sequence(futures).map(_ => {
        log.debug("Payload message seq out")
        Response.defaultInstance.withOk(Empty.defaultInstance)
      })
    } else {
      statsd.count(invalidCounter, in.items.length, tag)
      Future {
        Response.defaultInstance.withInvalid(Invalid.defaultInstance.withMessages(errors))
      }
    }
  }
}
