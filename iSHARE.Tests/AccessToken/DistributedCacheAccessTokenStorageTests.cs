using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.AccessToken;
using iSHARE.AccessToken.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace iSHARE.Tests.AccessToken
{
    public class DistributedCacheAccessTokenStorageTests
    {
        private readonly IAccessTokenStorage _sut;

        public DistributedCacheAccessTokenStorageTests()
        {
            _sut = new DistributedCacheAccessTokenStorage(CreateInMemoryDistributedCache());
        }

        [Fact]
        public async Task AddAsync_ContainsDuplicate_Overwrites()
        {
            await _sut.AddAsync("1", CreateAccessToken("1", 3600));

            await _sut.AddAsync("1", CreateAccessToken("modified", 3600));
            var result = await _sut.GetAsync("1");

            result.Should().Be("modified");
        }

        [Fact]
        public void AddAsync_NullAccessTokenRequestArgsPassed_Throws()
        {
            Func<Task> act = () => _sut.AddAsync("not null", null);

            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("    ")]
        [InlineData("")]
        public void AddAsync_InvalidRequestUriPassed_Throws(string requestUri)
        {
            Func<Task> act = () => _sut.AddAsync(requestUri, CreateAccessToken("a", 1));

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GetAsync_AccessTokenExpired_ReturnsNull()
        {
            await _sut.AddAsync("expires", CreateAccessToken("expires", 1));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var result = await _sut.GetAsync("expires");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_ContainsNoElements_ReturnsNull()
        {
            var result = await _sut.GetAsync("no_elements");

            result.Should().BeNull();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("    ")]
        [InlineData("")]
        public void GetAsync_InvalidRequestUriPassed_Throws(string requestUri)
        {
            Func<Task> act = () => _sut.GetAsync(requestUri);

            act.Should().Throw<ArgumentNullException>();
        }

        private static IDistributedCache CreateInMemoryDistributedCache()
        {
            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider.GetService<IDistributedCache>();
        }

        private static AccessTokenResponse CreateAccessToken(string value, int expiresIn) =>
            new AccessTokenResponse
            {
                AccessToken = value,
                TokenType = "Bearer",
                ExpiresIn = expiresIn
            };
    }
}
