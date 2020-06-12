using System;
using FluentAssertions;
using iSHARE.Capabilities.Args;
using Xunit;

namespace iSHARE.Tests.Capabilities.Args
{
    public class CapabilitiesRequestArgsTests
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
        public void Constructor_InvalidArguments_Throws(string requestUri, string requestedPartyId, string soToken)
        {
            Action act = () => new CapabilitiesRequestArgs(requestUri, requestedPartyId, soToken);

            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_InvalidAccessToken_Throws(string accessToken)
        {
            Action act = () => new CapabilitiesRequestArgs("valid", "valid", "valid", accessToken);

            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData("valid")]
        [InlineData(null)]
        public void Constructor_ValidAccessToken_CreatesObject(string accessToken)
        {
            var result = new CapabilitiesRequestArgs("valid", "valid", "valid", accessToken);

            result.Should().NotBeNull();
        }
    }
}
