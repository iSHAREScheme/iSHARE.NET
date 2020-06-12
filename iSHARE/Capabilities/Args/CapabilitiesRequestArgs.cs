using System;

namespace iSHARE.Capabilities.Args
{
    public class CapabilitiesRequestArgs
    {
        /// <summary>
        /// Constructor for object which is used to encapsulate <see cref="ICapabilitiesQueryService"/> parameters.
        /// </summary>
        /// <param name="requestUri">Absolute URI towards capabilities endpoint. E.x. "https://example.com/capabilities".</param>
        /// <param name="requestedPartyId">Party's EORI number which is going to issue capabilities token for your party.</param>
        /// <param name="schemeOwnerAccessToken">
        /// Access token which is going to be used in Authorization header.
        /// We need it to validate retrieved capabilities_token.
        /// The token might not be issued or signed by the expected party, so requests to parties and trusted list endpoints need to be made.
        /// </param>
        /// <param name="accessToken">
        /// Optional parameter.
        /// If an access token for <see cref="requestUri"/> is provided then restricted endpoints are returned.
        /// </param>
        /// <exception cref="ArgumentNullException">Throws if <see cref="RequestUri"/> or <see cref="requestedPartyId"/> is null or whitespace.</exception>
        public CapabilitiesRequestArgs(
            string requestUri,
            string requestedPartyId,
            string schemeOwnerAccessToken,
            string accessToken = null)
        {
            ValidateArguments(requestUri, requestedPartyId, schemeOwnerAccessToken, accessToken);

            RequestUri = requestUri;
            RequestedPartyId = requestedPartyId;
            SchemeOwnerAccessToken = schemeOwnerAccessToken;
            AccessToken = accessToken;
        }

        /// <summary>
        /// Absolute URI towards capabilities endpoint. E.x. "https://example.com/capabilities".
        /// </summary>
        public string RequestUri { get; }

        /// <summary>
        /// Party's EORI number which is going to issue capabilities token for your party.
        /// </summary>
        public string RequestedPartyId { get; }

        /// <summary>
        /// Access token which is going to be used in Authorization header.
        /// We need it to validate retrieved capabilities_token.
        /// The token might not be issued or signed by the expected party, so requests to parties and trusted list endpoints need to be made.
        /// </summary>
        public string SchemeOwnerAccessToken { get; }

        /// <summary>
        /// If an access token is provided then restricted endpoints are returned.
        /// </summary>
        public string AccessToken { get; }

        private static void ValidateArguments(
            string requestUri,
            string requestedPartyId,
            string schemeOwnerAccessToken,
            string accessToken)
        {
            static void EnsureValidString(string name, string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(name);
                }
            }

            EnsureValidString(nameof(requestUri), requestUri);
            EnsureValidString(nameof(requestedPartyId), requestedPartyId);
            EnsureValidString(nameof(schemeOwnerAccessToken), schemeOwnerAccessToken);

            if (accessToken != null && string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(accessToken);
            }
        }
    }
}
