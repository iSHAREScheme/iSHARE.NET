using iSHARE.Internals;
using Microsoft.Extensions.Configuration;

namespace iSHARE
{
    internal class DefaultSettings : IShareSettings
    {
        public DefaultSettings(IConfiguration configuration)
        {
            SchemeOwnerUrl = configuration.GetConfigurationValue("SchemeOwnerUrl").RemoveSlashSuffix();
            Eori = configuration.GetConfigurationValue("Eori");
        }

        public string SchemeOwnerUrl { get; }

        public string Eori { get; }
    }
}
