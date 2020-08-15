import sbt._

object CommonDependencies {
  lazy val akka_version = "2.6.8"
  lazy val kafka_clients_version = "2.5.0"

  val akkaActor = "com.typesafe.akka" %% "akka-actor" % akka_version
  val akkaDiscovery = "com.typesafe.akka" %% "akka-discovery" % akka_version
  val akkaActorTyped = "com.typesafe.akka" %% "akka-actor-typed" % akka_version
  val akkaStream = "com.typesafe.akka" %% "akka-stream" % akka_version
  val akkaSlf4j = "com.typesafe.akka" %% "akka-slf4j" % akka_version
  val akkaManagement = "com.lightbend.akka.management" %% "akka-management" % "1.0.8"
  val akkaKafka = "com.typesafe.akka" %% "akka-stream-kafka" % "2.0.3"
  // val kamon = "io.kamon" %% "kamon-bundle" % "2.1.2"
  val guava = "com.google.guava" % "guava" % "29.0-jre"
  val kafka_clients =  "org.apache.kafka" % "kafka-clients" % "2.5.0"
  val logback = "ch.qos.logback" % "logback-classic" % "1.2.3"
  val catsCore = "org.typelevel" %% "cats-core" % "2.1.1"
  val scalaTest = "org.scalatest" %% "scalatest" % "3.1.2" % Test
  val scalaCheck = "org.scalatestplus" %% "scalatestplus-scalacheck" % "3.1.0.0-RC2" % Test
  val statsd = "com.datadoghq" % "java-dogstatsd-client" % "2.8.1"
  val scalaTime = "com.github.nscala-time" %% "nscala-time" % "2.24.0"
  val sext = "com.github.nikita-volkov" % "sext" % "0.2.6"
}
