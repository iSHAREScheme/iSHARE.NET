using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Internals.GenericHttpClient.Args;

namespace iSHARE.Internals.GenericHttpClient
{
    internal interface ITokenResponseClient
    {
        /// <summary>
        /// Sends a get request and retrieves a jwt token
        /// </summary>
        /// <param name="args">Request args.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>JWT token</returns>
        /// <exception cref="HttpRequestException">Throws when response status does not indicate success (200).</exception>
        /// <exception cref="TokenNotFoundException">Throws when response was successful but does not contain token.</exception>
        Task<string> SendRequestAsync(TokenSendRequestArgs args, CancellationToken token = default);
    }
}