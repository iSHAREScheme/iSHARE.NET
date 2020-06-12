using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iSHARE.Capabilities.Responses
{
    public class SupportedVersion
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("supported_features")]
        public IReadOnlyCollection<SupportedFeature> SupportedFeatures { get; set; }
    }
}
