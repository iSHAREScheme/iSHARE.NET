using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using FluentAssertions;
using iSHARE.Capabilities.Responses;
using iSHARE.TrustedList.Responses;
using Xunit;

namespace iSHARE.Tests
{
    public class TokenConvertTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void DeserializeClaim_InvalidClaimName_Throws(string claimName)
        {
            var jwtToken = new JwtSecurityToken();

            Action act = () => TokenConvert.DeserializeClaim<dynamic>(jwtToken, claimName);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DeserializeClaim_NullJwtSecurityToken_Throws()
        {
            JwtSecurityToken jwtToken = null;

            Action act = () => TokenConvert.DeserializeClaim<dynamic>(jwtToken, "not_null");

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DeserializeClaim_JwtSecurityTokenDoesNotContainProvidedClaim_Throws()
        {
            var jwtToken = CreateCapabilitiesToken();

            Action act = () => TokenConvert.DeserializeClaim<dynamic>(jwtToken, "invalid");

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void DeserializeClaim_ValidClaimButInvalidType_ReturnsObjectWithNulls()
        {
            var jwtToken = CreateCapabilitiesToken();

            var result = TokenConvert.DeserializeClaim<CertificateAuthority>(jwtToken, "capabilities_info");

            result.Should().NotBeNull();
            result.Subject.Should().BeNull();
            result.CertificateFingerprint.Should().BeNull();
            result.Validity.Should().BeNull();
            result.Status.Should().BeNull();
        }

        [Fact]
        public void DeserializeClaim_ValidClaimAndValidType_ReturnsValidObject()
        {
            var jwtToken = CreateCapabilitiesToken();

            var result = TokenConvert.DeserializeClaim<CapabilitiesResponse>(jwtToken, "capabilities_info");

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
        public void DeserializeClaim_ClaimWhichContainsArrayPayload_ReturnsValidObject()
        {
            var jwtToken = CreateTrustedListToken();

            var result = TokenConvert.DeserializeClaim<CertificateAuthority[]>(jwtToken, "trusted_list");

            result.Should().HaveCount(7);
            var ca = result.First();
            ca.Subject.Should().Be("C=NL, O=Staat der Nederlanden, CN=TEST Staat der Nederlanden Organisatie Services CA - G3");
            ca.CertificateFingerprint.Should().Be("DC13FC94FF0149DE1B07F7965F655AED54C6A6BDA7ADF71A732FFCFABC454C7A");
            ca.Validity.Should().Be("valid");
            ca.Status.Should().Be("granted");
        }

        private static JwtSecurityToken CreateTrustedListToken()
        {
            const string trustedListToken =
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

            return new JwtSecurityTokenHandler().ReadJwtToken(trustedListToken);
        }

        private static JwtSecurityToken CreateCapabilitiesToken()
        {
            const string capabilitiesToken =
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

            return new JwtSecurityTokenHandler().ReadJwtToken(capabilitiesToken);
        }
    }
}
