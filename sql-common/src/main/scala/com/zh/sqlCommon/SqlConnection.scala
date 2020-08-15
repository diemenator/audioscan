package com.zh.sqlCommon

import java.sql.{DriverManager, ResultSet}

import com.zh.common.Logging
import com.zh.common.io._

import com.microsoft.sqlserver.jdbc.SQLServerConnection

import scala.collection.mutable.ArrayBuffer
import scala.concurrent.{ExecutionContext, Future, blocking}

object SqlConnection extends Logging {
  def open(string: String): SQLServerConnection = {
    DriverManager.getConnection(string).asInstanceOf[SQLServerConnection]
  }

  def openConnectionString(sqlSettings: SqlSettings): SQLServerConnection = {
    open(sqlSettings.SQL_CONNECTION_STRING)
  }

  def open(server: String, database: String, props: String = "useUnicode=true;useBulkCopyForBatchInsert=true;characterEncoding=UTF-8;user=testUser;password=changeit;"): SQLServerConnection = {
    val it = s"jdbc:sqlserver://$server;databaseName=$database;$props"
    open(it)
  }


  def executeQuery[A](query: String, mapper: ResultSet => A, timeout: Int = 1200)(implicit c: SQLServerConnection): Seq[A] = {
    c.createStatement().using(s => {
      if (log.isTraceEnabled()){
        val catalog = c.getCatalog
        log.trace(
          s"""
             |query $catalog:
             |$query
             |""".stripMargin)
      }
      s.setQueryTimeout(timeout)
      s.executeQuery(query).using(r => {
        val ab = new ArrayBuffer[A]()
        while (r.next()) {
          val a = mapper(r)
          if (log.isTraceEnabled) {
            log.trace(s"read: ${a}")
          }
          ab.append(a)
        }
        ab.toSeq
      })
    })
  }

  def executeQueryAsync[A](query: String, mapper: ResultSet => A, timeout: Int = 1200)(implicit c: SQLServerConnection, ec: ExecutionContext): Future[Seq[A]] = {
    Future {
      blocking {
        executeQuery[A](query, mapper)
      }
    }
  }
}
