using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Exceptions;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;
using iSHARE.TokenValidator;
using iSHARE.TokenValidator.Models;
using iSHARE.TokenValidator.SchemeOwner;
using Moq;
using Xunit;

namespace iSHARE.Tests.Parties
{
    public class PartiesQueryServiceTests
    {
        private readonly Mock<ITokenResponseClient> _clientMock;
        private readonly Mock<ISchemeOwnerJwtTokenResponseValidator> _tokenResponseValidatorMock;
        private readonly IPartiesQueryService _sut;

        public PartiesQueryServiceTests()
        {
            var settingsMock = CreateSettingsMock();
            var jwtTokenParserMock = new Mock<IJwtTokenParser>();
            _clientMock = new Mock<ITokenResponseClient>();
            _tokenResponseValidatorMock = new Mock<ISchemeOwnerJwtTokenResponseValidator>();

            jwtTokenParserMock.Setup(x => x.Parse(It.IsAny<string>())).Returns(CreateAssertionModel());
            _tokenResponseValidatorMock.Setup(x => x.IsValid(It.IsAny<AssertionModel>())).Returns(true);

            _sut = new PartiesQueryService(
                settingsMock.Object,
                _clientMock.Object,
                jwtTokenParserMock.Object,
                _tokenResponseValidatorMock.Object);
        }

        [Fact]
        public async Task GetAsync_AllArgumentsExist_MapsAndInvokesClient()
        {
            var args = new PartiesRequestArgs(
                "accessToken",
                "name",
                "eori",
                certifiedOnly: true,
                activeOnly: true,
                "certificateSubjectName",
                page: 10,
                new DateTime(2020, 12, 11, 10, 9, 8));

            await _sut.GetAsync(args);

            _clientMock.Verify(
                x => x.SendRequestAsync(
                    It.Is<TokenSendRequestArgs>(x =>
                        x.AccessToken == "accessToken" &&
                        x.RequestUri == "https://scheme.isharetest.net/parties" &&
                        x.Parameters["name"] == "name" &&
                        x.Parameters["eori"] == "eori" &&
                        x.Parameters["certified_only"] == "true" &&
                        x.Parameters["active_only"] == "true" &&
                        x.Parameters["certificate_subject_name"] == "certificateSubjectName" &&
                        x.Parameters["page"] == "10" &&
                        x.Parameters["date_time"] == "2020-12-11T10:09:08Z"),
                    It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task GetAsync_ArgumentsDoNotExist_MapsAndInvokesClient()
        {
            var args = new PartiesRequestArgs("accessToken", "*");

            await _sut.GetAsync(args);

            _clientMock.Verify(
                x => x.SendRequestAsync(
                    It.Is<TokenSendRequestArgs>(x =>
                        x.AccessToken == "accessToken" &&
                        x.RequestUri == "https://scheme.isharetest.net/parties" &&
                        x.Parameters.Count == 1),
                    It.IsAny<CancellationToken>()));
        }

        [Fact]
        public void GetAsync_RetrievedTokenIsInvalid_Throws()
        {
            var args = new PartiesRequestArgs("accessToken", "*");
            _tokenResponseValidatorMock.Setup(x => x.IsValid(It.IsAny<AssertionModel>())).Returns(false);

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public void GetAsync_SomethingThrows_Throws()
        {
            var args = new PartiesRequestArgs("accessToken", "*");
            _clientMock
                .Setup(x => x.SendRequestAsync(It.IsAny<TokenSendRequestArgs>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public async Task GetAsync_SuccessfulRequest_ReturnsResponse()
        {
            var args = new PartiesRequestArgs("accessToken", "*");
            
            var result = await _sut.GetAsync(args);

            result.Count.Should().Be(1);
            var party = result.Parties.First();
            party.PartyId.Should().Be("EU.EORI.NL000000004");
            party.PartyName.Should().Be("AskMeAnything Authorization Registry");
            party.CapabilityUrl.Should().Be("https://ar.isharetest.net/capabilities");
            party.Adherence.Status.Should().Be("Active");
            party.Adherence.StartDate.Should().Be(new DateTime(2018, 04, 26));
            party.Adherence.EndDate.Should().Be(new DateTime(2020, 07, 25));
            party.Certifications.Should().HaveCount(1);
            var certification = party.Certifications.First();
            certification.StartDate.Should().Be(new DateTime(2018, 01, 04));
            certification.EndDate.Should().Be(new DateTime(2020, 02, 02));
            certification.Role.Should().Be("AuthorisationRegistry");
            certification.LoA.Should().Be(LevelOfAssurance.High);
        }

        private static Mock<IShareSettings> CreateSettingsMock()
        {
            var settingsMock = new Mock<IShareSettings>();
            settingsMock.Setup(x => x.SchemeOwnerUrl).Returns("https://scheme.isharetest.net");
            return settingsMock;
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
                "S5OTDAwMDAwMDAwMCIsImp0aSI6IjU4MDc2YmFhY2Q5NjQwOThiYmU5MzhiM2JkMzZmNjRlIiwiaWF0IjoxNTg5MjgxMTY4LCJl" +
                "eHAiOjE1ODkyODExOTgsImF1ZCI6IkVVLkVPUkkuTkwwMDAwMDAwMDEiLCJwYXJ0aWVzX2luZm8iOnsiY291bnQiOjEsImRhdGE" +
                "iOlt7InBhcnR5X2lkIjoiRVUuRU9SSS5OTDAwMDAwMDAwNCIsInBhcnR5X25hbWUiOiJBc2tNZUFueXRoaW5nIEF1dGhvcml6YX" +
                "Rpb24gUmVnaXN0cnkiLCJhZGhlcmVuY2UiOnsic3RhdHVzIjoiQWN0aXZlIiwic3RhcnRfZGF0ZSI6IjIwMTgtMDQtMjZUMDA6M" +
                "DA6MDAiLCJlbmRfZGF0ZSI6IjIwMjAtMDctMjVUMDA6MDA6MDAifSwiY2VydGlmaWNhdGlvbnMiOlt7InJvbGUiOiJBdXRob3Jp" +
                "c2F0aW9uUmVnaXN0cnkiLCJzdGFydF9kYXRlIjoiMjAxOC0wMS0wNFQwMDowMDowMCIsImVuZF9kYXRlIjoiMjAyMC0wMi0wMlQ" +
                "wMDowMDowMCIsImxvYSI6M31dLCJjYXBhYmlsaXR5X3VybCI6Imh0dHBzOi8vYXIuaXNoYXJldGVzdC5uZXQvY2FwYWJpbGl0aW" +
                "VzIn1dfX0.J1iDraepCz3VYIrRZ10u90uOqqj7taUKTwOBvFZIik5M4ea47xklRPfqJJcm5FfH2fZ5vQbWJ7XRjwIldPMElo1wK" +
                "78jCvdO95hkoTWZo5pn9zbwKbYwDOdQYEGTT5pw_Ij8B63KHLfIrj0BMdiaz9QeZ2g5ZlIoQUGvF2FLPKz8wgM1w1CRxNSaiZ6u" +
                "PJKP_5JUKbgYqLWjQ8v54O5GRspaCVNFyDoyY3TvOmQNEDFKlfjR1yeTKtZ2zA7AQX486QxgmezqLQV4aUanEvdD6kCWZh4dTjP" +
                "NZk41a4jcKccmMH93099NgrZTp27Zg8i3BMOUrKLVPLy8rAjieDen3Q";

            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            return new AssertionModel(new string[0], jwtSecurityToken, jwtToken);
        }
    }
}