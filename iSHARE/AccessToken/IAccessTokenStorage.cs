using System;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Responses;

namespace iSHARE.AccessToken
{
    /// <summary>
    /// Storage which keeps all access tokens. Used to avoid unnecessary calls to web services.
    /// </summary>
    internal interface IAccessTokenStorage
    {
        /// <summary>
        /// Adds access token into the storage. If such token already exists - overwrites it.
        /// </summary>
        /// <param name="requestUri">RequestUri for which access token should be stored.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Throws if invalid arguments are passed.</exception>
        Task AddAsync(string requestUri, AccessTokenResponse accessToken, CancellationToken token = default);

        /// <summary>
        /// Gets access token from the storage. If such token does not exist, returns null.
        /// </summary>
        /// <param name="requestUri">RequestUri for which access token should be retrieved.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Access token or null.</returns>
        /// <exception cref="ArgumentNullException">Throws if invalid arguments are passed.</exception>
        Task<string> GetAsync(string requestUri, CancellationToken token = default);
    }
}
