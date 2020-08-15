using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace AudioScan.Common
{
    public sealed class LoggingConfig : LoggingConfiguration
    {
    }

    public static class LoggingInstaller
    {
        public const string MinLevelConfigurationKey = "Log.MinLevel";
        public const string DirectoryConfigurationKey = "Log.Directory";
        public const string MaxArchiveFilesConfigurationKey = "Log.MaxArchiveFiles";
        public const string FileSizeConfigurationKey = "Log.FileSize";

        public const string DefaultLoggingRulesPattern = "*";

        public const string DefaultMessageLayout =
            "${longdate} ${level} [${logger}, ${threadid}]\t${message}${newline}${exception}\t${custom.sql-exception}\t${custom.exception-details:maxInnerExceptionLevel=5:format=all}${newline}";

        private const long DefaultFileSize = 1048576;
        private const int DefaultMaxArchiveFiles = 1024;

        static LoggingInstaller()
        {
            AppDomain.CurrentDomain.UnhandledException += (a, b) =>
            {
                if (LogManager.Configuration != null)
                {
                    var logger = LogManager.GetLogger("Unhandled Exception");
                    logger.Error(b.ExceptionObject);
                }
            };
        }

        public static void Apply(this LoggingConfig configuration)
        {
            LogManager.Configuration = configuration;
        }

        public static LoggingConfig Configure(bool apply = false)
        {
            var config = CreateConfiguration()
                .WriteToTrace();

            if (apply) LogManager.Configuration = config;

            return config;
        }

        public static LoggingConfig WriteToTrace(this LoggingConfig configuration,
            string name = "trace",
            LogLevel minLevel = null, bool apply = true)
        {
            var target = new TraceTarget
            {
                Name = name,
                Layout = DefaultMessageLayout
            };

            configuration.AddTarget(target);
            configuration.LoggingRules.Add(new LoggingRule(DefaultLoggingRulesPattern,
                ConfigureMinLevel(minLevel, name), target));

            if (apply)
            {
                configuration.Apply();
            }

            return configuration;
        }

        public static LoggingConfig WriteToConsole(this LoggingConfig configuration,
            string name = "coloredConsole",
            LogLevel minLevel = null, bool apply = true)
        {
            var console = new ColoredConsoleTarget
            {
                Name = name,
                Layout = DefaultMessageLayout
            };

            configuration.AddTarget(console);
            configuration.LoggingRules.Add(new LoggingRule(DefaultLoggingRulesPattern,
                ConfigureMinLevel(minLevel, name), console));

            if (apply)
            {
                configuration.Apply();
            }

            return configuration;
        }

        /// <summary>
        /// WriteToFile("file", DefaultLoggingRulesPattern, "log", minLevel, ...)
        /// .WriteToFile("errors-file", DefaultLoggingRulesPattern, "errors-log", LogLevel.Error ...)
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="path"></param>
        /// <param name="minLevel"></param>
        /// <param name="apply"></param>
        /// <returns></returns>
        public static LoggingConfig WriteToFolder(this LoggingConfig configuration, string path = null,
            LogLevel minLevel = null, bool apply = true)
        {
            var resolvedPath = ResolveLogsPath(path);

            configuration.WriteToFile("file", DefaultLoggingRulesPattern, "log", minLevel, directory: resolvedPath)
                .WriteToFile("errors-file", DefaultLoggingRulesPattern, "errors-log", LogLevel.Error,
                    directory: resolvedPath);

            if (apply)
            {
                configuration.Apply();
            }

            return configuration;
        }

        public static Target Async(this Target target,
            int batchSize = 1024,
            int queueLimit = 2048,
            int batchTimeout = 1000,
            AsyncTargetWrapperOverflowAction overflowAction = AsyncTargetWrapperOverflowAction.Discard)
        {
            return new AsyncTargetWrapper
            {
                Name = target.Name + "_async",
                BatchSize = 1024,
                OverflowAction = overflowAction,
                QueueLimit = 1024,
                TimeToSleepBetweenBatches = 5000,
                WrappedTarget = target
            };
        }

        private static LoggingConfig CreateConfiguration()
        {
            var configuration = LogManager.Configuration;

            if (configuration != null && configuration.GetType() != typeof(LoggingConfig))
            {
                throw new InvalidOperationException(
                    $"Can't configure via LoggingInstaller as Logging is already configured via {configuration.GetType().AssemblyQualifiedName}.");
            }

            return (configuration as LoggingConfig) ?? new LoggingConfig();
        }

        public static LoggingConfig WriteToFile(this LoggingConfig configuration,
            string name,
            string loggerNamePattern,
            string fileNamePrefix,
            LogLevel minLevel = null,
            string directory = null)
        {
            var dir = directory;

            var actualPath = ResolveLogsPath(dir);

            var target = CreateFileTarget(name, actualPath, fileNamePrefix);
            configuration.AddTarget(target);

            var rule = new LoggingRule(loggerNamePattern, ConfigureMinLevel(minLevel, name), target);
            configuration.LoggingRules.Add(rule);

            return configuration;
        }

        public static LoggingConfiguration IgnoreLogger(this LoggingConfiguration configuration,
            string loggerNamePattern)
        {
            var nullTarget = configuration.AllTargets.OfType<NullTarget>().FirstOrDefault();

            if (nullTarget == null)
            {
                nullTarget = new NullTarget("null-logger");

                configuration.AddTarget(nullTarget);
            }

            configuration.LoggingRules.Insert(0,
                new LoggingRule(loggerNamePattern, LogLevel.Trace, LogLevel.Error, nullTarget) {Final = true});

            return configuration;
        }

        private static FileTarget CreateFileTarget(string name, string directory, string fileNamePrefix = "")
        {
            var fileTarget = new FileTarget
            {
                Name = name,
                FileName = Path.Combine(directory, $"{fileNamePrefix}${{#}}.{{###}}.log"),
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                ArchiveOldFileOnStartup = true,
                ConcurrentWrites = true,
                KeepFileOpen = false,
                MaxArchiveFiles = DefaultMaxArchiveFiles,
                ArchiveAboveSize = DefaultFileSize,
                Layout = DefaultMessageLayout
            };

            return fileTarget;
        }

        public static LogLevel ConfigureMinLevel(LogLevel minLevel, string targetName)
        {
            if (minLevel == null)
            {
                LogLevel defaultMinLevel;
                {
#if DEBUG
                    defaultMinLevel = LogLevel.Debug;
#else
                    defaultMinLevel = LogLevel.Info;
#endif
                    Trace.TraceInformation(
                        $"Using Default logging level '{defaultMinLevel}' for target '{targetName}'.");
                }
                Trace.TraceInformation(
                    $"Using Configuration {defaultMinLevel} '{MinLevelConfigurationKey}' for target '{targetName}'.");
                return defaultMinLevel;
            }

            Trace.TraceInformation(
                $"Using Explicit {MinLevelConfigurationKey} '{minLevel}' for target '{targetName}'.");
            return minLevel;
        }

        public static string ResolveLogsPath(string path)
        {
            // use nlog layout renderer to get actual path
            var actualPath = path;

            if (string.IsNullOrEmpty(actualPath))
            {
                Trace.TraceInformation(
                    $"Applying {DirectoryConfigurationKey} settings property from configuration/appSettings.");

                Trace.TraceInformation($"Configured {DirectoryConfigurationKey} is '{actualPath}'.");
            }

            var throwAwayTarget = new FileTarget {FileName = actualPath};

            actualPath = throwAwayTarget.FileName.Render(new LogEventInfo(LogLevel.Error, "none", "none"));

            Trace.TraceInformation($"Resolved log directory is '{actualPath}'.");

            actualPath = actualPath.TrimEnd('\\', '/');
            if (!Directory.Exists(actualPath))
            {
                Directory.CreateDirectory(actualPath);
            }

            return actualPath;
        }
    }
}