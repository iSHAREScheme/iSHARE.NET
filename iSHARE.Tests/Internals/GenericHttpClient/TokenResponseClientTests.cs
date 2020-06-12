using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using iSHARE.Internals;
using iSHARE.Internals.GenericHttpClient;
using iSHARE.Internals.GenericHttpClient.Args;
using Moq;
using Moq.Protected;
using Xunit;

namespace iSHARE.Tests.Internals.GenericHttpClient
{
    public class TokenResponseClientTests
    {
        private const string RequestUri = "https://example.com/some_path";

        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly ITokenResponseClient _sut;

        public TokenResponseClientTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _sut = new TokenResponseClient(_httpClientFactoryMock.Object);
        }

        [Fact]
        public void SendRequestAsync_ResponseNot200_Throws()
        {
            SetupHttpClientFactory(RequestUri, responseCode: HttpStatusCode.NotFound);

            Func<Task> act = () => _sut.SendRequestAsync(new TokenSendRequestArgs(RequestUri));

            act.Should().Throw<HttpRequestException>();
        }

        [Fact]
        public void SendRequestAsync_TokenNotFound_Throws()
        {
            SetupHttpClientFactory(RequestUri, tokenName: "some_fake_name");

            Func<Task> act = () => _sut.SendRequestAsync(new TokenSendRequestArgs(RequestUri));

            act.Should().Throw<TokenNotFoundException>();
        }

        [Theory]
        [InlineData("https://example.com/trusted_list", "trusted_list_token")]
        [InlineData("https://example.com/parties/", "parties_token")]
        public async Task SendRequestAsync_RequestWithoutParamsOrAccessToken_ReturnsToken(
            string requestUri,
            string tokenName)
        {
            SetupHttpClientFactory(requestUri.TrimEnd('/'), tokenName);

            var response = await _sut.SendRequestAsync(new TokenSendRequestArgs(requestUri));

            response.Should().Be("token_value");
        }

        [Fact]
        public async Task SendRequestAsync_RequestWithAccessToken_ReturnsToken()
        {
            Expression<Func<HttpRequestMessage, bool>> expression = x =>
                x.Headers.First(x => x.Key == "Authorization").Value.First() == "Bearer token.AaA";
            SetupHttpClientFactory(RequestUri, httpExpression: expression);

            var response = await _sut.SendRequestAsync(new TokenSendRequestArgs(RequestUri, accessToken: "token.AaA"));

            response.Should().Be("token_value");
        }

        [Fact]
        public async Task SendRequestAsync_RequestWithParameters_ReturnsToken()
        {
            SetupHttpClientFactory($"{RequestUri}?name=*&valid_only=true");

            var response = await _sut.SendRequestAsync(
                new TokenSendRequestArgs(
                    RequestUri,
                    new Dictionary<string, string> { { "name", "*" }, { "valid_only", "true" } }));

            response.Should().Be("token_value");
        }

        [Fact]
        public async Task SendRequestAsync_RequestWithParametersAndAccessToken_ReturnsToken()
        {
            Expression<Func<HttpRequestMessage, bool>> expression = x =>
                x.Headers.First(x => x.Key == "Authorization").Value.First() == "Bearer super.secret.value!!!";
            SetupHttpClientFactory($"{RequestUri}?name=*&valid_only=true", httpExpression: expression);

            var response = await _sut.SendRequestAsync(
                new TokenSendRequestArgs(
                    RequestUri,
                    new Dictionary<string, string> { { "name", "*" }, { "valid_only", "true" } },
                    accessToken: "super.secret.value!!!"));

            response.Should().Be("token_value");
        }

        private void SetupHttpClientFactory(
            string requestUri,
            string tokenName = "capabilities_token",
            HttpStatusCode responseCode = HttpStatusCode.OK,
            Expression<Func<HttpRequestMessage, bool>> httpExpression = null)
        {
            Expression<Func<HttpRequestMessage, bool>> defaultExpression = x => x.RequestUri == new Uri(requestUri);

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is(httpExpression ?? defaultExpression),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = responseCode,
                    Content = new StringContent($"{{\"{tokenName}\": \"token_value\"}}"),
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
        }
    }
}
