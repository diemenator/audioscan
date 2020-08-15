package com.zh.common

import org.slf4j.{Logger, LoggerFactory}

trait Logging { self =>
  val log: Logger =
    new MLogger(
      LoggerFactory.getLogger(self.getClass.getSimpleName)
    )
}
