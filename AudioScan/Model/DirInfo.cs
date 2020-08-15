using AudioScan.Sql;
using System;

namespace AudioScan.Model
{
    public class DirInfo
    {
        [SqlColumn]
        public DateTime LastSeen { get; set; }

        [SqlColumn]
        public long TotalFilesCount { get; set; }

        [SqlColumn]
        public long FilesCount { get; set; }
    }
}