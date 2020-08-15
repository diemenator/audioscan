using Akka.Actor;
using Akka.Configuration;
using Akka.Streams;
using Akka.Streams.Dsl;
using AudioScan.Model;
using NLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AudioScan
{
    class Program
    {
        private static ILogger Log = Settings.logger.Invoke("Main");

        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(hocon:
                @"akka { actor {
    processing-dispatcher {
      type = ForkJoinDispatcher
      throughput = 100
      dedicated-thread-pool {
          thread-count = 16
          deadlock-timeout = 3s
          threadtype = background
      }
    }

    sql-dispatcher {
      type = Dispatcher
      throughput = 100
    }
} }
");
            var sys = ActorSystem.Create("Program", config);


            var mat = ActorMaterializer.Create(sys);

            var i = Flow.Create<string>()
                .WithAttributes(ActorAttributes.CreateDispatcher("akka.actor.processing-dispatcher")).SelectAsync(16,
                    x =>
                        Task.Run(() =>
                        {
                            var track = Audio.Load(x);
                            return Tuple.Create(track, x);
                        })
                )
                .Select(x =>
                {
                    Log.Info($"processed path {x.Item2}.");
                    return x;
                })
                .Where(x => x.Item1 != null)
                .Select(x =>
                {
                    Log.Info($"parsed track {x.Item1.Title} by {x.Item1.Artist}.");
                    return x.Item1;
                });
            var o = Flow.Create<AudioTrack>()
                .WithAttributes(ActorAttributes.CreateDispatcher("akka.actor.sql-dispatcher"))
                .SelectAsync(16, async x =>
                {
                    var it = await Dal.SyncAudioTrack(x, CancellationToken.None).ConfigureAwait(false);
                    return Tuple.Create(it, it != null);
                }).Select(x =>
                    {
                        if (x.Item2)
                        {
                            foreach (var z in x.Item1)
                            {
                                Log.Info($"updated {z.Path} with id {z.Id}");
                            }

                            return x.Item1.Count;
                        }
                        else
                        {
                            return (int) 0;
                        }
                    }
                );

            var stream = Source
                .From(Directory.EnumerateFiles(Settings.Default.ScanDir, "*.mp3", SearchOption.AllDirectories))
                .Where(x => x != null)
                .Via(i)
                .Via(o)
                .WatchTermination(
                    async (x, complete) =>
                    {
                        await complete.ConfigureAwait(false);
                        await Task.Delay(1000);
                        await sys.Terminate();
                    });

            var sink = Sink.Sum<int>((x, y) => x + y);
            var materialized = stream.To(sink);

            materialized.Run(mat).Wait();
        }
    }
}