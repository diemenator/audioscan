ThisBuild / name := "apps-scala"
ThisBuild / version := "0.1"
ThisBuild / scalaVersion := "2.12.10"
ThisBuild / javacOptions ++= Seq("-source", "11", "-target", "11")
import sbt._
import Keys._
import CommonDependencies._

import akka.grpc.sbt.AkkaGrpcPlugin
import akka.grpc.sbt.AkkaGrpcPlugin.autoImport.akkaGrpcCodeGeneratorSettings

Compile / PB.targets := Seq(
  scalapb.gen(lenses = false) -> (sourceManaged in Compile).value
)

lazy val proto = (project in file("proto"))
  .enablePlugins(AkkaGrpcPlugin)
  .settings(
    libraryDependencies ++= Seq(
      "com.thesamet.scalapb" %% "scalapb-runtime" % scalapb.compiler.Version.scalapbVersion % "protobuf",
      "com.thesamet.scalapb" %% "scalapb-runtime-grpc" % scalapb.compiler.Version.scalapbVersion % "protobuf"
    ),
    akkaGrpcCodeGeneratorSettings += "server_power_apis"
  )

lazy val common = (project in file("common"))
  .dependsOn(proto)
  .settings(
    libraryDependencies ++= Seq(
      akkaActor,
      akkaDiscovery,
      akkaActorTyped,
      akkaStream,
      akkaSlf4j,
      // kamon,
      guava,
      kafka_clients,
      logback,
      catsCore,
      scalaTest,
      scalaCheck,
      statsd,
      scalaTime,
      sext,
      "com.typesafe.akka" %% "akka-http-spray-json" % "10.1.12"
    )
  )

lazy val grpc_to_kafka = (project in file("grpc_to_kafka"))
  .enablePlugins(AkkaGrpcPlugin, JavaAppPackaging, JavaAgent)
  .dependsOn(proto,common)
  .settings(
    libraryDependencies ++= Seq(
      akkaActor,
      akkaDiscovery,
      akkaActorTyped,
      akkaStream,
      akkaSlf4j,
      guava,
      kafka_clients,
      logback,
      catsCore,
      scalaTest,
      scalaCheck
    ),
    Universal / javaOptions ++= Seq (
      "-J-Xmx700m",
      "-J-Xms700m"
    )
  )

lazy val sql_common = (project in file("sql-common"))
  .dependsOn(common)
  .settings(
    libraryDependencies ++= Seq(
      "com.microsoft.sqlserver" % "mssql-jdbc" % "8.2.2.jre11",
      logback,
      catsCore,
      akkaActor,
      akkaActorTyped,
      akkaStream,
      akkaDiscovery,
      akkaSlf4j,
      scalaTest
    )
  )

lazy val kafka_to_sql = (project in file("kafka_to_sql"))
  .enablePlugins(JavaAppPackaging, JavaAgent)
  .dependsOn(proto,common,sql_common)
  .settings(
    libraryDependencies ++= Seq(
      akkaActor,
      akkaDiscovery,
      akkaActorTyped,
      akkaStream,
      akkaSlf4j,
      guava,
      akkaKafka,
      akkaManagement,
      kafka_clients,
      logback,
      catsCore,
      scalaTest,
      scalaCheck
    ),
    Universal / javaOptions ++= Seq (
      "-J-Xmx700m",
      "-J-Xms700m",
      "-Djava.security.auth.login.config=/kerberos-secrets/SQLJDBCDriver.conf",
      "-Djava.security.krb5.conf=/kerberos-secrets/krb5.conf"
    )
  )