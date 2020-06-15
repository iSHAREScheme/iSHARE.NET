using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Capabilities;
using iSHARE.Capabilities.Args;
using iSHARE.Capabilities.Responses;
using iSHARE.Exceptions;
using iSHARE.IdentityProviders;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using iSHARE.Parties.Responses;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace iSHARE.Tests.IdentityProviders
{
    public class IdentityProvidersQueryServiceTests
    {
        private readonly Mock<IPartiesQueryService> _partiesQueryServiceMock;
        private readonly Mock<ICapabilitiesQueryService> _capabilitiesQueryServiceMock;
        private readonly IIdentityProvidersQueryService _sut;

        public IdentityProvidersQueryServiceTests()
        {
            var loggerMock = new Mock<ILogger<IdentityProvidersQueryService>>();
            _partiesQueryServiceMock = new Mock<IPartiesQueryService>();
            _capabilitiesQueryServiceMock = new Mock<ICapabilitiesQueryService>();

            _sut = new IdentityProvidersQueryService(
                _partiesQueryServiceMock.Object,
                _capabilitiesQueryServiceMock.Object,
                loggerMock.Object);
        }

        [Fact]
        public void GetAsync_SomethingThrows_Throws()
        {
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception());

            Func<Task> act = () => _sut.GetAsync("access token");

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Fact]
        public async Task GetAsync_ReturnsLessThan10Parties_DoesNotInvokePartiesQueryServiceAgain()
        {
            SetupPartyQueryServiceMock();

            await _sut.GetAsync("access token");

            _partiesQueryServiceMock.Verify(
                x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void GetAsync_ReturnsZeroIdps_ThrowsUnsuccessfulResponseException()
        {
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 1,
                    Parties = new[]
                    {
                        new Party
                        {
                            PartyName = "party name",
                            PartyId = "party id",
                            CapabilityUrl = "http://localhost.net/not-exists",
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddDays(-1),
                                EndDate = DateTime.UtcNow.AddDays(1),
                                Status = "Active"
                            },
                            Certifications = new[]
                            {
                                new Certification
                                {
                                    Role = "AuthorisationRegistry",
                                    StartDate = DateTime.UtcNow.AddDays(-1),
                                    EndDate = DateTime.UtcNow.AddDays(1),
                                }
                            }
                        }
                    }
                });

            Func<Task> act = () => _sut.GetAsync("access token");

            act.Should().Throw<UnsuccessfulResponseException>();
        }

        [Theory]
        [InlineData(11, 2)]
        [InlineData(20, 2)]
        [InlineData(10, 1)]
        [InlineData(25, 3)]
        [InlineData(30, 3)]
        public async Task GetAsync_ReturnsXParties_InvokesPartyQueryServiceYTimes(int parties, int invocationTimes)
        {
            SetupPartyQueryServiceMock(count: parties);

            await _sut.GetAsync("access token");

            _partiesQueryServiceMock.Verify(
                x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()),
                Times.Exactly(invocationTimes));
        }

        [Fact]
        public async Task GetAsync_ReturnsManyPartiesButOnlyOneOfThemIdp_ReturnsOneIdp()
        {
            SetupPartyQueryServiceMock(9);
            SetupCapabilitiesQueryServiceMock();

            var result = await _sut.GetAsync("access token");

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("party name");
            result.First().Eori.Should().Be("party id");
            result.First().BaseUri.Should().BeEquivalentTo(new Uri("http://localhost.com/idp/connect"));
        }

        [Fact]
        public async Task GetAsync_ReturnsManyPartiesButCapabilitiesThrowsForOneOfThem_HandlesExceptionAndReturnsOneIdp()
        {
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 2,
                    Parties = new[]
                    {
                        CreateValidParty("party id", "http://localhost.net/not-exists"),
                        CreateValidParty("party id2", "http://localhost.net/not-exists2"),
                    },
                });
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists", "party id");
            _capabilitiesQueryServiceMock
                .Setup(x => x.GetAsync(It.Is<CapabilitiesRequestArgs>(
                    args => args.RequestUri == "http://localhost.net/not-exists2"), It.IsAny<CancellationToken>()))
                .Throws(new UnsuccessfulResponseException("exception"));

            var result = await _sut.GetAsync("access token");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAsync_ReturnsOneParyWithoutCapabilitiesUrl_IgnoresItAndProceeds()
        {
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 3,
                    Parties = new[]
                    {
                        CreateValidParty("party id", "http://localhost.net/not-exists"),
                        CreateValidParty("party id2", null),
                        CreateValidParty("party id2", " ")
                    },
                });
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists", "party id");
            CreateCapabilitiesRequestMockForParty(null, "party id2");
            CreateCapabilitiesRequestMockForParty(" ", "party id3");

            var result = await _sut.GetAsync("access token");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAsync_ReturnsManyIdpsButCapabilitiesForThemAreIncorrect_HandlesExceptionsAndReturnsIdps()
        {
            var noNameParty = CreateValidParty("party id2", "http://localhost.net/not-exists2");
            noNameParty.PartyName = null;
            var noSupportedVersionsParty = CreateValidParty("party id3", "http://localhost.net/not-exists3");
            var noSupportedFeaturesParty = CreateValidParty("party id4", "http://localhost.net/not-exists4");
            var noPublicFeaturesParty = CreateValidParty("party id5", "http://localhost.net/not-exists5");
            var noAuthorizeEndpointParty = CreateValidParty("party id6", "http://localhost.net/not-exists6");
            var partyCertificationExpired = CreateValidParty("party id8", "http://localhost.net/not-exists7");
            partyCertificationExpired.Certifications.First().EndDate = DateTime.UtcNow.AddDays(-1);
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = 7,
                    Parties = new[]
                    {
                        CreateValidParty("party id", "http://localhost.net/not-exists"),
                        noNameParty,
                        noSupportedVersionsParty,
                        noSupportedFeaturesParty,
                        noPublicFeaturesParty,
                        noAuthorizeEndpointParty,
                        CreateValidParty("party id7", "http://localhost.net/not-exists7"),
                        partyCertificationExpired
                    },
                });
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists", "party id");
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists2", "party id2");
            _capabilitiesQueryServiceMock
                .Setup(x => x.GetAsync(
                    It.Is<CapabilitiesRequestArgs>(
                        c => c.RequestUri == "http://localhost.net/not-exists3"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CapabilitiesResponse
                {
                    PartyId = "party id3",
                    SupportedVersions = new List<SupportedVersion>(0)
                });
            CreateCapabilitiesRequestMockForParty(
                "http://localhost.net/not-exists4",
                "party id4",
                new SupportedVersion());
            CreateCapabilitiesRequestMockForParty(
                "http://localhost.net/not-exists5",
                "party id5",
                new SupportedVersion { SupportedFeatures = new SupportedFeature[0] });
            CreateCapabilitiesRequestMockForParty(
                "http://localhost.net/not-exists6",
                "party id6",
                new SupportedVersion
                {
                    SupportedFeatures = new[]
                    {
                        new SupportedFeature { Public = new[] { new FeatureObject { Url = new Uri("http://lol.com") }}}
                    }
                });
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists7", "party id7");
            CreateCapabilitiesRequestMockForParty("http://localhost.net/not-exists8", "party id8");

            var result = await _sut.GetAsync("access token");

            result.Should().HaveCount(2);
        }

        private void CreateCapabilitiesRequestMockForParty(
            string partyRequestUri,
            string partyId,
            SupportedVersion supportedVersion = null)
        {
            if (supportedVersion == null)
            {
                supportedVersion = new SupportedVersion
                {
                    SupportedFeatures = new[]
                    {
                        new SupportedFeature
                        {
                            Public = new[]
                            {
                                new FeatureObject
                                {
                                    Url = new Uri("http://localhost.com/authorize")
                                }
                            }
                        }
                    }
                };
            }

            _capabilitiesQueryServiceMock
                .Setup(x => x.GetAsync(
                    It.Is<CapabilitiesRequestArgs>(
                        c => c.RequestUri == partyRequestUri), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CapabilitiesResponse
                {
                    PartyId = partyId,
                    SupportedVersions = new[]
                    {
                        supportedVersion
                    }
                });
        }

        private static Party CreateValidParty(string partyId, string capabilityUrl)
        {
            return new Party
            {
                PartyName = "party name",
                PartyId = partyId,
                CapabilityUrl = capabilityUrl,
                Certifications = new[]
                {
                    new Certification
                    {
                        Role = "IdentityProvider",
                        StartDate = DateTime.UtcNow.AddDays(-1),
                        EndDate = DateTime.UtcNow.AddDays(1),
                    }
                }
            };
        }

        private void SetupPartyQueryServiceMock(int count = 1)
        {
            _partiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<PartiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartiesResponse
                {
                    Count = count,
                    Parties = new[]
                    {
                        new Party
                        {
                            PartyName = "party name",
                            PartyId = "party id",
                            CapabilityUrl = "http://localhost.net/not-exists",
                            Adherence = new Adherence
                            {
                                StartDate = DateTime.UtcNow.AddDays(-1),
                                EndDate = DateTime.UtcNow.AddDays(1),
                                Status = "Active"
                            },
                            Certifications = new[]
                            {
                                new Certification
                                {
                                    Role = "IdentityProvider",
                                    StartDate = DateTime.UtcNow.AddDays(-1),
                                    EndDate = DateTime.UtcNow.AddDays(1),
                                }
                            }
                        }
                    }
                });
        }

        private void SetupCapabilitiesQueryServiceMock()
        {
            _capabilitiesQueryServiceMock
                .Setup(x => x.GetAsync(It.IsAny<CapabilitiesRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CapabilitiesResponse
                {
                    PartyId = "party id",
                    SupportedVersions = new[]
                    {
                        new SupportedVersion
                        {
                            SupportedFeatures = new[]
                            {
                                new SupportedFeature
                                {
                                    Public = new[]
                                    {
                                        new FeatureObject
                                        {
                                            Url = new Uri("http://localhost.com/idp/connect/authorize")
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
        }
    }
}