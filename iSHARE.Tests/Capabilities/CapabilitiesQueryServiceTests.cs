using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Capabilities;
using iSHARE.Capabilities.Args;
using iSHARE.Exceptions;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using Moq;
using Xunit;

namespace iSHARE.Tests.Capabilities
{
    public class CapabilitiesQueryServiceTests
    {
        private readonly Mock<ITokenResponseClient> _clientMock;
        private readonly Mock<IJwtTokenParser> _jwtTokenParserMock;
        private readonly Mock<IJwtTokenResponseValidator> _tokenResponseValidatorMock;
        private readonly ICapabilitiesQueryService _sut;

        public CapabilitiesQueryServiceTests()
        {
            var settings = new DefaultSettings(ConfigurationBuilder.Build());
            _clientMock = new Mock<ITokenResponseClient>();
            _jwtTokenParserMock = new Mock<IJwtTokenParser>();
            _tokenResponseValidatorMock = new Mock<IJwtTokenResponseValidator>();

            _sut = new CapabilitiesQueryService(
                _clientMock.Object,
                _jwtTokenParserMock.Object,
                _tokenResponseValidatorMock.Object,
                settings);
        }

        [Fact]
        public void GetAsync_RetrievedTokenIsInvalid_Throws()
        {
            _jwtTokenParserMock
                .Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(CreateAssertionModel());
            _tokenResponseValidatorMock
                .Setup(x => x.IsValidAsync(It.IsAny<TokenValidationArgs>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var args = CreateArgs();

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public void GetAsync_SomethingThrows_Throws()
        {
            _clientMock
                .Setup(x => x.SendRequestAsync(It.IsAny<TokenSendRequestArgs>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());
            var args = CreateArgs();

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public async Task GetAsync_SuccessfulRequest_ReturnsResponse()
        {
            _jwtTokenParserMock
                .Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(CreateAssertionModel());
            _tokenResponseValidatorMock
                .Setup(x => x.IsValidAsync(It.IsAny<TokenValidationArgs>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var args = CreateArgs();

            var result = await _sut.GetAsync(args);

            result.Should().NotBeNull();
            result.PartyId.Should().Be("EU.EORI.NL000000003");
            result.Roles.Should().HaveCount(1);
            result.SupportedVersions.Should().NotBeNull();
            result.SupportedVersions.Should().HaveCount(1);
            result.SupportedVersions.First().Version.Should().Be("1.7");
            result.SupportedVersions.First().SupportedFeatures.First().Public.Should().HaveCount(4);
            var firstPublicFeature = result.SupportedVersions.First().SupportedFeatures.First().Public.First();
            firstPublicFeature.Id.Should().Be("A51D413F-B3CC-477D-96C4-E37A9003BFE3");
            firstPublicFeature.Feature.Should().Be("capabilities");
            firstPublicFeature.Description.Should().Be("Retrieves iSHARE capabilities");
            firstPublicFeature.Url.Should().Be("https://w13.isharetest.net/capabilities");
            firstPublicFeature.TokenEndpoint.Should().Be("https://w13.isharetest.net/connect/token");
            result.SupportedVersions.First().SupportedFeatures.First().Restricted.Should().HaveCount(0);
        }

        [Fact]
        public async Task GetAsync_AccessTokenNotProvided_DoesNotUseAudienceForValidation()
        {
            _jwtTokenParserMock
                .Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(CreateAssertionModel());
            _tokenResponseValidatorMock
                .Setup(x => x.IsValidAsync(It.IsAny<TokenValidationArgs>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var args = CreateArgs();

            await _sut.GetAsync(args);

            _tokenResponseValidatorMock
                .Verify(x => x.IsValidAsync(It.Is<TokenValidationArgs>(
                    t => t.Audience == null), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task GetAsync_AccessTokenProvided_UsesAudienceForValidation()
        {
            _jwtTokenParserMock
                .Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(CreateAssertionModel());
            _tokenResponseValidatorMock
                .Setup(x => x.IsValidAsync(It.IsAny<TokenValidationArgs>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var args = new CapabilitiesRequestArgs("someUri", "eori", "token", "anotherOne");

            await _sut.GetAsync(args);

            _tokenResponseValidatorMock
                .Verify(x => x.IsValidAsync(It.Is<TokenValidationArgs>(
                    t => t.Audience == "EU.EORI.NL000000001"), It.IsAny<string>(), It.IsAny<CancellationToken>()));
        }

        private static CapabilitiesRequestArgs CreateArgs() => new CapabilitiesRequestArgs("someUri", "eori", "token");

        private static AssertionModel CreateAssertionModel()
        {
            const string jwtToken =
                "eyJ4NWMiOlsiTUlJRWdUQ0NBbW1nQXdJQkFnSUlTOTBLKzFROUhPa3dEUVlKS29aSWh2Y05BUUVMQlFBd1NERVpNQmNHQTFVRUF" +
                "3d1FhVk5JUVZKRlZHVnpkRU5CWDFSTVV6RU5NQXNHQTFVRUN3d0VWR1Z6ZERFUE1BMEdBMVVFQ2d3R2FWTklRVkpGTVFzd0NRWU" +
                "RWUVFHRXdKT1REQWVGdzB4T1RBeU1UVXhNVFEzTVRWYUZ3MHlNVEF5TVRReE1UUTNNVFZhTUVJeEZUQVRCZ05WQkFNTURGZGhjb" +
                "VZvYjNWelpTQXhNekVjTUJvR0ExVUVCUk1UUlZVdVJVOVNTUzVPVERBd01EQXdNREF3TXpFTE1Ba0dBMVVFQmhNQ1Rrd3dnZ0Vp" +
                "TUEwR0NTcUdTSWIzRFFFQkFRVUFBNElCRHdBd2dnRUtBb0lCQVFDd2FEQXpFSjI4REtURmRQUHU5b0o0SnQ2NHBLeEFIc1AzRkh" +
                "MeVk1N1VpcGRCOW1vMTU3eGY5MXo1dlpNY05NdXFVdWF1OE9IVHpseWtQQTl0Q0VPb2NFMERZd1I3RzBFM1F6bnE5VjhVRy9oY2" +
                "42eGVxY1J2NTV3RS9sZDdFUVV1SnhZaHJ1VVVwNnc4S3lYQnZWdHRkVzhFMnlxa3lDVmFCRTUwRHNzaFlxazdnaVFDSExtVlhSS" +
                "m44dDZDdW52dGlIdHVuTzBaM1hlM0U1TVJma0NqTm5jajdPTVRNQ1h2OHVDTlNkOE4ydGV1YmhUM2dBQnpXMlBqdTJFeU54SmV6" +
                "eWRTSXU5eTNYa3VnZUNKSUdIRlk4TEtSZXU5YUV6dDhhTC9NeTdaOENKaklWRzFjRDBmREZ0NEpaeHVZd1c4RC9uamw0Q21PTER" +
                "hR2VwZ0dHd2ZBZ01CQUFHamRUQnpNQXdHQTFVZEV3RUIvd1FDTUFBd0h3WURWUjBqQkJnd0ZvQVVGanpuSU9uV2xPOGY1YUx4dV" +
                "B5KzZ0OHNONEV3RXdZRFZSMGxCQXd3Q2dZSUt3WUJCUVVIQXdFd0hRWURWUjBPQkJZRUZHYm92VjdGTE01MkZjYStZRTJkRXFvK" +
                "0tveHFNQTRHQTFVZER3RUIvd1FFQXdJRm9EQU5CZ2txaGtpRzl3MEJBUXNGQUFPQ0FnRUFKdHZFMmlBYXB1Rzh5cm9WeWVpNEd2" +
                "dGpmTStJT3RoczFWd2ozWjR0TnRXalc4VTFZcXFnd2ZBSThnUHVDekk2SEdNRDBycnZ6SVM2dGc0cGVUekRFZFk2VmZDNGx0bWN" +
                "TNWNwaFlZcXM2WDB3aWZUcDRWVmJQdCs2R3hwTWU5b01CVGNBR1JIVGF4VzJmYVBzclFtZkRuaGNoSEZsU2FqaHF4RDVnUi9GWX" +
                "ZYZUFHc29QMEJCcW1SUkRiUnR5ZFB4Qnd1WVFyVTdUSmQraXJQdmhDVm9BMENPQ0FZM2loU3N5UHBuK0xDYnJsQ2JkNVNGRHBIO" +
                "WFXdHQ2MmlqQ3M4TWRETk1QMmw1bGJ3clFCN3YxYnNGelkxU3hTQ0QxRTh5bVhrQlFSaENCY2cwbWszRXNTVmpkL0dCWG52Q3RC" +
                "NzdlVUY4MGdMMUR4VzJwT2huTU5DdDMwcTFnemQvajJ0bWNLb0tvR3puUFFDL3NkNWs0c29tLytIdE9ZUWJTbVliQW5LcmFJVWd" +
                "qUmQzYTN1UzFYQkZHWjMvem9JTXNuMXFOdnI1cENPRkJWcXlLVEp0S1ZjRzcvZi9ncXgxdGplOGVzV0c4bjZReUlPWjBXbWEzVn" +
                "BaVnFEZGFYRHIrSi9rQ2FZcC9IbUZ2RjhDVkNDQlJSV082ajR6UXh0dXJxL1ZhZUxMOCtJYjlTb1FvRld5UnBMbkZpeXU3em81c" +
                "UxBNlg1UThTY3owL0pONElOcW5EaFBMYWRrL0p3ZEF0RC80eFBZVlZUYUFYc2hlQmtlKy85QS9nZ0tJakpoazlhUEV2OE1KcnhD" +
                "a0F1Zm1iVS9pVFVYYXVpUGhjaGM4RmEwZ05IV0RrOW1nMDcyek5HRW1hUlNQSjJQZm52dzNuTXhDanpleDVOdzB0ND0iXSwiYWx" +
                "nIjoiUlMyNTYiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJFVS5FT1JJLk5MMDAwMDAwMDAzIiwic3ViIjoiRVUuRU9SSS5OTDAwMDA" +
                "wMDAwMyIsImp0aSI6IjVjZDk0YmU0NWJjYjQ1M2FiOTg3YWNmNjMzNWFiYTVmIiwiaWF0IjoxNTg4NzcyMzA5LCJleHAiOjE1OD" +
                "g3NzIzMzksImF1ZCI6IkVVLkVPUkkuTkwwMDAwMDAwMDEiLCJjYXBhYmlsaXRpZXNfaW5mbyI6eyJwYXJ0eV9pZCI6IkVVLkVPU" +
                "kkuTkwwMDAwMDAwMDMiLCJpc2hhcmVfcm9sZXMiOlt7InJvbGUiOiJTZXJ2aWNlIFByb3ZpZGVyIn1dLCJzdXBwb3J0ZWRfdmVy" +
                "c2lvbnMiOlt7InZlcnNpb24iOiIxLjciLCJzdXBwb3J0ZWRfZmVhdHVyZXMiOlt7InB1YmxpYyI6W3siaWQiOiJBNTFENDEzRi1" +
                "CM0NDLTQ3N0QtOTZDNC1FMzdBOTAwM0JGRTMiLCJmZWF0dXJlIjoiY2FwYWJpbGl0aWVzIiwiZGVzY3JpcHRpb24iOiJSZXRyaW" +
                "V2ZXMgaVNIQVJFIGNhcGFiaWxpdGllcyIsInVybCI6Imh0dHBzOi8vdzEzLmlzaGFyZXRlc3QubmV0L2NhcGFiaWxpdGllcyIsI" +
                "nRva2VuX2VuZHBvaW50IjoiaHR0cHM6Ly93MTMuaXNoYXJldGVzdC5uZXQvY29ubmVjdC90b2tlbiJ9LHsiaWQiOiI0OUY2RTY2" +
                "Mi1GMDU1LTRBQUMtOTZCMi1FODMzRkE1RjU0MTQiLCJmZWF0dXJlIjoiYWNjZXNzIHRva2VuIiwiZGVzY3JpcHRpb24iOiJPYnR" +
                "haW5zIGFjY2VzcyB0b2tlbiIsInVybCI6Imh0dHBzOi8vdzEzLmlzaGFyZXRlc3QubmV0L2Nvbm5lY3QvdG9rZW4ifSx7ImlkIj" +
                "oiMDUzNTdCMUMtQTkzNC00QkIyLUE3Q0QtNDI5NDhEQTUyMzc5IiwiZmVhdHVyZSI6ImJvb20gYWNjZXNzIiwiZGVzY3JpcHRpb" +
                "24iOiJSZXF1ZXN0IGJvb20gYWNjZXNzIGJhc2VkIG9uIHVzZXIgaW5mb3JtYXRpb24iLCJ1cmwiOiJodHRwczovL3cxMy5pc2hh" +
                "cmV0ZXN0Lm5ldC9ib29tX2FjY2VzcyIsInRva2VuX2VuZHBvaW50IjoiaHR0cHM6Ly93MTMuaXNoYXJldGVzdC5uZXQvY29ubmV" +
                "jdC90b2tlbiJ9LHsiaWQiOiIxMDVEMTlDNy0wMkIxLTQ4MUYtOEI5OC0wQzBGMkY1RUJCNEIiLCJmZWF0dXJlIjoicmV0dXJuIG" +
                "NsaWVudCBpbmZvcm1hdGlvbiIsImRlc2NyaXB0aW9uIjoiRGlzcGxheXMgaWRlbnRpdHkgb2YgY2xpZW50IHRvIHdoaWNoIGFjY" +
                "2VzcyB0b2tlbiB3YXMgaXNzdWVkIiwidXJsIjoiaHR0cHM6Ly93MTMuaXNoYXJldGVzdC5uZXQvbWUiLCJ0b2tlbl9lbmRwb2lu" +
                "dCI6Imh0dHBzOi8vdzEzLmlzaGFyZXRlc3QubmV0L2Nvbm5lY3QvdG9rZW4ifV0sInJlc3RyaWN0ZWQiOltdfV19XX19.Gyn7iy" +
                "Qivi-j7fz0h5RvWPppUckROizuB57FAMDn6uydHuKF1nt3XKPUoKruuBWevfVpxR7uqJDNZ5ypYarFW5h16PlA_iA2WLaD72L6a" +
                "vfCbEYrxSUC-9I2uTXfv7HiCWASwA6RXBJgFFqH-htrrmhNmmhZtkOVo3I8ZtN8fmt3WW4kxi3yki_vTAZ504x5EotXw8v7ZVNf" +
                "oeHRrHx2ToOCXRkVZ12Y3kVhLGsJ-W33BYLpKlSu-CgKYv_GPsCtbGFZW6A9EmrGQMHZue-fhdD3bHSQpSQZWtmqCPShkel93jc" +
                "hNTQRoj1exW_7RJgPJmVMUlUyT77uz0Y7te9pyg";

            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            return new AssertionModel(new string[0], jwtSecurityToken, jwtToken);
        }
    }
}
