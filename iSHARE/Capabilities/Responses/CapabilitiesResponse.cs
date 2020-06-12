using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iSHARE.Capabilities.Responses
{
    public class CapabilitiesResponse
    {
        [JsonPropertyName("party_id")]
        public string PartyId { get; set; }

        [JsonPropertyName("ishare_roles")]
        public IReadOnlyCollection<SchemeRole> Roles { get; set; }

        [JsonPropertyName("supported_versions")]
        public IReadOnlyCollection<SupportedVersion> SupportedVersions { get; set; }
    }
}
