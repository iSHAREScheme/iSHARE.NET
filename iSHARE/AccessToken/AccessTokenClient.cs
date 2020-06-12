using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Args;
using iSHARE.AccessToken.Responses;

namespace iSHARE.AccessToken
{
    internal class AccessTokenClient : IAccessTokenClient
    {
        private readonly HttpClient _httpClient;

        public AccessTokenClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AccessTokenResponse> SendRequestAsync(AccessTokenRequestArgs args, CancellationToken token)
        {
            var requestBody = CreateBody(args.ClientId, args.ClientAssertion);

            var response = await _httpClient.PostAsync(RemoveSlashSuffix(args.RequestUri), requestBody, token);
            response.EnsureSuccessStatusCode();

            return await AccessTokenResponse.FromHttpContentAsync(response.Content);
        }

        private static FormUrlEncodedContent CreateBody(string clientId, string clientAssertion)
        {
            return new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", "iSHARE" },
                { "client_id", clientId },
                { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                { "client_assertion", clientAssertion }
            });
        }

        private static string RemoveSlashSuffix(string requestUri)
        {
            return requestUri.TrimEnd('/');
        }
    }
}
