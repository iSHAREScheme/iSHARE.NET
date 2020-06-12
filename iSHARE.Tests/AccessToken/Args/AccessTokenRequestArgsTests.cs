using System;
using FluentAssertions;
using iSHARE.AccessToken.Args;
using Xunit;

namespace iSHARE.Tests.AccessToken.Args
{
    public class AccessTokenRequestArgsTests
    {
        [Theory]
        [InlineData(null, "valid", "valid")]
        [InlineData("", "valid", "valid")]
        [InlineData(" ", "valid", "valid")]
        [InlineData("valid", null, "valid")]
        [InlineData("valid", "", "valid")]
        [InlineData("valid", " ", "valid")]
        [InlineData("valid", "valid", null)]
        [InlineData("valid", "valid", "")]
        [InlineData("valid", "valid", " ")]
        public void Constructor_InvalidArguments_Throws(string requestUri, string clientId, string clientAssertion)
        {
            Action act = () => new AccessTokenRequestArgs(requestUri, clientId, clientAssertion);

            act.Should().Throw<ArgumentNullException>();
        }
    }
}
