using System.Threading;
using System.Threading.Tasks;
using iSHARE.TokenValidator.Args;

namespace iSHARE.TokenValidator
{
    public interface IJwtTokenResponseValidator
    {
        /// <summary>
        /// Validates if JWT token is iSHARE compliant and
        /// contains 'x5c' certificate (and proper chain) that belongs to the party which issued the token.
        /// </summary>
        /// <param name="args">Request arguments.</param>
        /// <param name="schemeOwnerAccessToken">
        /// Access token which is going to be used in Authorization header for SO requests.
        /// It is needed in order to retrieve trusted list and to check if certificate belongs to the issuing party.
        /// </param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Boolean result if provided JWT is valid within iSHARE.</returns>
        Task<bool> IsValidAsync(
            TokenValidationArgs args,
            string schemeOwnerAccessToken,
            CancellationToken token = default);
    }
}
