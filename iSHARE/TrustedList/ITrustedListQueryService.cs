using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Exceptions;
using iSHARE.TrustedList.Responses;

namespace iSHARE.TrustedList
{
    public interface ITrustedListQueryService
    {
        /// <summary>
        /// Retrieves PKIoverheid and eIDAS-qualified CAs valid under iSHARE.
        /// </summary>
        /// <param name="accessToken">Access token which is going to be used in the header for authorization.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Collection of trusted certificate authorities.</returns>
        /// <exception cref="UnsuccessfulResponseException">Throws if unable to retrieve certificate authorities.</exception>
        Task<IReadOnlyCollection<CertificateAuthority>> GetAsync(string accessToken, CancellationToken token = default);
    }
}
