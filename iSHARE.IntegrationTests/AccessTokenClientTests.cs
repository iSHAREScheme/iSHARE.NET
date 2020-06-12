using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.AccessToken;
using iSHARE.AccessToken.Args;
using Xunit;

namespace iSHARE.IntegrationTests
{
    public class AccessTokenClientTests
    {
        private readonly IAccessTokenClient _sut;

        public AccessTokenClientTests()
        {
            _sut = new AccessTokenClient(new HttpClient());
        }

        [Fact]
        public async Task SendRequestAsync_ValidRequest_ReturnsAccessToken()
        {
            var clientAssertion = JwtUtilities.Create(
                issuer: Constants.AbcParty.ClientId,
                audience: Constants.SchemeOwner.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);
            var requestArgs = CreateRequestArgs(clientAssertion);

            var result = await _sut.SendRequestAsync(requestArgs);

            result.Should().NotBeNull();
            result.ExpiresIn.Should().Be(3600);
            result.TokenType.Should().Be("Bearer");
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task SendRequestAsync_UriRequestEndsWithSlash_ReturnsAccessToken()
        {
            var clientAssertion = JwtUtilities.Create(
                issuer: Constants.AbcParty.ClientId,
                audience: Constants.SchemeOwner.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);
            var requestArgs = CreateRequestArgs(clientAssertion, $"{Constants.SchemeOwner.AccessTokenRequestUri}/");

            var result = await _sut.SendRequestAsync(requestArgs);

            result.Should().NotBeNull();
            result.ExpiresIn.Should().Be(3600);
            result.TokenType.Should().Be("Bearer");
            result.AccessToken.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void SendRequestAsync_InvalidRequest_ReturnsResponse()
        {
            var clientAssertion = JwtUtilities.Create(
                issuer: Constants.AbcParty.ClientId,
                audience: Constants.AbcParty.ClientId,
                Constants.AbcParty.PrivateKey,
                Constants.AbcParty.PublicKeyBase64Der);
            var requestArgs = CreateRequestArgs(clientAssertion);

            Func<Task> act = () => _sut.SendRequestAsync(requestArgs);

            act.Should().Throw<HttpRequestException>();
        }

        private static AccessTokenRequestArgs CreateRequestArgs(
            string clientAssertion,
            string requestUri = Constants.SchemeOwner.AccessTokenRequestUri) =>
            new AccessTokenRequestArgs(requestUri, Constants.AbcParty.ClientId, clientAssertion);
    }
}
