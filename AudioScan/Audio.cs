using System;
using System.IO;
using System.Text;
using AudioScan.Model;
using AudioScan.Common;
using NLog;
using System.Security.Cryptography;
using ATL;

namespace AudioScan
{
    public static class Audio
    {
        private static ILogger Log = Settings.logger.Invoke("Audio");

        public static AudioTrack Load(string filename)
        {
            var result = new AudioTrack();
            try
            {
                Log.Debug($"Loading '{filename}'...");
                var t = new Track(filename);
                {
                    result.Id = Guid.Empty;
                    result.FileName = Path.GetFileName(filename);
                    result.Path = Path.GetFullPath(filename);
                    result.DirectoryPath = Path.GetDirectoryName(result.Path);
                    result.DirectoryName = Directory.GetParent(result.Path).Name;
                    result.Genre = t.Genre;
                    result.Album = t.Album;
                    result.Artist = t.AlbumArtist ?? t.Artist;
                    result.Title = t.Title;
                    result.Track = t.TrackNumber;
                    var duration = Convert.ToInt64(t.DurationMs);
                    result.LengthMillis = duration;
                    result.Length = TimeSpan.FromMilliseconds(duration);
                }

                using (var crypto = MD5.Create())
                {
                    var content = File.ReadAllBytes(filename);
                    result.Md5 = crypto.ComputeHash(content).ToInvariantByteString();
                    result.PathMd5 = crypto.ComputeHash(Encoding.UTF8.GetBytes(result.Path.ToLowerInvariant())).ToInvariantByteString();
                }

                var now = DateTime.UtcNow;
                result.FirstSeen = result.LastSeen = now;
                return result;
            }
            catch (Exception e)
            {
                Log.Error(e);

                return null;
            }
        }

    }
}
