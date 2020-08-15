using AudioScan.Model;
using AudioScan.Sql;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudioScan
{
    public static class Dal
    {
        private static readonly ILogger Log = Settings.logger.Invoke("Dal");

        private static readonly Action<GetDirInfo, SqlParameterCollection>
            GetDirInfoCmd = Database.Mapper<GetDirInfo>();

        private static readonly Action<AudioTrack, SqlParameterCollection>
            AudioTrackCmd = Database.Mapper<AudioTrack>();

        private static readonly Action<SqlDataReader, AudioTrack> ReadAudioTrack = Database.Reader<AudioTrack>();
        private static readonly Action<SqlDataReader, DirInfo> ReadDirInfo = Database.Reader<DirInfo>();

        public static async Task<SqlConnection> Connect()
        {
            var str = Settings.Default.Db.ConnectionString;
            if (Log.IsTraceEnabled)
            {
                Log.Trace($"Connecting to {str}...");
            }

            var it = new SqlConnection(str);
            await it.OpenAsync().ConfigureAwait(false);
            return it;
        }

        public static async Task<List<T>> Read<T>(SqlDataReader source, Action<SqlDataReader, T> reader,
            CancellationToken token) where T : new()
        {
            var it = new List<T>();
            if (source.HasRows)
            {
                while (await source.ReadAsync(token).ConfigureAwait(false))
                {
                    var t = new T();
                    reader(source, t);
                    it.Add(t);
                }
            }

            return it;
        }


        public static async Task<List<AudioTrack>> SyncAudioTrack(AudioTrack target, CancellationToken token)
        {
            try
            {
                using (var connection = await Connect().ConfigureAwait(false))
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.update_track";
                    AudioTrackCmd(target, cmd.Parameters);

                    using var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
                    return await Read(reader, ReadAudioTrack, token).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception);

                return null;
            }
        }

        public static async Task<DirInfo> GetDirInfo(this string directoryName, CancellationToken token)
        {
            try
            {
                using (var connection = await Connect())
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "dbo.get_dirinfo";

                    var p = new GetDirInfo()
                    {
                        DirectoryPath = directoryName
                    };

                    GetDirInfoCmd(p, cmd.Parameters);

                    using var reader = await cmd.ExecuteReaderAsync(token).ConfigureAwait(false);
                    var it = await Read(reader, ReadDirInfo, token).ConfigureAwait(false);
                    return it.FirstOrDefault();
                }
            }
            catch (Exception exception)
            {
                Settings.logger.Invoke(typeof(Database).Name).Error(exception);

                return null;
            }
        }
    }
}