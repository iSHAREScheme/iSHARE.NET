using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iSHARE.Exceptions;
using iSHARE.Internals;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.SchemeOwner;

namespace iSHARE.Parties
{
    internal class PartiesQueryService : IPartiesQueryService
    {
        private readonly string _requestUri;
        private readonly ITokenResponseClient _client;
        private readonly IJwtTokenParser _jwtTokenParser;
        private readonly ISchemeOwnerJwtTokenResponseValidator _tokenResponseValidator;

        public PartiesQueryService(
            IShareSettings settings,
            ITokenResponseClient client,
            IJwtTokenParser jwtTokenParser,
            ISchemeOwnerJwtTokenResponseValidator tokenResponseValidator)
        {
            _requestUri = $"{settings.SchemeOwnerUrl}/{Constants.SchemeOwnerPartiesEndpoint}";
            _client = client;
            _tokenResponseValidator = tokenResponseValidator;
            _jwtTokenParser = jwtTokenParser;
        }

        public async Task<PartiesResponse> GetAsync(PartiesRequestArgs args, CancellationToken token)
        {
            try
            {
                var requestArgs = MapIntoTokenSendRequestArgs(args);
                var response = await _client.SendRequestAsync(requestArgs, token);

                var clientAssertion = _jwtTokenParser.Parse(response);
                if (!_tokenResponseValidator.IsValid(clientAssertion))
                {
                    throw new UnsuccessfulResponseException("Token which was retrieved from SO is corrupted.");
                }

                return TokenConvert.DeserializeClaim<PartiesResponse>(clientAssertion.JwtSecurityToken, "parties_info");
            }
            catch (UnsuccessfulResponseException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnsuccessfulResponseException("Parties request was unsuccessful.", e);
            }
        }

        private TokenSendRequestArgs MapIntoTokenSendRequestArgs(PartiesRequestArgs args)
        {
            return new TokenSendRequestArgs(_requestUri, CreateParameters(args), args.AccessToken);
        }

        private static Dictionary<string, string> CreateParameters(PartiesRequestArgs args)
        {
            var dictionary = new Dictionary<string, string>();

            void AddParameterIfExists(string name, string value)
            {
                if (value != null)
                {
                    dictionary.Add(name, value);
                }
            }

            void AddParameter(string name, bool? value)
            {
                if (value.HasValue)
                {
                    dictionary.Add(name, value.ToString().ToLower());
                }
            }

            AddParameterIfExists("name", args.Name);
            AddParameterIfExists("eori", args.Eori);
            AddParameter("certified_only", args.CertifiedOnly);
            AddParameter("active_only", args.ActiveOnly);
            AddParameterIfExists("certificate_subject_name", args.CertificateSubjectName);
            AddParameterIfExists("page", args.Page == null ? null : args.Page.ToString());
            AddParameterIfExists("date_time", args.DateTime == null ? null : args.DateTime.Value.ToString("s") + "Z");

            return dictionary;
        }
    }
}