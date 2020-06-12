using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace iSHARE.TokenValidator
{
    internal static class SecurityKeysExtractor
    {
        public static IReadOnlyCollection<SecurityKey> Extract(IReadOnlyCollection<string> certificates) =>
            ConvertTox509Certificates(certificates)
                .Select(c => (SecurityKey)new X509SecurityKey(c))
                .ToArray();

        private static IEnumerable<X509Certificate2> ConvertTox509Certificates(IEnumerable<string> certificates) =>
            certificates
                .Select(CreateCertificateFromString)
                .Where(c => c != null);

        private static X509Certificate2 CreateCertificateFromString(string certificate) =>
            new X509Certificate2(Convert.FromBase64String(certificate));
    }
}