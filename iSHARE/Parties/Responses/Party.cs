using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iSHARE.Parties.Responses
{
    public class Party
    {
        [JsonPropertyName("party_id")]
        public string PartyId { get; set; }

        [JsonPropertyName("party_name")]
        public string PartyName { get; set; }

        [JsonPropertyName("adherence")]
        public Adherence Adherence { get; set; }

        [JsonPropertyName("certifications")]
        public IEnumerable<Certification> Certifications { get; set; }

        [JsonPropertyName("capability_url")]
        public string CapabilityUrl { get; set; }
    }
}
