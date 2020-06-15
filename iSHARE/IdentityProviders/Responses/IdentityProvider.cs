using System;

namespace iSHARE.IdentityProviders.Responses
{
    public class IdentityProvider
    {
        public IdentityProvider(string name, string eori, Uri baseUri)
        {
            Name = name;
            Eori = eori;
            BaseUri = baseUri;
        }

        /// <summary>
        /// Name of Identity Provider.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// EORI of Identity Provider.
        /// </summary>
        public string Eori { get; }

        /// <summary>
        /// Base URI of identity provider. '/authorize', '/token' and '/userinfo` endpoints should be appended to it.
        /// </summary>
        public Uri BaseUri { get; }
    }
}
