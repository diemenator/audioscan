package com.zh.common


import scala.util.control.NonFatal

package object io {
  implicit class AutoCloseableOps[T <: AutoCloseable](val autoCloseable: T) extends AnyVal {
    def using[R](f: T => R): R = com.zh.common.io.using(autoCloseable)(f)
  }

  def using[T <: AutoCloseable, R](autoCloseable: => T)(f: T => R): R = {
    val ac: T = autoCloseable
    require(ac != null, "resource is null")
    var exception: Throwable = null
    try {
      f(ac)
    } catch {
      case e: Throwable =>
        exception = e
        throw e
    } finally {
      close(exception, ac)
    }
  }

  private def close(e: Throwable, autoCloseable: AutoCloseable) = {
    if (e != null) {
      try {
        autoCloseable.close()
      } catch {
        case NonFatal(suppressed) =>
          e addSuppressed suppressed
        case fatal: Throwable if NonFatal(e) =>
          fatal addSuppressed e
          throw fatal
        case fatal: InterruptedException =>
          fatal addSuppressed e
          throw fatal
        case fatal: Throwable =>
          e addSuppressed fatal
      }
    } else {
      autoCloseable.close()
    }
  }
}
