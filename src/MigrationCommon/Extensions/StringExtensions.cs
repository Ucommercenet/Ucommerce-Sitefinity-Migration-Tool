using System;

namespace MigrationCommon.Extensions
{
    public static class StringExtensions
    {
        public static string ToShortName(this String str, int maxLength)
        {
            if (str.Length > maxLength)
            {
                return str.Substring(0, maxLength) + "...";
            }

            return str;
        }
    }
}
