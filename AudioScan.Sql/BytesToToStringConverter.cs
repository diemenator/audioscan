using System;
using AudioScan.Common;

namespace AudioScan.Sql
{
    public class BytesToToStringConverter : IDbValueConverter
    {
        public Type TargetType => typeof (string);

        public Type SourceType  => typeof (byte[]);

        public object Convert(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return null;
            }

            var v = value as byte[];

            if (v == null)
            {
                throw new InvalidOperationException();
            }

            return v.ToInvariantByteString();
        }
    }

    public class MillisToTimespanConverter : IDbValueConverter
    {
        public Type TargetType => typeof (TimeSpan);

        public Type SourceType  => typeof (long);

        public object Convert(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return null;
            }

            var v = (long)value;

            return TimeSpan.FromMilliseconds(v);
        }
    }

    public class TimespanToMillisConverter : IDbValueConverter
    {
        public Type SourceType  => typeof (TimeSpan);

        public Type TargetType => typeof (long);

        public object Convert(object value)
        {
            if (value == DBNull.Value || value == null)
            {
                return null;
            }

            var v = (TimeSpan)value;

            return System.Convert.ToInt64(Math.Round(v.TotalMilliseconds));
        }
    }
}