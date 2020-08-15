package com.zh.common.metrics

import java.time.Instant

import com.timgroup.statsd.{NoOpStatsDClient, NonBlockingStatsDClient, StatsDClient}
import org.slf4j.LoggerFactory
object Metrics {
  def client(): StatsDClient = {
    statsd
  }

  val statsd: StatsDClient = {
    try {
      new NonBlockingStatsDClient(
        sys.env.getOrElse("DOGSTATSD_PREFIX", "app."),
        sys.env.getOrElse("DOGSTATSD_HOST","telegraf"),
        Integer.parseInt(sys.env.getOrElse("DOGSTATSD_PORT","2003"))
      )
    } catch {
      case e: Throwable =>
        val l = LoggerFactory.getLogger("Metrics")
        l.error("Failed to initialize NonBlockingStatsDClient, reverting to NoOpStatsDClient", e)
        new NoOpStatsDClient()
    }
  }

  def timeFrom(from:Instant, aspect:String, tags:String*): Unit = {
    val ms   = Instant.now().toEpochMilli - from.toEpochMilli
    statsd.time(aspect, ms, tags:_*)
  }

  def timeOf[A](f : => A, aspect:String, tags:String*): A = {
    val s = Instant.now()
    try {
      f
    }
    finally {
      timeFrom(s, aspect, tags:_*)
    }
  }

  def histogram(aspect:String, value: Double, tags: Seq[String]): Unit = {
    client().histogram(aspect, value, tags:_*)
  }


  def gauge(aspect:String, value: Double, tags: Seq[String]): Unit = {
    client().gauge(aspect, value, tags:_*)
  }


  def increment(aspect:String, value: Double, tags:Seq[String]): Unit = {
    client().increment(aspect, value, tags:_*)
  }

}
