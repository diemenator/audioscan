package com.zh.common

trait Settings extends Logging {
  private val envs = sys.env

  def get(key:String): Option[String] = {
    val it = envs.get(key)
    it match {
      case Some(x) =>
        if (x.trim.isEmpty) {
          None
        } else Some(x)
      case None =>
        None
    }
  }

  def akkaLogLevel: String = {
    val it =
      getOrDefault("ROOT_LOG_LEVEL", "DEBUG")
    if (it == "TRACE") "DEBUG" else it
  }

  def getOrFail(key:String): String = {
    val it = get(key)
    it match {
      case Some(x) => x
      case None => throw new Exception(s"undefined required environment variable $key.")
    }
  }
  def getOrDefault(key:String, default: String): String = {
    get(key) match {
      case None =>
        log.warn(s"Using default value '$key' = '$default'.")
        default
      case Some(x) =>
        x
    }
  }

  def getOrEmpty(key:String): String = {
    getOrDefault(key, "")
  }


  def getBoolean(key:String): Boolean  = {
    getOrDefault(key, default = "false").toLowerCase.trim == "true"
  }
}
