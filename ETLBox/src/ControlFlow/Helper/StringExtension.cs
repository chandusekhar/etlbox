﻿namespace ETLBox.Helper
{
    /// <summary>
    /// Extension methods for strings
    /// </summary>
    public static class StringExtension
    {
        /// <summary>
        ///		This replicates the functionality of case-insensitive functionality built into Replace in .Net Core.
        /// </summary>
        /// <param name="toSearch"></param>
        /// <param name="find"></param>
        /// <param name="replace"></param>
        /// <returns>The string with replaced values</returns>
        public static string ReplaceIgnoreCase(this string toSearch, string find, string replace) {
            int index = toSearch.IndexOf(
                find,
                System.StringComparison.InvariantCultureIgnoreCase);

            if (index < 0) {
                return toSearch;
            }

            string replacement = toSearch.Substring(0, index) + replace;

            if (toSearch.Length > index + find.Length) {
                replacement += toSearch.Substring(index + find.Length);
            }

            return replacement;
        }
    }
}
