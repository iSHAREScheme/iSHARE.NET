using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Exceptions;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Models;
using iSHARE.TokenValidator.SchemeOwner;
using iSHARE.TrustedList;
using Moq;
using Xunit;

namespace iSHARE.Tests.TrustedList
{
    public class TrustedListQueryServiceTests
    {
        private readonly Mock<IJwtTokenParser> _jwtTokenParserMock;
        private readonly Mock<ITokenResponseClient> _clientMock;
        private readonly Mock<ISchemeOwnerJwtTokenResponseValidator> _tokenResponseValidatorMock;
        private readonly ITrustedListQueryService _sut;

        public TrustedListQueryServiceTests()
        {
            var settings = new DefaultSettings(ConfigurationBuilder.Build());
            _jwtTokenParserMock = new Mock<IJwtTokenParser>();
            _clientMock = new Mock<ITokenResponseClient>();
            _tokenResponseValidatorMock = new Mock<ISchemeOwnerJwtTokenResponseValidator>();

            _sut = new TrustedListQueryService(
                settings,
                _clientMock.Object,
                _jwtTokenParserMock.Object,
                _tokenResponseValidatorMock.Object);
        }

        [Fact]
        public void GetAsync_RetrievedTokenIsInvalid_Throws()
        {
            _tokenResponseValidatorMock.Setup(x => x.IsValid(It.IsAny<AssertionModel>())).Returns(false);

            Func<Task> act = () => _sut.GetAsync("random");

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public void GetAsync_SomethingThrows_Throws()
        {
            _clientMock
                .Setup(x => x.SendRequestAsync(It.IsAny<TokenSendRequestArgs>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            Func<Task> act = () => _sut.GetAsync("random");

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public async Task GetAsync_SuccessfulRequest_ReturnsResponse()
        {
            _jwtTokenParserMock
                .Setup(x => x.Parse(It.IsAny<string>()))
                .Returns(CreateAssertionModel());
            _tokenResponseValidatorMock.Setup(x => x.IsValid(It.IsAny<AssertionModel>())).Returns(true);

            var result = await _sut.GetAsync("access.token");

            result.Should().HaveCount(7);
            var ca = result.First();
            ca.Subject.Should().Be("C=NL, O=Staat der Nederlanden, CN=TEST Staat der Nederlanden Organisatie Services CA - G3");
            ca.CertificateFingerprint.Should().Be("DC13FC94FF0149DE1B07F7965F655AED54C6A6BDA7ADF71A732FFCFABC454C7A");
            ca.Validity.Should().Be("valid");
            ca.Status.Should().Be("granted");
        }

        private static AssertionModel CreateAssertionModel()
        {
            const string jwtToken =
                "eyJ4NWMiOlsiTUlJRWlEQ0NBbkNnQXdJQkFnSUllRElyZG5ZbzJuZ3dEUVlKS29aSWh2Y05BUUVMQlFBd1NERVpNQmNHQTFVRUF" +
                "3d1FhVk5JUVZKRlZHVnpkRU5CWDFSTVV6RU5NQXNHQTFVRUN3d0VWR1Z6ZERFUE1BMEdBMVVFQ2d3R2FWTklRVkpGTVFzd0NRWU" +
                "RWUVFHRXdKT1REQWVGdzB4T1RBeU1UVXhNVFEyTlRoYUZ3MHlNVEF5TVRReE1UUTJOVGhhTUVreEhEQWFCZ05WQkFNTUUybFRTR" +
                "UZTUlNCVFkyaGxiV1VnVDNkdVpYSXhIREFhQmdOVkJBVVRFMFZWTGtWUFVra3VUa3d3TURBd01EQXdNREF4Q3pBSkJnTlZCQVlU" +
                "QWs1TU1JSUJJakFOQmdrcWhraUc5dzBCQVFFRkFBT0NBUThBTUlJQkNnS0NBUUVBMFJvb2hlUEwwMk52NEJaVEoza3A3bktzaHN" +
                "tanJjcjhNQmFBaFFwWlpBc2dUQWxtUWlDVFBtM2M4cVlQcU4rVHVnZ0ZXQ05uKzlXNTRDNVVHcXNJd3RYVGszWWV4QXdaNG9qUl" +
                "J0bzhsMUhQRFZBUzZXdlc3NEFDTlpsRWdHd2pyQ0d5MitNNVFQN083d0IwVDZvRkJvZlJ3SFpHemdidFNiU1FodXF3VXhmMEdaS" +
                "Th4QWwyL0dUSDI1VmZwOVQ3MUpFcG9aOWtzUDNDSWk1QkhrbGJUNUdLeEVPRmZkTU11cFg3bVduTlFiTHh1UXBBdEdDdW9yR2ZQ" +
                "RkU3RjVldkUxem9wd2NlQTVGc0UxTGFCUnF0K0VPcFBJbVNhalIwMmJjaEs5alM2bllFV3MvRlpHTHRKYWxsNUwzU25aTTZPaFd" +
                "4TStsS0d6Rkt3NVRJWE45RE13SURBUUFCbzNVd2N6QU1CZ05WSFJNQkFmOEVBakFBTUI4R0ExVWRJd1FZTUJhQUZCWTg1eURwMX" +
                "BUdkgrV2k4Ymo4dnVyZkxEZUJNQk1HQTFVZEpRUU1NQW9HQ0NzR0FRVUZCd01CTUIwR0ExVWREZ1FXQkJSZndpalQ3NWRJS1BsR" +
                "kMvQ3RSRHFVS1g5VE5qQU9CZ05WSFE4QkFmOEVCQU1DQmFBd0RRWUpLb1pJaHZjTkFRRUxCUUFEZ2dJQkFLNFBXVHEvZHF0Vm0r" +
                "NFdDZDFLUUo0dGorbjRjY0lBWUxETXFZU0JKc042UTJjdE1SQy8rK3lNL293UEhCcmlUendXL2pvQXBOUGVaaDFJVFRnU3phMzh" +
                "tM2h4b0RxMXV4NkhWR3lLNVFDUW9qRmRsZWM3dE9IbG1jYnV5VjRDRXlNWmJHK3lMbVZESTNxNTNWQVBnV3ZLSWkyUlVwc1BOdz" +
                "JsbzZINjZ2SE5wNWZpcEIvdEU0Q0RsYS9UYU41MWxOM2xYT3c0bHRiWmJ6YmQ2TXhJbEVDUWZKSDVlUHJpcGFrSmhuaVZrWnZRV" +
                "mthS0FlcFNYMGFEWUxPcFFRbmV0RFdab1ZKS0FzR0VMM0hMaWhxWXNEejcvQlQzRHdUMEtDNSs5OGdqR1p3dkx3ZXRKOTZLWFRB" +
                "RTUwMmYzak95UDdERDZ1SytKS2d2UVp5dkk1L0V1cDBUdE5sUmZKeThhZDhweCszOG9JeEdBbGJzS29XbXowb2FNR0MrbFZHTlA" +
                "yTTQ4TTdWa3RCVHB5bXF4Vnd0VGt2TVBqWldIS2xYdDJXMzNtTktHakpTOTVJNXZxT0tQV1NTc1dkSlJZSkNsbUVybWlkTlczMW" +
                "xXQVZQcjFpU1M0SlhEdllQTENQNDRhNGVkMEdhV1pSdi9iK0QyK1FVZ09iOFN6bWpQdmYvMkdOdHFXUmR5WXRYWjl2eDAzNGkrW" +
                "EYveWU4c2lOK1grd0ZIdFJ1bXRzd2Irc2NRZjRmVTZNaktCS1VzUERBUkFHakhqNXhIQkRqcTg0bmpHdVFjbDBoYlMzK1pTVjRO" +
                "cWtKSVVzMkxVdDlFdjFYN2FCNy85NUVYQWNnTlhkM0tQVm0zcDhORDc1QXFNMEZHUUVhUXlPd3FLY3FUQ2xKQ3VmR3NVWTRzN3J" +
                "XUiJdLCJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJFVS5FT1JJLk5MMDAwMDAwMDAwIiwic3ViIjoiRVUuRU9SS" +
                "S5OTDAwMDAwMDAwMCIsImp0aSI6ImE0NmQ2Zjg0MWEyZTRmNWNiZWExYjljODRmZDkyODBhIiwiaWF0IjoxNTg5Mjg0ODAzLCJl" +
                "eHAiOjE1ODkyODQ4MzMsImF1ZCI6IkVVLkVPUkkuTkwwMDAwMDAwMDEiLCJ0cnVzdGVkX2xpc3QiOlt7InN1YmplY3QiOiJDPU5" +
                "MLCBPPVN0YWF0IGRlciBOZWRlcmxhbmRlbiwgQ049VEVTVCBTdGFhdCBkZXIgTmVkZXJsYW5kZW4gT3JnYW5pc2F0aWUgU2Vydm" +
                "ljZXMgQ0EgLSBHMyIsImNlcnRpZmljYXRlX2ZpbmdlcnByaW50IjoiREMxM0ZDOTRGRjAxNDlERTFCMDdGNzk2NUY2NTVBRUQ1N" +
                "EM2QTZCREE3QURGNzFBNzMyRkZDRkFCQzQ1NEM3QSIsInZhbGlkaXR5IjoidmFsaWQiLCJzdGF0dXMiOiJncmFudGVkIn0seyJz" +
                "dWJqZWN0IjoiQz1OTCwgTz1pU0hBUkUgRm91bmRhdGlvbiwgQ049VEVTVCBpU0hBUkUgRm91bmRhdGlvbiBQS0lvdmVyaGVpZCB" +
                "PcmdhbmlzYXRpZSBTZXJ2ZXIgQ0EgLSBHMyIsImNlcnRpZmljYXRlX2ZpbmdlcnByaW50IjoiRjIxODEzM0NEM0FDMkQ5NzBEMT" +
                "BDQTQ2QkIwM0Y4MzI0NTMzMjRCMEY0QUY1QzNGNjFCQUQ2RkRFRUM1RUI4MyIsInZhbGlkaXR5IjoidmFsaWQiLCJzdGF0dXMiO" +
                "iJncmFudGVkIn0seyJzdWJqZWN0IjoiQz1OTCwgTz1URVNUIFN0YWF0IGRlciBOZWRlcmxhbmRlbiwgQ049VEVTVCBTdGFhdCBk" +
                "ZXIgTmVkZXJsYW5kZW4gUm9vdCBDQSAtIEczIiwiY2VydGlmaWNhdGVfZmluZ2VycHJpbnQiOiI5OEM5QzE0RjdGMUY5QTgzQTc" +
                "0NEUwQUNCQTlEQTZBNDdFRTk2RTA1M0Q3Mjc5NTQ1N0E1QkMwMjA3MjI5RDQzIiwidmFsaWRpdHkiOiJ2YWxpZCIsInN0YXR1cy" +
                "I6ImdyYW50ZWQifSx7InN1YmplY3QiOiJDTj1URVNUIGlTSEFSRSBGb3VuZGF0aW9uIGVJREFTIiwiY2VydGlmaWNhdGVfZmluZ" +
                "2VycHJpbnQiOiI4QzM5REQwNkUzNURFODQ2NzAwNEE1NDJEMENBNEI4RkRDN0Q2RjhGNzEzRjQwQTM1QkQ5RTY1OTM4QTE5MUNG" +
                "IiwidmFsaWRpdHkiOiJ2YWxpZCIsInN0YXR1cyI6ImdyYW50ZWQifSx7InN1YmplY3QiOiJDPU5MLCBPPWlTSEFSRSwgT1U9VGV" +
                "zdCwgQ049aVNIQVJFVGVzdENBIiwiY2VydGlmaWNhdGVfZmluZ2VycHJpbnQiOiJBNzhGREY3QkExM0JCRDk1QzYyMzY5NzJERD" +
                "AwM0ZBRTA3RjRFNDQ3Qjc5MUI2RUY2NzM3QUQyMkYwQjYxODYyIiwidmFsaWRpdHkiOiJ2YWxpZCIsInN0YXR1cyI6ImdyYW50Z" +
                "WQifSx7InN1YmplY3QiOiJDTj1URVNUIGlTSEFSRSBFVSBJc3N1aW5nIENlcnRpZmljYXRpb24gQXV0aG9yaXR5IEc1IiwiY2Vy" +
                "dGlmaWNhdGVfZmluZ2VycHJpbnQiOiJGRDU1OTNEQzg3NEVDQzExMzNDMjFBNzcyNTlDMzU5MjU1MkVDMEM4OURGQ0Q3QUIzQzB" +
                "CRENGRDczRjBGNUNDIiwidmFsaWRpdHkiOiJ2YWxpZCIsInN0YXR1cyI6ImdyYW50ZWQifSx7InN1YmplY3QiOiJDPU5MLCBPPW" +
                "lTSEFSRSwgT1U9VGVzdCwgQ049aVNIQVJFVGVzdENBX1RMUyIsImNlcnRpZmljYXRlX2ZpbmdlcnByaW50IjoiREYyRkY1MUQxQ" +
                "jI1NTlENjg2NzIzQzk3MDM3REM5RDVDNTg5NDA2Q0FDNEY4NEMyOUFCM0Q0M0UwMTI2MjUxRCIsInZhbGlkaXR5IjoidmFsaWQi" +
                "LCJzdGF0dXMiOiJncmFudGVkIn1dfQ.jELzMCTz85ojtgA78DAERj-ULBpiWvDm90x3TPNWyUC0Rv7Wjb31Cb7zm3Vmbz8dBn1N" +
                "kWBw52zF0a-DqhrbqxBi9sFTAoyoMBVy55YLRDqnj5HuPJJzS5HeDwXiZKg_kkJ8EiLrGgy-vPyudm_sHA4uk6zY_Bc-8niDfvA" +
                "eolnIs7t__IeNZvPMTjeykdCmaWTTm9tp1nx4-jFT5moYAiSvaDqkAhWW4HaBMs8sBkq91J8Tve9FYOy_TX6IoUzBpNhGgAFl7a" +
                "VQ1aCjb0XGfGtJMSHxxaApz8R-RGTe0xGJvRy_EwL6vjj0YSRLp1Ye0vAmH6l0L3sqgCKa7KotUQ";

            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            return new AssertionModel(new string [0], jwtSecurityToken, jwtToken);
        }
    }
}
