using System.Threading;
using System.Threading.Tasks;
using iSHARE.Capabilities.Args;
using iSHARE.Capabilities.Responses;

namespace iSHARE.Capabilities
{
    public interface ICapabilitiesQueryService
    {
        /// <summary>
        /// Retrieves capabilities of the party. If an access token is provided - restricted endpoints are also included.
        /// </summary>
        /// <param name="args">Request arguments.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Party's capabilities response.</returns>
        Task<CapabilitiesResponse> GetAsync(CapabilitiesRequestArgs args, CancellationToken token = default);
    }
}
