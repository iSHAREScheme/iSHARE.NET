using System.Threading;
using System.Threading.Tasks;
using iSHARE.AccessToken.Args;
using iSHARE.AccessToken.Responses;

namespace iSHARE.AccessToken
{
    internal interface IAccessTokenClient
    {
        Task<AccessTokenResponse> SendRequestAsync(AccessTokenRequestArgs args, CancellationToken token = default);
    }
}