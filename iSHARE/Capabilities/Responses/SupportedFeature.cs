using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iSHARE.Capabilities.Responses
{
    public class SupportedFeature
    {
        [JsonPropertyName("public")]
        public IReadOnlyCollection<FeatureObject> Public { get; set; }

        [JsonPropertyName("restricted")]
        public IReadOnlyCollection<FeatureObject> Restricted { get; set; }
    }
}
