using System;
using FluentAssertions;
using iSHARE.TokenValidator.Args;
using iSHARE.TokenValidator.Models;
using Xunit;

namespace iSHARE.Tests.TokenValidator.Args
{
    public class TokenValidationArgsTests
    {
        [Theory]
        [InlineData(true, "valid", "valid")]
        [InlineData(false, null, "valid")]
        [InlineData(false, "", "valid")]
        [InlineData(false, " ", "valid")]
        [InlineData(false, "valid", "")]
        [InlineData(false, "valid", " ")]
        public void Constructor_InvalidArguments_Throws(bool isAssertionModelNull, string issuer, string audience)
        {
            var assertionModel = isAssertionModelNull ? null : new AssertionModel(null, null, null);

            Action act = () => new TokenValidationArgs(assertionModel, issuer, audience);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_ValidArguments_CreatesObject()
        {
            var result = new TokenValidationArgs(new AssertionModel(null, null, null), "valid", null);

            result.Should().NotBeNull();
        }
    }
}
