using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Capabilities;
using iSHARE.Capabilities.Args;
using iSHARE.Capabilities.Responses;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Sdk;

namespace iSHARE.IntegrationTests
{
    public class CapabilitiesQueryServiceTests : TestsBase
    {
        private readonly ICapabilitiesQueryService _sut;

        public CapabilitiesQueryServiceTests()
        {
            _sut = ServiceScope.ServiceProvider.GetRequiredService<ICapabilitiesQueryService>();
        }

        [Fact]
        public async Task SendsValidRequest_AccessTokenProvided_ExpectsCapabilities()
        {
            var accessToken = await GetAccessTokenAsync();
            var args = new CapabilitiesRequestArgs(
                Constants.SchemeOwner.CapabilitiesRequestUri,
                Constants.SchemeOwner.ClientId,
                accessToken,
                accessToken);

            var response = await _sut.GetAsync(args);

            Assert(response);
        }

        [Fact]
        public async Task SendsValidRequest_AccessTokenNotProvided_ExpectsCapabilities()
        {
            var accessToken = await GetAccessTokenAsync();
            var args = new CapabilitiesRequestArgs(
                Constants.SchemeOwner.CapabilitiesRequestUri,
                Constants.SchemeOwner.ClientId,
                accessToken);

            var response = await _sut.GetAsync(args);

            Assert(response);
        }

        [Fact]
        public async Task SendsValidRequest_UsesNonSoParty_ExpectsCapabilities()
        {
            var schemeOwnerAccessToken = await GetAccessTokenAsync();
            var args = new CapabilitiesRequestArgs(
                Constants.AuthorizationRegistry.CapabilitiesRequestUri,
                Constants.AuthorizationRegistry.ClientId,
                schemeOwnerAccessToken);

            var response = await _sut.GetAsync(args);

            Assert(response);
            response.PartyId.Should().Be(Constants.AuthorizationRegistry.ClientId);
        }

        private static void Assert(CapabilitiesResponse response)
        {
            response.Should().NotBeNull();
            response.SupportedVersions.Should().NotBeNullOrEmpty();
            response.SupportedVersions.First().Version.Should().NotBeNullOrEmpty();
            response.SupportedVersions.First().SupportedFeatures.Should().NotBeNullOrEmpty();
            var feature = response.SupportedVersions.First().SupportedFeatures.First();
            feature.Should().NotBeNull();
            feature.Public.Should().NotBeNullOrEmpty();
            feature.Public.First().Should().NotBeNull();
            feature.Public.FirstOrDefault(x => x.Url.AbsoluteUri.Contains("capabilities")).Should().NotBeNull();
        }
    }
}
