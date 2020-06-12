using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using iSHARE.TokenValidator.SchemeOwner;
using iSHARE.TokenValidator.SchemeOwner.RootCertificate;
using iSHARE.TokenValidator.SchemeOwner.TestCaStrategy;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace iSHARE.Tests.TokenValidator.SchemeOwner
{
    public class SchemeOwnerJwtTokenResponseValidatorTests
    {
        private readonly IShareSettings _settings;
        private readonly Mock<ILogger<SchemeOwnerJwtTokenResponseValidator>> _loggerMock;
        private readonly Mock<IDecodedJwtValidator> _decodedJwtValidatorMock;
        private readonly ISchemeOwnerJwtTokenResponseValidator _sut;

        public SchemeOwnerJwtTokenResponseValidatorTests()
        {
            var testCaStrategy = new TestCaStrategy();
            var rootCertificateStorage = new TestEnvironmentRootCertificateStorage();
            _decodedJwtValidatorMock = new Mock<IDecodedJwtValidator>();
            _settings = new DefaultSettings(ConfigurationBuilder.Build());
            _loggerMock = new Mock<ILogger<SchemeOwnerJwtTokenResponseValidator>>();

            _sut = new SchemeOwnerJwtTokenResponseValidator(
                _decodedJwtValidatorMock.Object,
                rootCertificateStorage,
                _loggerMock.Object,
                testCaStrategy,
                _settings);
        }

        [Fact]
        public void IsValid_ExceptionOccurs_HandlesAndReturnsFalse()
        {
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);
            var jwtString = CreateJwtString();
            var assertionModel = new AssertionModel(null, CreateToken(jwtString), jwtString);

            var result = _sut.IsValid(assertionModel);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_NotIShareCompliant_ReturnsFalse()
        {
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(
                    It.Is<TokenValidationArgs>(
                        args => args.Issuer == "EU.EORI.NL000000000" && args.Audience == "EU.EORI.NL000000001")))
                .Returns(false);
            var assertionModel = CreateAssertionModel();

            var result = _sut.IsValid(assertionModel);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_JwtSignedUsingNotSOCert_ReturnsFalse()
        {
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);
            var assertionModel = CreateAssertionModel();

            var result = _sut.IsValid(assertionModel);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_JwtSignedUsingSOCert_ReturnsTrue()
        {
            var jwtString = CreateJwtString();
            // screwing assertionModel because signing jwtString with SO private key is not an option
            // we won't expose private key to you! :)
            var assertionModel = new AssertionModel(
                    new[]
                    {
                        Constants.SchemeOwner.PublicKeyBase64Der,
                        Constants.TestCertificateAuthority.IntermediateCaPublicKeyBase64Der,
                        Constants.TestCertificateAuthority.RootCaPublicKeyBase64Der
                    },
                    (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(jwtString),
                    jwtString);
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);

            var result = _sut.IsValid(assertionModel);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_ValidAndTrusted_ReturnsTrue()
        {
            var jwtString = CreateJwtString();
            // screwing assertionModel because signing jwtString with SO private key is not an option
            // we won't expose private key to you! :)
            var assertionModel = new AssertionModel(
                new[]
                {
                    Constants.TrustedCertificates.PublicKeyBase64Der,
                    Constants.TrustedCertificates.RootCaPublicKeyBase64Der
                },
                (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(jwtString),
                jwtString);
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);
            var sut = new SchemeOwnerJwtTokenResponseValidator(
                _decodedJwtValidatorMock.Object,
                new ProductionEnvironmentRootCertificateStorage(),
                _loggerMock.Object,
                new ProductionCaStrategy(), 
                _settings);

            var result = sut.IsValid(assertionModel);

            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_JwtSignedUsingSOCertButChainIncomplete_ReturnsFalse()
        {
            // screwing assertionModel because signing jwtString with SO private key is not an option
            // we won't expose private key to you! :)
            var jwtString = CreateJwtString();
            var assertionModel = new AssertionModel(
                new[]
                {
                    Constants.SchemeOwner.PublicKeyBase64Der,
                    Constants.TestCertificateAuthority.RootCaPublicKeyBase64Der
                },
                (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(jwtString),
                jwtString);
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);

            var result = _sut.IsValid(assertionModel);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_JwtSignedUsingSOCertButRootCertAndIntermediateCertAreSwitched_ReturnsFalse()
        {
            // screwing assertionModel because signing jwtString with SO private key is not an option
            // we won't expose private key to you! :)
            var jwtString = CreateJwtString();
            var assertionModel = new AssertionModel(
                new[]
                {
                    Constants.SchemeOwner.PublicKeyBase64Der,
                    Constants.TestCertificateAuthority.RootCaPublicKeyBase64Der,
                    Constants.TestCertificateAuthority.IntermediateCaPublicKeyBase64Der
                },
                (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(jwtString),
                jwtString);
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);

            var result = _sut.IsValid(assertionModel);

            result.Should().BeFalse();
        }

        private static AssertionModel CreateAssertionModel()
        {
            var jwtString = CreateJwtString();

            return new AssertionModel(
                new[] { Constants.AbcParty.PublicKeyBase64Der },
                CreateToken(jwtString),
                jwtString
            );
        }

        private static string CreateJwtString() =>
            JwtUtilities.Create(
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);

        private static JwtSecurityToken CreateToken(string jwtString = null) =>
            (JwtSecurityToken)new JwtSecurityTokenHandler().ReadToken(jwtString ?? CreateJwtString());
    }
}