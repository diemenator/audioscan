package com.zh.common.actors

import akka.actor.typed.ActorRef
import akka.Done

sealed trait SenderCommand[A]
final case class Initialize[A]() extends SenderCommand[A]
final case class Initialized[A](p:Sender[A]) extends SenderCommand[A]
final case class Send[A](message:A, replyTo: ActorRef[Done]) extends SenderCommand[A]
final case class ReInitialize[A]() extends SenderCommand[A]
final case class TimerTick[A]() extends SenderCommand[A]
final case class Terminate[A](error:Throwable) extends SenderCommand[A]
final case class Terminated[A](error:Throwable) extends SenderCommand[A]

