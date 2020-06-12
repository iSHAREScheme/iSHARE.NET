using FluentAssertions;
using iSHARE.TokenValidator.SchemeOwner.RootCertificate;
using Xunit;

namespace iSHARE.Tests.TokenValidator.SchemeOwner.RootCertificate
{
    public class CertificateStorageTests
    {
        [Fact]
        public void GetSha256_Test_ReturnsSha()
        {
            var sut = new TestEnvironmentRootCertificateStorage();

            var result = sut.GetSha256();

            result.Should().Be("A78FDF7BA13BBD95C6236972DD003FAE07F4E447B791B6EF6737AD22F0B61862");
        }

        [Fact]
        public void GetSha256_Prod_ReturnsSha()
        {
            var sut = new ProductionEnvironmentRootCertificateStorage();

            var result = sut.GetSha256();

            result.Should().Be("85363A24CB1B66E6CF6244E87D243DBB8306F607357C614CB9C4C224A0E04358");
        }
    }
}
