using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace iSHARE.Tests.TokenValidator
{
    public class JwtTokenResponseValidatorTests
    {
        private readonly Mock<IDecodedJwtValidator> _decodedJwtValidatorMock;
        private readonly Mock<IJwtCertificateValidator> _jwtCertificateValidatorMock;
        private readonly Mock<ILogger<JwtTokenResponseValidator>> _loggerMock;
        private readonly IJwtTokenResponseValidator _sut;

        public JwtTokenResponseValidatorTests()
        {
            _decodedJwtValidatorMock = new Mock<IDecodedJwtValidator>();
            _jwtCertificateValidatorMock = new Mock<IJwtCertificateValidator>();
            _loggerMock = new Mock<ILogger<JwtTokenResponseValidator>>();

            _sut = new JwtTokenResponseValidator(
                _decodedJwtValidatorMock.Object,
                _jwtCertificateValidatorMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task IsValidAsync_NotIShareCompliant_ReturnsFalse()
        {
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(false);

            var result = await _sut.IsValidAsync(null, "not_needed");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidAsync_SomethingThrows_ReturnsFalse()
        {
            SetupDecodedJwtValidatorMock();

            var result = await _sut.IsValidAsync(null, "not_needed");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsValidAsync_EverythingValid_ReturnsTrue()
        {
            const string accessToken = "this_time_needed";
            SetupDecodedJwtValidatorMock();
            _jwtCertificateValidatorMock
                .Setup(x => x.IsValidAsync(
                    It.IsAny<CertificateValidationArgs>(),
                    accessToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var args = new TokenValidationArgs(
                CreateValidAssertionModel(),
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = await _sut.IsValidAsync(args, accessToken, CancellationToken.None);

            result.Should().BeTrue();
        }

        private static AssertionModel CreateValidAssertionModel()
        {
            var jwtToken = CreateValidJwtToken();
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jwtToken);

            return new AssertionModel(new[] { Constants.AbcParty.PublicKeyBase64Der }, jwtSecurityToken, jwtToken);
        }

        private static string CreateValidJwtToken() =>
            JwtUtilities.Create(
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);

        private void SetupDecodedJwtValidatorMock()
        {
            _decodedJwtValidatorMock
                .Setup(x => x.IsIShareCompliant(It.IsAny<TokenValidationArgs>()))
                .Returns(true);
        }
    }
}
