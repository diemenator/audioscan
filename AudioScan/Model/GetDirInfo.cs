using AudioScan.Sql;

namespace AudioScan.Model
{
    public class GetDirInfo
    {
        [SqlParamValue]
        public string DirectoryPath { get; set; }
    }
}