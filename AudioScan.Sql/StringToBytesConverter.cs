using System;
using AudioScan.Common;

namespace AudioScan.Sql
{
    public class StringToBytesConverter : IDbValueConverter
    {
        public Type SourceType => typeof(string);

        public Type TargetType => typeof(byte[]);

        public object Convert(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            var v = value as string;

            if (v == null)
            {
                throw new InvalidOperationException();
            }

            return v.ToBytes();
        }
    }
}