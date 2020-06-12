using System;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Responses;
using Microsoft.Extensions.Caching.Distributed;

namespace iSHARE.AccessToken
{
    /// <summary>
    /// Implementation of access token storage which makes use of <see cref="IDistributedCache"/>.
    /// </summary>
    internal class DistributedCacheAccessTokenStorage : IAccessTokenStorage
    {
        private const string CachePrefix = "DistributedCacheAccessTokenStorage";

        private readonly IDistributedCache _distributedCache;

        public DistributedCacheAccessTokenStorage(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        /// <inheritdoc/>
        public async Task AddAsync(string requestUri, AccessTokenResponse accessToken, CancellationToken token)
        {
            ValidateArguments(requestUri, accessToken);

            var cacheKey = CreateCacheKey(requestUri);
            var options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(TimeSpan.FromSeconds(accessToken.ExpiresIn));

            await _distributedCache.SetStringAsync(cacheKey, accessToken.AccessToken, options, token);
        }

        /// <inheritdoc/>
        public async Task<string> GetAsync(string requestUri, CancellationToken token)
        {
            ValidateArguments(requestUri);

            var cacheKey = CreateCacheKey(requestUri);

            return await _distributedCache.GetStringAsync(cacheKey, token);
        }

        private static void ValidateArguments(string requestUri)
        {
            if (string.IsNullOrWhiteSpace(requestUri))
            {
                throw new ArgumentNullException(nameof(requestUri));
            }
        }

        private static void ValidateArguments(string requestUri, AccessTokenResponse accessToken)
        {
            ValidateArguments(requestUri);

            if (accessToken == null)
            {
                throw new ArgumentNullException(nameof(accessToken));
            }
        }

        private static string CreateCacheKey(string requestUri) => $"{CachePrefix}-{requestUri}";
    }
}
