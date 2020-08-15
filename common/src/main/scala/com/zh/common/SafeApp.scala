package com.zh.common

import scala.collection.mutable.ListBuffer
import scala.util.control.NonFatal

trait SafeApp extends DelayedInit with Logging with Settings {
  @deprecatedOverriding("args should not be overridden", "2.11.0")
  protected def args: Array[String] = _args

  private var _args: Array[String] = _

  private val initCode = new ListBuffer[() => Unit]

  @deprecated("the delayedInit mechanism will disappear", "2.11.0")
  override def delayedInit(body: => Unit) {
    initCode += (() => body)
  }

  @deprecatedOverriding("main should not be overridden", "2.11.0")
  def main(args: Array[String]) = {
    this._args = args
    try {
      for (proc <- initCode) proc()
    } catch {
      case NonFatal(e) =>
        log.error("Main failed.", e)
        throw e
    }
  }
}
