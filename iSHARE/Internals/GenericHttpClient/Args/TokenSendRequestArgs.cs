using System.Collections.Generic;

namespace iSHARE.Internals.GenericHttpClient.Args
{
    internal class TokenSendRequestArgs
    {
        /// <summary>
        /// Constructor for object which is used to encapsulate <see cref="ITokenResponseClient"/> parameters.
        /// </summary>
        /// <param name="requestUri">Absolute request URI.</param>
        /// <param name="parameters">Optional query parameters.</param>
        /// <param name="accessToken">Optional access token which is going to be used in authorization header.</param>
        public TokenSendRequestArgs(
            string requestUri,
            Dictionary<string, string> parameters = null,
            string accessToken = null)
        {
            RequestUri = requestUri;
            Parameters = parameters;
            AccessToken = accessToken;
        }

        /// <summary>
        /// Absolute request URI.
        /// </summary>
        public string RequestUri { get; }

        /// <summary>
        /// Query parameters. Key represents parameter name and value represents value.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>
        /// An access token which is going to be used in authorization header.
        /// </summary>
        public string AccessToken { get; } 
    }
}
