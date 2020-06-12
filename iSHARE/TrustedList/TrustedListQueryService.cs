using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Exceptions;
using iSHARE.Internals;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.SchemeOwner;
using iSHARE.TrustedList.Responses;

namespace iSHARE.TrustedList
{
    internal class TrustedListQueryService : ITrustedListQueryService
    {
        private readonly string _requestUri;
        private readonly ITokenResponseClient _client;
        private readonly IJwtTokenParser _jwtTokenParser;
        private readonly ISchemeOwnerJwtTokenResponseValidator _tokenResponseValidator;
        
        public TrustedListQueryService(
            IShareSettings settings,
            ITokenResponseClient client,
            IJwtTokenParser jwtTokenParser,
            ISchemeOwnerJwtTokenResponseValidator tokenResponseValidator)
        {
            _requestUri = $"{settings.SchemeOwnerUrl}/{Constants.SchemeOwnerTrustedListEndpoint}";
            _client = client;
            _jwtTokenParser = jwtTokenParser;
            _tokenResponseValidator = tokenResponseValidator;
        }

        public async Task<IReadOnlyCollection<CertificateAuthority>> GetAsync(string accessToken, CancellationToken token)
        {
            try
            {
                var requestArgs = new TokenSendRequestArgs(_requestUri, accessToken: accessToken);
                var response = await _client.SendRequestAsync(requestArgs, token);

                var clientAssertion = _jwtTokenParser.Parse(response);
                if (!_tokenResponseValidator.IsValid(clientAssertion))
                {
                    throw new UnsuccessfulResponseException("Token which was retrieved from SO is corrupted.");
                }

                return TokenConvert.DeserializeClaim<List<CertificateAuthority>>(
                    clientAssertion.JwtSecurityToken,
                    "trusted_list");
            }
            catch (UnsuccessfulResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnsuccessfulResponseException("Trusted list request was unsuccessful.", e);
            }
        }
    }
}