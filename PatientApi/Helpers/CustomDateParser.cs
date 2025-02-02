using System;
using System.Globalization;

namespace PatientApi.Helpers
{
    public class CustomDateParser
    {
        private static readonly string[] Formats =
        {
            "yyyy-MM-ddTHH:mm:ss.fffK", // Полный формат с миллисекундами и таймзоной
            "yyyy-MM-ddTHH:mm:ssK",     // Полный формат с таймзоной
            "yyyy-MM-ddTHH:mm:ss",      // Полный формат без таймзоны
            "yyyy-MM-ddTHH:mm",         // Без секунд
            "yyyy-MM-dd"                // Только дата
        };

        public static bool TryParseDate(string dateString, out DateTime result)
        {
            if (string.IsNullOrWhiteSpace(dateString))
            {
                result = default;
                return false;
            }

            // Декодируем URL (убираем %3A и прочие экранирования)
            string decodedDate = Uri.UnescapeDataString(dateString).Replace(" ", "T");

            // Парсим с учетом различных форматов
            return DateTime.TryParseExact(decodedDate, Formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out result);
        }
    }
}
