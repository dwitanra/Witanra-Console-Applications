using System.IO;

namespace Witanra.AccountLogger.Extentions
{
    public static class StringExtentions
    {
        public static string RemoveHTMLFormatting(this string Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return null;
            }
            Value = Value.Replace("\r", "");
            Value = Value.Replace("\n", "");
            Value = Value.Trim();
            return Value;
        }

        public static string GetSafeFilename(this string Value)
        {
            return string.Join("_", Value.Split(Path.GetInvalidFileNameChars()));
        }
    }
}