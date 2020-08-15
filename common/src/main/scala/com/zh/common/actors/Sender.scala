package com.zh.common.actors
import scala.concurrent.{ExecutionContext, Future}
import scala.util.Failure
import scala.util.Success
import akka.Done
import akka.actor.typed.ActorRef
import akka.actor.typed.Behavior
import akka.actor.typed.scaladsl.ActorContext
import akka.actor.typed.scaladsl.Behaviors
import akka.actor.typed.scaladsl.StashBuffer

trait Sender[A] {
  def send(message:A)(implicit ec:ExecutionContext):Future[Done]
  def close()(implicit ec:ExecutionContext):Future[Done]
}
