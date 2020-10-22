using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Internals.GenericHttpClient.Args;

namespace iSHARE.Internals.GenericHttpClient
{
    internal class TokenResponseClient : ITokenResponseClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenResponseClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> SendRequestAsync(TokenSendRequestArgs args, CancellationToken token)
        {
            var httpClient = CreateHttpClient(args.AccessToken);
            var fullUri = AppendParametersIfNeeded(args.RequestUri, args.Parameters);

            var response = await httpClient.GetAsync(fullUri, token);
            response.EnsureSuccessStatusCode();

            return await ExtractToken(response.Content, token);
        }

        private static string AppendParametersIfNeeded(
            string requestUri,
            IReadOnlyDictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return requestUri.RemoveSlashSuffix();
            }

            var query = parameters.Aggregate(
                string.Empty,
                (current, parameter) => current + $"{parameter.Key}={parameter.Value}&");
            query = query.TrimEnd('&');

            return $"{requestUri.RemoveSlashSuffix()}?{query}";
        }

        private static async Task<string> ExtractToken(HttpContent httpContent, CancellationToken token)
        {
            using var responseStream = await httpContent.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(
                responseStream,
                cancellationToken: token);

            var tokenPair = response.FirstOrDefault(x => x.Key.EndsWith("_token"));
            if (WasTokenFound(tokenPair))
            {
                return tokenPair.Value;
            }

            var msg = $"Token with suffix '_token' was not found.{Environment.NewLine}" +
                      $"Response: {JsonSerializer.Serialize(response)}";
            throw new TokenNotFoundException(msg);
        }

        private static bool WasTokenFound(KeyValuePair<string, string> tokenPair)
        {
            return !tokenPair.Equals(default(KeyValuePair<string, string>));
        }

        private HttpClient CreateHttpClient(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            if (accessToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return httpClient;
        }
    }
}
