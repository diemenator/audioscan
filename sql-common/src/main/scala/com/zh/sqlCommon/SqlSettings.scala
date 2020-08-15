package com.zh.sqlCommon


import com.zh.common.Settings

case class SqlSettings
(
  SQL_CONNECTION_STRING: String
)

object SqlSettings extends Settings {

  def apply(prefix:String): SqlSettings = {
    new SqlSettings(
        getOrDefault(s"${prefix}SQL_CONNECTION_STRING","jdbc:sqlserver://localhost;databaseName=TEST;useUnicode=true;useBulkCopyForBatchInsert=true;characterEncoding=UTF-8;user=testUser;password=changeit")
    )
  }

  def apply(): SqlSettings = apply("")
}
