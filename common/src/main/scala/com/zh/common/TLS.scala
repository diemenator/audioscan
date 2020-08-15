package com.zh.common
import com.zh.common.io._
import java.io.FileInputStream
import java.security.{KeyStore, SecureRandom}
import javax.net.ssl.{KeyManager, KeyManagerFactory, SSLContext, TrustManager, TrustManagerFactory, X509TrustManager}

object TLS {
  val protocol: String = "TLS"
  val managersAlgorithm: String = "SunX509"

  case class TLSSettings
  (
    KEYSTORE_TYPE: String,
    KEYSTORE_PATH: String,
    KEYSTORE_PASSWORD: String,
    KEY_PASSWORD: String,
    TRUSTSTORE_TYPE: String,
    TRUSTSTORE_PATH: String,
    TRUSTSTORE_PASSWORD: String
  )

  object TLSSettings extends Settings {
    def apply(prefix:String) = {
      new TLSSettings(
        getOrDefault(s"${prefix}KEYSTORE_TYPE", "JKS"),
        getOrDefault(s"${prefix}KEYSTORE_PATH", s"/secrets/${prefix}keystore.jks"),
        getOrDefault(s"${prefix}KEYSTORE_PASSWORD", "changeit"),
        getOrDefault(s"${prefix}KEY_PASSWORD", "changeit"),
        getOrDefault(s"${prefix}TRUSTSTORE_TYPE", "JKS"),
        getOrDefault(s"${prefix}TRUSTSTORE_PATH", s"/secrets/${prefix}truststore.jks"),
        getOrDefault(s"${prefix}TRUSTSTORE_PASSWORD", "changeit")
      )
    }
  }

  object Context {
    private def apply () : SSLContext = {
      SSLContext.getInstance(protocol)
    }

    def apply(keys: Array[KeyManager],trust: Array[TrustManager]): SSLContext = {
      val it = apply()
      it.init(keys, trust, new SecureRandom)
      it
    }

    def apply(trust: Array[TrustManager]): SSLContext = {
      val it = apply()
      it.init(null  , trust, new SecureRandom)
      it
    }

    def apply(keyStore: KeyStore, trustStore: KeyStore, keyPassword: String): SSLContext = {
      val keys = KeyManagers.fromKeyStore(keyStore, keyPassword)
      val trust = TrustManagers.fromKeyStore(keyStore)
      apply(keys,trust)
    }

    def apply(settings: TLSSettings): SSLContext = {
      val keys = KeyManagers.fromFile(settings.KEYSTORE_TYPE, settings.KEYSTORE_PATH, settings.KEYSTORE_PASSWORD, settings.KEY_PASSWORD)
      val trust = TrustManagers.fromFile(settings.TRUSTSTORE_TYPE, settings.TRUSTSTORE_PATH, settings.TRUSTSTORE_PASSWORD)
      apply(keys, trust)
    }

    def applyClient(settings: TLSSettings): SSLContext = {
      val trust = TrustManagers.fromFile(settings.TRUSTSTORE_TYPE, settings.TRUSTSTORE_PATH, settings.TRUSTSTORE_PASSWORD)
      apply(trust)
    }
  }

  object Store {
    private def apply(`type`:String): KeyStore = {
      `type` match {
        case "JKS" | "PKCS12" =>
        case _ => throw new SecurityException(s"Unknown store type '${`type`}'.'")
      }
      KeyStore.getInstance(`type`)
    }

    def fromFile(`type`:String, path:String, password: String): KeyStore = {
      val keyStore = apply(`type`)
      new FileInputStream(path).using { inputStream =>
        keyStore.load(inputStream, password.toCharArray)
      }
      keyStore
    }
  }

  object TrustManagers {
    def fromKeyStore(keyStore:KeyStore): Array[TrustManager] = {
      val factory = TrustManagerFactory.getInstance(managersAlgorithm)
      factory.init(keyStore)
      factory.getTrustManagers
    }

    def fromFile(`type`:String, path:String, password: String): Array[TrustManager] = {
      val keyStore = Store.fromFile(`type`, path, password)
      fromKeyStore(keyStore)
    }
  }

  object X509TrustManagers {
    def fromTrustManagers(trustManagers:Array[TrustManager]): Array[X509TrustManager] = {
      trustManagers.collect { case x: X509TrustManager => x }
    }

    def fromKeyStore(keyStore: KeyStore): Array[X509TrustManager] = {
      fromTrustManagers(TrustManagers.fromKeyStore(keyStore))
    }

    def fromFile(`type`:String, path:String, password: String): Array[X509TrustManager] = {
      val keyStore = Store.fromFile(`type`, path, password)
      fromKeyStore(keyStore)
    }
  }

  object KeyManagers {
    def fromKeyStore(keyStore: KeyStore, keyPassword:String): Array[KeyManager] = {
      val factory = KeyManagerFactory.getInstance(managersAlgorithm)
      factory.init(keyStore, keyPassword.toCharArray)
      factory.getKeyManagers
    }

    def fromFile(`type`:String, path:String, password: String, keyPassword:String): Array[KeyManager] = {
      val keyStore = Store.fromFile(`type`, path, password)
      fromKeyStore(keyStore, keyPassword)
    }
  }
}
