using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class TokenTests
    {
        private static int accountCount;

        public TestContext TestContext { get; set; }

        private Account account;

        [TestInitialize]
        public async Task TestInitialize()
        {
            if (accountCount++ > TierLimits.AccountsPerCreatedFrom[AccountTier.Small])
            {
                await Assembly.ResetCosmosEmulatorAsync();
            }

            this.account = new Account
            {
                Name = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5()
            };
            _ = await Assembly.Client.Account.CreateAsync(account);
        }

        [TestMethod]
        public async Task Token_Creation_Succeeds()
        {
            // Arrange
            var token = new Token
            {
                Account = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5(),
                Name = this.TestContext.TestName
            };

            // Act
            var response = await Assembly.Client.Token.CreateAsync(token);

            // Assert
            response.Account.Should().Be(this.TestContext.TestName.ToLower());
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(90), TimeSpan.FromMinutes(1));
            response.Key.Should().BeNull();
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Value.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task Token_Creation_Succeeds_ForShortExpiration()
        {
            // Arrange
            var token = new Token
            {
                Account = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5(),
                Name = this.TestContext.TestName,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5)
            };

            // Act
            var response = await Assembly.Client.Token.CreateAsync(token);

            // Assert
            response.Account.Should().Be(this.TestContext.TestName.ToLower());
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddMinutes(5), TimeSpan.FromMinutes(1));
            response.Key.Should().BeNull();
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Value.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task Token_Creation_Succeeds_ForLongExpiration()
        {
            // Arrange
            var token = new Token
            {
                Account = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5(),
                Name = this.TestContext.TestName,
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(5)
            };

            // Act
            var response = await Assembly.Client.Token.CreateAsync(token);

            // Assert
            response.Account.Should().Be(this.TestContext.TestName.ToLower());
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(5), TimeSpan.FromMinutes(1));
            response.Key.Should().BeNull();
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Value.Should().NotBeNullOrWhiteSpace();
        }
    }
}
