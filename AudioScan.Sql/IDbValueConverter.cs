using System;

namespace AudioScan.Sql
{
    public interface IDbValueConverter
    {
        Type SourceType { get; }

        Type TargetType { get; }

        object Convert(object value);
    }
}