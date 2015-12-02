namespace Vurdalakov
{
    using System;
    using System.Globalization;

    public static class StringExtensions
    {
        public static Boolean ContainsWord(this String text, String word)
        {
            return !String.IsNullOrEmpty(text) && (text.Equals(word) || text.StartsWith(word + " ") || text.Contains(" " + word + " ") || text.EndsWith(" " + word));
        }

        public static String ToUpperFirst(this String text, CultureInfo cultureInfo = null)
        {
            if (null == cultureInfo)
            {
                cultureInfo = CultureInfo.CurrentUICulture;
            }

            return String.IsNullOrEmpty(text) ? text : Char.ToUpper(text[0], cultureInfo) + text.Substring(1);
        }

        public static String XmlEscape(this String text)
        {
            return String.IsNullOrEmpty(text) ? text : text.Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;");
        }

        public static String XmlUnescape(this String text)
        {
            return String.IsNullOrEmpty(text) ? text : text.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        }
    }
}
