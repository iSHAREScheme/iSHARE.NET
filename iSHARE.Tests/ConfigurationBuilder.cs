using System.IO;
using Microsoft.Extensions.Configuration;

namespace iSHARE.Tests
{
    internal static class ConfigurationBuilder
    {
        public static IConfigurationRoot Build() =>
            new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
    }
}
