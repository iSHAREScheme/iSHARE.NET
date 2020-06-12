using System;
using System.Text.Json.Serialization;

namespace iSHARE.Parties.Responses
{
    public class Adherence
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime? EndDate { get; set; }
    }
}
