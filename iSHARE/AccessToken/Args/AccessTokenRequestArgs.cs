using System;

namespace iSHARE.AccessToken.Args
{
    public class AccessTokenRequestArgs
    {
        /// <summary>
        /// Constructor for object which is used to encapsulate <see cref="IAccessTokenAccessor"/> parameters.
        /// </summary>
        /// <param name="requestUri">Absolute URI towards token endpoint. E.x. "https://example.com/token".</param>
        /// <param name="clientId">Client that sends the request ID. Should be EORI number.</param>
        /// <param name="clientAssertion">Client assertion in JWT format.</param>
        /// <exception cref="ArgumentNullException">Throws if null or white space values provided.</exception>
        public AccessTokenRequestArgs(string requestUri, string clientId, string clientAssertion)
        {
            ValidateArguments(requestUri, clientId, clientAssertion);

            RequestUri = requestUri;
            ClientId = clientId;
            ClientAssertion = clientAssertion;
        }

        /// <summary>
        /// Absolute URI towards token endpoint. E.x. "https://example.com/token".
        /// </summary>
        public string RequestUri { get; }

        /// <summary>
        /// Client's that sends the request ID. Should be EORI number.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// Client assertion in JWT format.
        /// </summary>
        public string ClientAssertion { get; }

        private static void ValidateArguments(string requestUri, string clientId, string clientAssertion)
        {
            void ValidateSingle(string value, string nameOf)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameOf);
                }
            }

            ValidateSingle(requestUri, nameof(requestUri));
            ValidateSingle(clientId, nameof(clientId));
            ValidateSingle(clientAssertion, nameof(clientAssertion));
        }
    }
}
