using System;

namespace iSHARE.IdentityProviders.Responses
{
    public class IdentityProvider
    {
        public IdentityProvider(string name, Uri baseUri)
        {
            Name = name;
            BaseUri = baseUri;
        }

        /// <summary>
        /// Name of Identity Provider.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Base URI of identity provider. '/authorize', '/token' and '/userinfo` endpoints should be appended to it.
        /// </summary>
        public Uri BaseUri { get; }
    }
}
