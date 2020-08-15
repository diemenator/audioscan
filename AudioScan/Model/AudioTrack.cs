using AudioScan.Sql;
using System;

namespace AudioScan.Model
{
    public class AudioTrack
    {
        [SqlParamValue]
        [SqlColumn]
        public Guid Id { get; set; }

        [SqlColumn]
        public DateTime FirstSeen { get; set; }

        
        [SqlColumn]
        public DateTime LastSeen { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string FileName { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string Path { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string DirectoryPath { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string DirectoryName { get; set; } 


        [SqlParamValue]
        [SqlColumn]
        public string Album { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string Genre { get; set; }         

        [SqlParamValue]
        [SqlColumn]
        public string Artist { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public string Title { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public int Track { get; set; }

        [SqlColumn]
        public long LengthMillis { get; set; }

        [SqlParamValue(typeof(TimespanToMillisConverter), "LengthMillis")]
        [SqlColumn(typeof(MillisToTimespanConverter), "LengthMillis")]
        public TimeSpan Length { get; set; }

        [SqlParamValue(typeof(StringToBytesConverter))]
        [SqlColumn(typeof(BytesToToStringConverter))]
        public string Md5 { get; set; }

        [SqlParamValue(typeof(StringToBytesConverter))]
        [SqlColumn(typeof(BytesToToStringConverter))]
        public string PathMd5 { get; set; }
        
        [SqlParamValue]
        [SqlColumn]
        public int Rating { get; set; }
        
        [SqlParamValue]
        [SqlColumn]
        public int PlayCount { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public int SkipCount { get; set; }

        [SqlParamValue]
        [SqlColumn]
        public DateTime? LastPLayed { get; set; }
    }
}