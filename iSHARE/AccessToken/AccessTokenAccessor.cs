using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Args;
using iSHARE.AccessToken.Responses;

namespace iSHARE.AccessToken
{
    internal class AccessTokenAccessor : IAccessTokenAccessor
    {
        private readonly IAccessTokenClient _client;
        private readonly IAccessTokenStorage _storage;

        public AccessTokenAccessor(IAccessTokenClient client, IAccessTokenStorage storage)
        {
            _client = client;
            _storage = storage;
        }

        public async Task<string> GetAsync(AccessTokenRequestArgs args, CancellationToken token = default)
        {
            ValidateArgs(args);

            try
            {
                var accessToken = await _storage.GetAsync(args.RequestUri, token);
                if (accessToken != null)
                {
                    return accessToken;
                }

                var accessTokenResponse = await _client.SendRequestAsync(args, token);
                EnsureValidAccessToken(accessTokenResponse);

                await _storage.AddAsync(args.RequestUri, accessTokenResponse, token);

                return accessTokenResponse.AccessToken;
            }
            catch (Exception e)
            {
                throw new AuthenticationException("Could not retrieve an access token.", e);
            }
        }

        private static void ValidateArgs(AccessTokenRequestArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
        }

        private static void EnsureValidAccessToken(AccessTokenResponse accessTokenResponse)
        {
            bool IsTokenTypeValid()
            {
                return accessTokenResponse.TokenType == null || accessTokenResponse.TokenType != "Bearer";
            }

            if (string.IsNullOrEmpty(accessTokenResponse.AccessToken))
            {
                throw new AuthenticationException("Access token cannot be null or empty.");
            }

            if (IsTokenTypeValid())
            {
                throw new AuthenticationException(
                    $"Access token type is {accessTokenResponse.TokenType}. It MUST be 'Bearer'.");
            }
        }
    }
}
