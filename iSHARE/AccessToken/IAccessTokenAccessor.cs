using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Args;

namespace iSHARE.AccessToken
{
    public interface IAccessTokenAccessor
    {
        /// <summary>
        /// Retrieves an access token and ensures that its type is correct and it has not expired.
        /// </summary>
        /// <param name="args">Access token request arguments.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Access token</returns>
        /// <exception cref="AuthenticationException">Throws if fails to receive an access token.</exception>
        /// <exception cref="ArgumentNullException">Throws if invalid arguments are passed.</exception>
        Task<string> GetAsync(AccessTokenRequestArgs args, CancellationToken token = default);
    }
}
