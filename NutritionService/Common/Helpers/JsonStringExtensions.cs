using System.Text.Json;

namespace NutritionService.Common.Helpers
{
    public static class JsonStringExtensions
    {
        /// <summary>
        /// Safely deserializes a raw database JSON string into a string array.
        /// Returns an empty array if the string is null, empty, or corrupted.
        /// </summary>
        public static IEnumerable<string> ToParsedStringArray(this string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Array.Empty<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? Enumerable.Empty<string>();
            }
            catch
            {
                // Failsafe for corrupted database data
                return Array.Empty<string>();
            }
        }
    }
}
