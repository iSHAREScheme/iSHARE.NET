using System.Text.Json.Serialization;

namespace iSHARE.Capabilities.Responses
{
    public class SchemeRole
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
