using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.IdentityProviders;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iSHARE.IntegrationTests
{
    public class IdentityProvidersQueryServiceTests : TestsBase
    {
        private readonly IIdentityProvidersQueryService _sut;

        public IdentityProvidersQueryServiceTests()
        {
            _sut = ServiceScope.ServiceProvider.GetRequiredService<IIdentityProvidersQueryService>();
        }

        [Fact]
        public async Task SendsValidRequest_ExpectsParties()
        {
            var accessToken = await GetAccessTokenAsync();

            var response = await _sut.GetAsync(accessToken);

            response.Should().NotBeNull();
            response.Count.Should().BeGreaterThan(0);
        }
    }
}
