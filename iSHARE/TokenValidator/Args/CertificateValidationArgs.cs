using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace iSHARE.TokenValidator.Args
{
    internal class CertificateValidationArgs
    {
        public CertificateValidationArgs(
            X509Certificate2 partyCertificate,
            string partyEori,
            IEnumerable<X509Certificate2> additionalCertificates)
        {
            PartyCertificate = partyCertificate;
            PartyEori = partyEori;
            AdditionalCertificates = additionalCertificates as X509Certificate2[] ?? additionalCertificates.ToArray();
        }

        public X509Certificate2 PartyCertificate { get; }

        public string PartyEori { get; }

        /// <summary>
        /// Signed JWTs MUST contain an array of the complete certificate chain that should be used for validating
        /// the JWT’s signature in the x5c header parameter up until an Issuing CA is listed from the iSHARE Trusted List.
        /// The root certificate MUST be the last
        /// </summary>
        public X509Certificate2[] AdditionalCertificates { get; }
    }
}
