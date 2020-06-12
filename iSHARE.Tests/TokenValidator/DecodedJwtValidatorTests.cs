using System;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace iSHARE.Tests.TokenValidator
{
    public class DecodedJwtValidatorTests
    {
        private readonly IDecodedJwtValidator _sut;

        public DecodedJwtValidatorTests()
        {
            var loggerMock = new Mock<ILogger<DecodedJwtValidator>>();

            _sut = new DecodedJwtValidator(loggerMock.Object);
        }

        [Fact]
        public void IsIShareCompliant_KeysNotFound_ReturnsFalse()
        {
            var assertionModel = new AssertionModel(new string[0], null, null);
            var args = new TokenValidationArgs(assertionModel, "issuer", "audience");

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void
            IsIShareCompliant_JwtSecurityTokenHandlerValidationFailsDueToInvalidAudienceAndIssuerParams_ReturnsFalse()
        {
            var assertionModel = CreateValidAssertionModel();
            var args = new TokenValidationArgs(assertionModel,
                Constants.AbcParty.ClientId,
                Constants.SchemeOwner.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse("JwtSecurityTokenHandler.ValidateToken throws exception which is handled.");
        }

        [Fact]
        public void IsIShareCompliant_AssertionModelContainsDifferentJwtSecurityTokenThanItsString_ReturnsFalse()
        {
            var jwtToken = CreateValidJwtToken();
            var handler = new JwtSecurityTokenHandler();
            var jwtSecurityToken = handler.ReadJwtToken(jwtToken);
            var assertionModel = new AssertionModel(
                new[] { Constants.AbcParty.PublicKeyBase64Der },
                jwtSecurityToken,
                CreateValidJwtToken());
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);
            
            const string reason =
                "JWT is valid because assertion model contains its public keys which were extracted " +
                "from JwtSecurityToken. Those public keys applies for JwtToken (string). However, " +
                "those public keys were not extracted from the token which is being validated, so " +
                "validation should fail.";
            result.Should().BeFalse(reason);
        }

        [Fact]
        public void IsIShareCompliant_HeaderHasNoAlg_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_HeaderAlgNeqRS256_ReturnsFalse()
        {
            var signingAlgorithm = JwtUtilities.CreateSigningCredentials(
                Constants.AbcParty.PrivateKey,
                SecurityAlgorithms.RsaSsaPssSha512);
            var jwtSecurityToken = JwtUtilities.CreateToken(
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId,
                Constants.AbcParty.PublicKeyBase64Der,
                signingAlgorithm);
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse("Only RS256 should be supported.");
        }

        [Fact]
        public void IsIShareCompliant_HeaderHasInvalidTyp_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWE"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_HeaderHasNoTyp_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadIssNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadIssAndSubDiffers_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.AbcParty.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadSubNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadAudNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadInvalidAud_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.AbcParty.ClientId}"",
                    ""sub"": ""{Constants.AbcParty.ClientId}"",
                    ""aud"": ""{Constants.SchemeOwner.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadJtiNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadIatNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadIatIssuedAfterNow_ReturnsFalse()
        {
            var now = DateTime.UtcNow.AddDays(1);
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(30))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(29)]
        [InlineData(31)]
        public void IsIShareCompliant_PayloadExpExpiresNotAfter30Secs_ReturnsFalse(int expiresIn)
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""exp"": ""{JwtUtilities.DateTimeToEpoch(now.AddSeconds(expiresIn))}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_PayloadExpNotExists_ReturnsFalse()
        {
            var now = DateTime.UtcNow;
            var jwtContent =
                $@"{{
                    ""alg"": ""RS256"",
                    ""typ"": ""JWT"",
                    ""x5c"": [""{ Constants.AbcParty.PublicKeyBase64Der }""]
                }}
                .
                {{
                    ""iss"": ""{Constants.SchemeOwner.ClientId}"",
                    ""sub"": ""{Constants.SchemeOwner.ClientId}"",
                    ""aud"": ""{Constants.AbcParty.ClientId}"",
                    ""jti"": ""{Guid.NewGuid()}"",
                    ""iat"": ""{JwtUtilities.DateTimeToEpoch(now)}""
                }}";
            var jwtToken = JwtUtilities.SignJwt(jwtContent, Constants.AbcParty.PrivateKey);
            var assertionModel = CreateValidAssertionModel(jwtToken);
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeFalse();
        }

        [Fact]
        public void IsIShareCompliant_ValidClientAssertionPassed_ReturnsTrue()
        {
            var assertionModel = CreateValidAssertionModel();
            var args = new TokenValidationArgs(
                assertionModel,
                Constants.SchemeOwner.ClientId,
                Constants.AbcParty.ClientId);

            var result = _sut.IsIShareCompliant(args);

            result.Should().BeTrue();
        }

        private static AssertionModel CreateValidAssertionModel(string jwtToken = null)
        {
            jwtToken ??= CreateValidJwtToken();
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
    }
}