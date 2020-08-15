using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AudioScan.Common
{
    public static class Utils
    {
        public static string ToInvariantByteString(this byte[] bytes)
        {
            var sb = new StringBuilder();

            bytes.Aggregate(sb,
                (s, c) =>
                {
                    s.Append(c.ToString("X2")); return s;

                });

            return "0x" + sb.ToString();
        }

        public static byte[] ToBytes(this string bytes)
        {
            if (string.IsNullOrWhiteSpace(bytes))
            {
                return null;
            }
            var s = bytes.Trim();

            s = s.StartsWith("0x") ? s.Substring(2) : s;
            if (s.Length % 2 != 0)
            {
                throw new FormatException();
            }

            var result = new List<byte>();

            var s2 = s.Length / 2;
            for (int i = 0; i < s2; i++)
            {
                byte b = 0;
                b = byte.Parse(string.Format("{0}{1}", s[i * 2], s[i * 2 + 1]), System.Globalization.NumberStyles.HexNumber);

                result.Add(b);
            }

            return result.ToArray();
        }
    }
}