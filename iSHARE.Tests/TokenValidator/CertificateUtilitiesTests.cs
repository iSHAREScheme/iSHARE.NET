using System;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using iSHARE.TokenValidator;
using Xunit;

namespace iSHARE.Tests.TokenValidator
{
    public class CertificateUtilitiesTests
    {
        [Fact]
        public void FromPemFormat_FromBase64Der_BothCertsConvertedCorrectly()
        {
            var cert1 = CertificateUtilities.FromPemFormat(Constants.SchemeOwner.PublicKey);
            var cert2 = CertificateUtilities.FromBase64Der(Constants.SchemeOwner.PublicKeyBase64Der);

            var sha1 = cert1.GetSha256();
            var sha2 = cert2.GetSha256();

            sha1.Should().Be(sha2);
        }

        [Fact]
        public void GetSha256t_Null_Throws()
        {
            X509Certificate2 cert = null;

            Action act = () => cert.GetSha256();

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
