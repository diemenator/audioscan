package com.zh.sqlCommon

import com.zh.common.io._
import com.microsoft.sqlserver.jdbc.SQLServerConnection

import scala.collection.mutable.ArrayBuffer

case class ColumnMetadata
(
  ordinal: Int,
  name: String,
  columnType: Int,
  precision: Int,
  scale: Int,
  isNullable: Boolean
)

object ColumnMetadata {
  def fromTable(sqlConnection: SQLServerConnection, schema: String, table: String): Seq[ColumnMetadata] = {
    sqlConnection.createStatement().using(s => {
      s.executeQuery(s"SELECT top 0 * from [$schema].[$table]").using(rs => {
        val md = rs.getMetaData
        val count = md.getColumnCount
        val buffer1 = new ArrayBuffer[ColumnMetadata]()
        for (i <- 0 until count) {
          val ii = i + 1
          val columnType = md.getColumnType(ii)
          val (p, s) = (md.getPrecision(ii), md.getScale(ii))
          buffer1.append(ColumnMetadata(
            ordinal = ii,
            name = md.getColumnName(ii),
            columnType = columnType,
            scale = s,
            isNullable = md.isNullable(ii) == 1,
            precision = p
          )
          )
        }
        buffer1
      })
    })
  }
}