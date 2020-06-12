using System;
using iSHARE.Exceptions;
using Microsoft.Extensions.Configuration;

namespace iSHARE.Internals
{
    internal static class ExtensionMethods
    {
        public static string RemoveSlashSuffix(this string requestUri)
        {
            return requestUri.TrimEnd(' ').TrimEnd('/');
        }

        /// <summary>
        /// Gets configuration or throws an exception if it was not found.
        /// </summary>
        /// <returns>Configuration value</returns>
        /// <exception cref="InvalidConfigurationException">If value was not found.</exception>
        public static string GetConfigurationValue(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<string>(key);
            if (value == null)
            {
                throw new InvalidConfigurationException("Failed to retrieve configuration value for 'SchemeOwner'.");
            }

            return value;
        }

        public static long ToEpochTime(this DateTime dateTime)
        {
            var date = dateTime.ToUniversalTime();
            var ticks = date.Ticks - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Ticks;

            return ticks / TimeSpan.TicksPerSecond;
        }
    }
}
