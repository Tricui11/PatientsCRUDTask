using System.Globalization;

namespace PatientApi.Helpers
{
    public class CustomDateParser
    {
        private static readonly string[] Formats =
        {
            "yyyy-MM-ddTHH:mm:ss.fffK",
            "yyyy-MM-ddTHH:mm:ssK",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm",
            "yyyy-MM-dd"
        };

        public static bool TryParseDate(string dateString, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                result = default;
                return false;
            }
            string decodedDate = Uri.UnescapeDataString(dateString).Replace(" ", "T");

            return DateTime.TryParseExact(decodedDate, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
        }
    }
}
