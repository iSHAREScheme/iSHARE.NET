using System;
using System.Text.Json.Serialization;

namespace iSHARE.Parties.Responses
{
    public class Certification
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("loa")]
        public LevelOfAssurance LoA { get; set; }
    }
}
