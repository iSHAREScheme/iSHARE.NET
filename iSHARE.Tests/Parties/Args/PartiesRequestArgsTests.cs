using System;
using FluentAssertions;
using iSHARE.Parties.Args;
using Xunit;

namespace iSHARE.Tests.Parties.Args
{
    public class PartiesRequestArgsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void Constructor_InvalidAccessToken_Throws(string accessToken)
        {
            Action act = () => new PartiesRequestArgs(accessToken, "*");

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_AllArgumentNulls_Throws()
        {
            Action act = () => new PartiesRequestArgs("accessToken", dateTime: DateTime.UtcNow);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_ValidArgs_CreatesObject()
        {
            var result = new PartiesRequestArgs("accessToken", "*");

            result.Should().NotBeNull();
        }
    }
}
