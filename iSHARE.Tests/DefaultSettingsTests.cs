using FluentAssertions;
using Xunit;

namespace iSHARE.Tests
{
    public class DefaultSettingsTests
    {
        [Fact]
        public void Ctor_AppSettingsContainsValues_CreatesProperInstance()
        {
            var sut = new DefaultSettings(ConfigurationBuilder.Build());

            sut.SchemeOwnerUrl.Should().Be("https://scheme.isharetest.net");
            sut.Eori.Should().Be("EU.EORI.NL000000001");
            sut.GetType().GetProperties().Length.Should().Be(2);
        }
    }
}
