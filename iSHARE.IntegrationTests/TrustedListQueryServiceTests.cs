using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.TrustedList;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iSHARE.IntegrationTests
{
    public class TrustedListQueryServiceTests : TestsBase
    {
        private readonly ITrustedListQueryService _sut;

        public TrustedListQueryServiceTests()
        {
            _sut = ServiceScope.ServiceProvider.GetRequiredService<ITrustedListQueryService>();
        }

        [Fact]
        public async Task SendsValidRequest_ExpectsTrustedList()
        {
            var accessToken = await GetAccessTokenAsync();

            var trustedList = await _sut.GetAsync(accessToken);

            trustedList.Should().NotBeNull();
            trustedList.Should().HaveCountGreaterThan(0);
        }
    }
}
