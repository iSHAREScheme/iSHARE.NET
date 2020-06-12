using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace iSHARE.AccessToken.Responses
{
    public class AccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        /// <summary>
        /// Seconds
        /// </summary>
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public static async Task<AccessTokenResponse> FromHttpContentAsync(HttpContent httpContent)
        {
            await using var responseStream = await httpContent.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AccessTokenResponse>(responseStream);
        }
    }
}
