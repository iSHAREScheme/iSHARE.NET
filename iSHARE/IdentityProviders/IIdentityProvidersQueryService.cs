using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Exceptions;
using iSHARE.IdentityProviders.Responses;

namespace iSHARE.IdentityProviders
{
    public interface IIdentityProvidersQueryService
    {
        /// <summary>
        /// Retrieves a collection of identity providers used within iSHARE.
        /// </summary>
        /// <param name="accessToken">Access token which is going to be used in the header for authorization.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Collection of identity providers.</returns>
        /// <exception cref="UnsuccessfulResponseException">Throws if unable to retrieve certificate authorities.</exception>
        Task<IReadOnlyCollection<IdentityProvider>> GetAsync(string accessToken, CancellationToken token = default);
    }
}
