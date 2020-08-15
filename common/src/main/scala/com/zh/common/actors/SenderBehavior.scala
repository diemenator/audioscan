package com.zh.common.actors

import java.time.Instant
import java.time.temporal.ChronoUnit

import akka.actor.typed.Behavior
import akka.actor.typed.scaladsl.Behaviors
import com.zh.common.Logging

import scala.concurrent.duration._
import scala.concurrent.{ExecutionContext, Future}
import scala.util.Failure

object SenderBehavior extends Logging {
  def apply[A]
  (
    ctor: Unit => Future[Sender[A]],
    idleTimeout: FiniteDuration,
    bufferSize: Int
  )(implicit ec: ExecutionContext): Behavior[SenderCommand[A]] = {
    Behaviors.withStash(bufferSize)(buffer =>
    {
      Behaviors.setup[SenderCommand[A]] { ctx =>

        val tickInterval = idleTimeout / 10
        log.debug("Actor starting.")

        def terminating() : Behaviors.Receive[SenderCommand[A]] = {
          log.debug("Entered Terminating State.")
          Behaviors.receiveMessagePartial[SenderCommand[A]] {
            case Terminated(error) =>
              throw error
              Behaviors.unhandled
            case _ =>
              Behaviors.same
          }
        }

        def active(p: Sender[A], last:Instant): Behaviors.Receive[SenderCommand[A]] = {
          log.debug("Entered Init State.")
          Behaviors.receiveMessagePartial[SenderCommand[A]] {
            case Send(message, replyTo) =>
              p.send(message)
                .map(x => replyTo ! x)
                .onComplete({
                  case Failure(x) =>
                    log.error("Terminating due to error", x)
                    ctx.self ! Terminate(x)
                  case _ => ()
                })
              active(p, Instant.now())
            case TimerTick() =>
              if (ChronoUnit.MILLIS.between(last, Instant.now()).millis > idleTimeout) {
                ctx.self ! ReInitialize()
              }
              ctx.scheduleOnce(tickInterval, ctx.self, TimerTick[A]())
              Behaviors.same
            case Initialize() =>
              Behaviors.same
            case Initialized(p2) =>
              p.close()
              active(p2, Instant.now())
            case ReInitialize() =>
              p.close().map(_ => ctx.self ! Initialize[A]())
              init()
            case Terminate(error) =>
              p.close().map(_ => ctx.self ! Terminated(error))
              terminating()
          }
        }

        def init() = {
          log.debug("Entered Active State.")
          Behaviors.receiveMessagePartial[SenderCommand[A]] {
            case Terminate(error) =>
              buffer.stash(Terminate(error))
              Behaviors.same
            case TimerTick() =>
              ctx.scheduleOnce(tickInterval, ctx.self, TimerTick[A]())
              Behaviors.same
            case Send(message, replyTo) =>
              buffer.stash(Send(message, replyTo))
              Behaviors.same
            case Initialized(p) =>
              val activated = active(p, Instant.now())
              buffer.unstashAll(activated)
              activated
            case ReInitialize() =>
              Behaviors.same
            case Initialize() =>
              ctor(Unit).map(x => ctx.self ! Initialized[A](x))
              Behaviors.same
            case other =>
              buffer.stash(other)
              Behaviors.same
          }
        }

        ctx.scheduleOnce(tickInterval, ctx.self, TimerTick[A]())
        ctx.self ! Initialize[A]()
        init()
      }

    }
    )
  }
}
