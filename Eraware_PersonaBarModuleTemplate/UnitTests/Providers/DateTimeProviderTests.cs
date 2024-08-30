using $ext_rootnamespace$.Providers;
using NSubstitute;
using System;
using Xunit;

namespace UnitTests.Providers
{
    public class DateTimeProviderTests
    {
        private readonly IDateTimeProvider provider;
        private readonly IDateTimeProvider providerMock;

        public DateTimeProviderTests()
        {
            this.provider = new DateTimeProvider();
            this.providerMock = Substitute.For<IDateTimeProvider>();
        }

        [Fact]
        public void DateTimeProvider_ProvidesUtc()
        {
            // Arrange
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            var inOneMinute = DateTime.UtcNow.AddMinutes(1);

            // Act
            var now = provider.GetUtcNow();

            // Assert
            Assert.True(oneMinuteAgo < now);
            Assert.True(inOneMinute > now);
        }

        [Fact]
        public void DateTimeProvider_CanBeMocked()
        {
            // Arrange
            var expected = new DateTime(2022, 1, 1);
            this.providerMock.GetUtcNow().Returns(expected);

            // Act
            var actual = this.providerMock.GetUtcNow();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}