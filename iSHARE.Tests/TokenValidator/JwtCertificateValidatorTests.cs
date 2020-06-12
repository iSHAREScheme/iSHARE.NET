using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.SchemeOwner.TestCaStrategy;
using iSHARE.TrustedList;
using iSHARE.TrustedList.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace iSHARE.Tests.TokenValidator
{
    public class JwtCertificateValidatorTests
    {
        private const string RandomCaPublicKey =
            @"-----BEGIN CERTIFICATE-----
MIIFdDCCA1ygAwIBAgIEAJiiOTANBgkqhkiG9w0BAQsFADBaMQswCQYDVQQGEwJO
TDEeMBwGA1UECgwVU3RhYXQgZGVyIE5lZGVybGFuZGVuMSswKQYDVQQDDCJTdGFh
dCBkZXIgTmVkZXJsYW5kZW4gUm9vdCBDQSAtIEczMB4XDTEzMTExNDExMjg0MloX
DTI4MTExMzIzMDAwMFowWjELMAkGA1UEBhMCTkwxHjAcBgNVBAoMFVN0YWF0IGRl
ciBOZWRlcmxhbmRlbjErMCkGA1UEAwwiU3RhYXQgZGVyIE5lZGVybGFuZGVuIFJv
b3QgQ0EgLSBHMzCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAL4yolQP
cPssXFnrbMSkUeiFKrPMSjTysF/zDsccPVMeiAho2G89rcKezIJnByeHaHE6n3WW
IkYFsO2tx1ueKt6c/DrGlaf1F2cY5y9JCAxcz+bMNO14+1Cx3Gsy8KL+tjzk7FqX
xz8ecAgwoNzFs21v0IJyEavSgWhZghe3eJJg+szeP4TrjTgzkApyI/o1zCZxMdFy
KJLZWyNtZrVtB0LrpjPOktvA9mxjeM3KTj215VKb8b475lRgsGYeCasH/lSJEULR
9yS6YHgamPfJEf0WwTUaVHXvQ9Plrk7O53vDxk5hUUurmkVLoR9BvUhTFXFkC4az
5S6+zqQbwSmEorXLCCN2QyIkHxcE1G6cxvx/K2Ya7Irl1s9N9WMJtxU51nus6+N8
6U78dULI7ViVDAZCopz35HCz33JvWjdAidiFpNfxC95DGdRKWCyMijmev4SH8RY7
Ngzp07TKbBlBUgmhHbBqv4LvcFEhMtwFdozL92TkA1CvjJFnq8Xy7ljY3r735zHP
bMk7ccHViLVlvMDoFxcHErVc0qsgk7TmgoNwNsXNo42ti+yjwUOH5kPiNL6VizXt
BznaqB16nzaeErAMZRKQFWDZJkBE41ZgpRDUajz9QdwOWke275dhdU/Z/seyHdTt
XUmzqWrLZoQT1Vyg3N9udwbRcXXIV2+vD3dbAgMBAAGjQjBAMA8GA1UdEwEB/wQF
MAMBAf8wDgYDVR0PAQH/BAQDAgEGMB0GA1UdDgQWBBRUrfrHkleuyjWcLhL75Lpd
INyUVzANBgkqhkiG9w0BAQsFAAOCAgEAMJmdBTLIXg47mAE6iqTnB/d6+Oea31BD
U5cqPco8R5gu4RV78ZLzYdqQJRZlwJ9UXQ4DO1t3ApyEtg2YXzTdO2PCwyiBwpwp
LiniyMMB8jPqKqrMCQj3ZWfGzd/TtiunvczRDnBfuCPRy5FOCvTIeuXZYzbB1N/8
Ipf3YF3qKS9Ysr1YvY2WTxB1v0h7PVGHoTx0IsL8B3+A3MSs/mrBcDCw6Y5p4ixp
gZQJut3+TcCDjJRYwEYgr5wfAvg1VUkvRtTA8KCWAg8zxXHzniN9lLf9OtMJgwYh
/WA9rjLA0u6NpvDntIJ8CsxwyXmA+P5M9zWEGYox+wrZ13+b8KKaa8MFSu1BYBQw
0aoRQm7TIwIEC8Zl3d1Sd9qBa7Ko+gE4uZbqKmxnl4mUnrzhVNXkanjvSr0rmj1A
fsbAddJu+2gw7OyLnflJNZoaLNmzlTnVHpL3prllL+U9bTpITAjc5CgSKL59NVzq
4BZ+Extq1z7XnvwtdbLBFNUjA9tbbws+eC8N3jONFrdI54OagQ97wUNNVQQXOEpR
1VmiiXTTn74eS9fGbbeIJG9gkaSChVtWQbzQRKtqE77RLFi3EjNYsjdj3BP1lB0/
QFH1T/U67cjF68IeHRaVesd+QnGTbksVtzDfqu1XhUisHWrdOWnk4Xl4vs4Fv6EM
94B7IWcnMFk=
-----END CERTIFICATE-----";

        private readonly Mock<IPartiesQueryService> _partiesQueryServiceMock;
        private readonly Mock<ITrustedListQueryService> _trustedListQueryServiceMock;
        private readonly Mock<ILogger<JwtCertificateValidator>> _loggerMock;
        private readonly IJwtCertificateValidator _sut;

        public JwtCertificateValidatorTests()
        {
            var testCaStrategy = new TestCaStrategy();
            _partiesQueryServiceMock = new Mock<IPartiesQueryService>();
            _trustedListQueryServiceMock = new Mock<ITrustedListQueryService>();
            _loggerMock = new Mock<ILogger<JwtCertificateValidator>>();

            SetupMocks();

            _sut = new JwtCertificateValidator(
                _partiesQueryServiceMock.Object,
                _trustedListQueryServiceMock.Object,
                testCaStrategy,
                _loggerMock.Object);
        }

        [Fact]
        public async Task IsValidAsync_EverythingValid_ReturnsTrue()
        {
            var args = CreateValidArgs();

            var result = await _sut.IsValidAsync(args, "access_token");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidAsync_ValidAndTrusted_ReturnsTrue()
        {
            var sut = new JwtCertificateValidator(
                 _partiesQueryServiceMock.Object,
                 _trustedListQueryServiceMock.Object,
                 new ProductionCaStrategy(),
                 _loggerMock.Object);
            var args = new CertificateValidationArgs(
                CertificateUtilities.FromBase64Der(Constants.TrustedCertificates.PublicKeyBase64Der),
                Constants.SchemeOwner.ClientId,
                new[]
                {
                    CertificateUtilities.FromBase64Der(Constants.TrustedCertificates.RootCaPublicKeyBase64Der)
                });

            var result = await sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidAsync_ChainInvalid_ReturnsFalse()
        {
            var args = new CertificateValidationArgs(
                CertificateUtilities.FromPemFormat(Constants.AbcParty.PublicKey),
                Constants.AbcParty.ClientId,
                new[] { CertificateUtilities.FromPemFormat(RandomCaPublicKey) });

            var result = await _sut.IsValidAsync(args, "access_token_which_wont_be_used_here");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidAsync_SomethingThrows_ReturnsFalse()
        {
            var args = CreateValidArgs();
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            var result = await _sut.IsValidAsync(args, "access_token");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidAsync_UntrustedRootAndProductionStrategy_ReturnsFalse()
        {
            var args = CreateValidArgs();
            var sut = new JwtCertificateValidator(
                _partiesQueryServiceMock.Object,
                _trustedListQueryServiceMock.Object,
                new ProductionCaStrategy(),
                _loggerMock.Object);

            var result = await sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public async Task IsValidAsync_ReturnsNotOneParty_ReturnsFalse(int count)
        {
            var args = CreateValidArgs();
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = count,
                    Parties = new[]
                    {
                        new Party
                        {
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddDays(-1),
                                EndDate = DateTime.UtcNow.AddDays(1),
                                Status = "Active"
                            }
                        }
                    }
                });

            var result = await _sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("Active", 1, 1)]
        [InlineData("Active", -1, -1)]
        [InlineData("Inactive", -1, 1)]
        public async Task IsValidAsync_InvalidAdherence_ReturnsFalse(
            string status,
            int addMinutesForStartDate,
            int addMinutesForEndDate)
        {
            var args = CreateValidArgs();
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 1,
                    Parties = new[]
                    {
                        new Party
                        {
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddMinutes(addMinutesForStartDate),
                                EndDate = DateTime.UtcNow.AddMinutes(addMinutesForEndDate),
                                Status = status
                            }
                        }
                    }
                });

            var result = await _sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidAsync_EndDateIsNull_ReturnsTrue()
        {
            var args = CreateValidArgs();
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 1,
                    Parties = new[]
                    {
                        new Party
                        {
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddMinutes(-10),
                                EndDate = null,
                                Status = "Active"
                            }
                        }
                    }
                });

            var result = await _sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsValidAsync_RootCertDoesNotExistInTrustedList_ReturnsFalse()
        {
            var args = CreateValidArgs();
            _trustedListQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new CertificateAuthority
                    {
                        CertificateFingerprint = "A78FDF7BA13BBD95C6236972DD003FAE07F4E447B791B6EF6737AD22F0B618625"
                    }
                });

            var result = await _sut.IsValidAsync(args, "access_token", CancellationToken.None);

            result.Should().BeFalse();
        }

        private static CertificateValidationArgs CreateValidArgs()
        {
            return new CertificateValidationArgs(
                CertificateUtilities.FromPemFormat(Constants.AbcParty.PublicKey),
                Constants.AbcParty.ClientId,
                new[]
                {
                    CertificateUtilities.FromPemFormat(Constants.TestCertificateAuthority.IntermediateCaPublicKey),
                    CertificateUtilities.FromPemFormat(Constants.TestCertificateAuthority.RootCaPublicKey)
                });
        }

        private void SetupMocks()
        {
            _trustedListQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new CertificateAuthority
                    {
                        Subject = "Random Root CA. Name is not validated, so it doesn't matter.",
                        CertificateFingerprint = "3C4FB0B95AB8B30032F432B86F535FE172C185D0FD39865837CF36187FA6F428"
                    },
                    new CertificateAuthority
                    {
                        Subject = "Prod Root CA. Name is not validated, so it doesn't matter.",
                        CertificateFingerprint = "85363A24CB1B66E6CF6244E87D243DBB8306F607357C614CB9C4C224A0E04358"
                    },
                    new CertificateAuthority
                    {
                        Subject = "C=NL, O=iSHARE, OU=Test, CN=iSHARETestCA",
                        CertificateFingerprint = "A78FDF7BA13BBD95C6236972DD003FAE07F4E447B791B6EF6737AD22F0B61862"
                    }
                });

            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 1,
                    Parties = new[]
                    {
                        new Party
                        {
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddDays(-1),
                                EndDate = DateTime.UtcNow.AddDays(1),
                                Status = "Active"
                            }
                        }
                    }
                });
        }
    }
}