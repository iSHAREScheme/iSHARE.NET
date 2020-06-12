using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Parties;
using iSHARE.Parties.Args;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iSHARE.IntegrationTests
{
    public class PartiesQueryServiceTests : TestsBase
    {
        private readonly IPartiesQueryService _sut;

        public PartiesQueryServiceTests()
        {
            _sut = ServiceScope.ServiceProvider.GetRequiredService<IPartiesQueryService>();
        }

        [Fact]
        public async Task SendsValidRequest_ExpectsParties()
        {
            var accessToken = await GetAccessTokenAsync();

            var response = await _sut.GetAsync(new PartiesRequestArgs(accessToken, "*"));

            response.Should().NotBeNull();
            response.Parties.Should().HaveCountGreaterThan(0);
        }
    }
}
