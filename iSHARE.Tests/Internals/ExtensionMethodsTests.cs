using System;
using FluentAssertions;
using iSHARE.Exceptions;
using iSHARE.Internals;
using Xunit;

namespace iSHARE.Tests.Internals
{
    public class ExtensionMethodsTests
    {
        [Theory]
        [InlineData("https://example.com/", "https://example.com")]
        [InlineData("https://example.com/test/", "https://example.com/test")]
        [InlineData("https://example.com/test", "https://example.com/test")]
        [InlineData("https://example.com/test/ ", "https://example.com/test")]
        public void RemoveSlashSuffix_StringPassed_RemovesSuffixIfNeeded(string input, string expectedValue)
        {
            var result = input.RemoveSlashSuffix();

            result.Should().Be(expectedValue);
        }

        [Fact]
        public void GetConfigurationValue_ConfigurationExists_ReturnsUrl()
        {
            var configuration = ConfigurationBuilder.Build();

            var result = configuration.GetConfigurationValue("SchemeOwnerUrl");

            result.Should().Be("https://scheme.isharetest.net");
        }

        [Fact]
        public void GetConfigurationValue_ConfigurationDoesNotExist_Throws()
        {
            var configuration = ConfigurationBuilder.Build();

            Action act = () => configuration.GetConfigurationValue("NotExists");

            act.Should().Throw<InvalidConfigurationException>();
        }
    }
}
