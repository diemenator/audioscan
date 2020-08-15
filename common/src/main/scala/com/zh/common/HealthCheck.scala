package com.zh.common

import java.util.concurrent.atomic.AtomicBoolean

import akka.actor.ActorSystem

import scala.concurrent.Future

class HealthCheck(actorSystem: ActorSystem) extends (() => Future[Boolean]) {
  private val actorSystemTerminated = new AtomicBoolean(false)
  actorSystem.registerOnTermination(actorSystemTerminated.set(true))

  override def apply(): Future[Boolean] = Future.successful(!actorSystemTerminated.get())
}
