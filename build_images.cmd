set PWD=%CD%

call docker build -t openjdk11 openjdk11\.

call docker build -t openjdk11sbt openjdk11sbt\.

rem build inside using docker with proper jdk
call docker run --rm -v %CD%:/opt/apps:rw openjdk11sbt /bin/bash -c "cd /opt/apps && sbt clean && sbt universal:packageBin"

call docker build -t ops/grpc_to_kafka grpc_to_kafka\.

call docker build -t ops/kafka_to_sql kafka_to_sql\.

call docker build -t ops/telegraf telegraf\.

call docker build -t ops/prometheus prometheus\.

call docker build -t ops/grafana grafana\.
