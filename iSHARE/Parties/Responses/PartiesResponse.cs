using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace iSHARE.Parties.Responses
{
    public class PartiesResponse
    {
        public int Count { get; set; }

        [JsonPropertyName("data")]
        public IReadOnlyList<Party> Parties { get; set; }
    }
}
