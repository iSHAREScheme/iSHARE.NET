using System;
using System.Text.Json.Serialization;

namespace iSHARE.Capabilities.Responses
{
    public class FeatureObject
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("feature")]
        public string Feature { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; }
    }
}
