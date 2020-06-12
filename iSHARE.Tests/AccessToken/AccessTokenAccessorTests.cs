using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.AccessToken;
using iSHARE.AccessToken.Args;
using iSHARE.AccessToken.Responses;
using Moq;
using Xunit;

namespace iSHARE.Tests.AccessToken
{
    public class AccessTokenAccessorTests
    {
        private readonly Mock<IAccessTokenClient> _clientMock;
        private readonly Mock<IAccessTokenStorage> _storageMock;
        private readonly IAccessTokenAccessor _sut;

        public AccessTokenAccessorTests()
        {
            _clientMock = new Mock<IAccessTokenClient>();
            _storageMock = new Mock<IAccessTokenStorage>();

            _sut = new AccessTokenAccessor(_clientMock.Object, _storageMock.Object);
        }

        [Fact]
        public void GetAsync_ArgsEqNull_Throws()
        {
            AccessTokenRequestArgs args = null;

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetAsync_SomethingThrows_Throws()
        {
            _storageMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<Exception>();
            var args = CreateArgs();

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<AuthenticationException>();
        }

        [Fact]
        public async Task GetAsync_StorageContainsAccessToken_ClientIsNotInvoked()
        {
            _storageMock
                .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("access token");
            var args = CreateArgs();

            await _sut.GetAsync(args);

            _clientMock.Verify(
                x => x.SendRequestAsync(It.IsAny<AccessTokenRequestArgs>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(null, "Bearer")]
        [InlineData("", "Bearer")]
        [InlineData("access_token_value", null)]
        [InlineData("access_token_value", "Not bearer")]
        public void GetAsync_ClientReturnsInvalidAccessToken_Throws(string token, string tokenType)
        {
            _clientMock
                .Setup(x => x.SendRequestAsync(It.IsAny<AccessTokenRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessTokenResponse{ AccessToken = token, TokenType = tokenType });
            var args = CreateArgs();

            Func<Task> act = () => _sut.GetAsync(args);

            act.Should().Throw<AuthenticationException>();
        }

        [Fact]
        public async Task GetAsync_ClientReturnsValidAccessToken_TokenIsStoredAndReturned()
        {
            _clientMock
                .Setup(x => x.SendRequestAsync(It.IsAny<AccessTokenRequestArgs>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AccessTokenResponse { AccessToken = "valid", TokenType = "Bearer" });
            var args = CreateArgs();

            var result = await _sut.GetAsync(args);

            _storageMock.Verify(
                x => x.AddAsync(It.IsAny<string>(), It.IsAny<AccessTokenResponse>(), It.IsAny<CancellationToken>()));
            result.Should().Be("valid");
        }

        private static AccessTokenRequestArgs CreateArgs() => new AccessTokenRequestArgs("a", "b", "c");
    }
}
