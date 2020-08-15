#!/bin/bash
sleep 5.0
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P ${SA_PASSWORD} -i /opt/init_test_db/init_test_db.sql

