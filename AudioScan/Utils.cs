namespace AudioScan
{
    public static class StringExtensions
    {
        public static string Join(this string[] strings, char separator)
        {
            if (strings == null)
            {
                return null;
            }

            return string.Join(separator, strings);
        }
    }
}
