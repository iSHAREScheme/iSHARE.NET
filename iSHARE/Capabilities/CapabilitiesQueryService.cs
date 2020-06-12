using System;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Capabilities.Args;
using iSHARE.Capabilities.Responses;
using iSHARE.Exceptions;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;

namespace iSHARE.Capabilities
{
    internal class CapabilitiesQueryService : ICapabilitiesQueryService
    {
        private readonly ITokenResponseClient _client;
        private readonly IJwtTokenParser _jwtTokenParser;
        private readonly IJwtTokenResponseValidator _tokenResponseValidator;
        private readonly string _eori;

        public CapabilitiesQueryService(
            ITokenResponseClient client,
            IJwtTokenParser jwtTokenParser,
            IJwtTokenResponseValidator tokenResponseValidator,
            IShareSettings settings)
        {
            _client = client;
            _jwtTokenParser = jwtTokenParser;
            _tokenResponseValidator = tokenResponseValidator;
            _eori = settings.Eori;
        }

        public async Task<CapabilitiesResponse> GetAsync(CapabilitiesRequestArgs args, CancellationToken token)
        {
            try
            {
                var requestArgs = new TokenSendRequestArgs(args.RequestUri, accessToken: args.AccessToken);
                var response = await _client.SendRequestAsync(requestArgs, token);

                var assertionModel = _jwtTokenParser.Parse(response);
                var validationArgs = CreateValidationArgs(args, assertionModel);
                if (!await _tokenResponseValidator.IsValidAsync(validationArgs, args.SchemeOwnerAccessToken, token))
                {
                    throw new UnsuccessfulResponseException($"Token which was retrieved from {args.RequestUri} is corrupted.");
                }

                return TokenConvert.DeserializeClaim<CapabilitiesResponse>(
                    assertionModel.JwtSecurityToken,
                    "capabilities_info");
            }
            catch (UnsuccessfulResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnsuccessfulResponseException("Capabilities request was unsuccessful.", e);
            }
        }

        private TokenValidationArgs CreateValidationArgs(CapabilitiesRequestArgs args, AssertionModel assertionModel)
        {
            return new TokenValidationArgs(
                assertionModel,
                args.RequestedPartyId,
                args.AccessToken == null ? null : _eori);
        }
    }
}
