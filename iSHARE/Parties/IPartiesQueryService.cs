using System.Threading;
using System.Threading.Tasks;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;

namespace iSHARE.Parties
{
    public interface IPartiesQueryService
    {
        /// <summary>
        /// Retrieves the parties from the Scheme Owner according to given filter arguments.
        /// </summary>
        /// <param name="args">Filter arguments.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Parties</returns>
        Task<PartiesResponse> GetAsync(PartiesRequestArgs args, CancellationToken token = default);
    }
}
