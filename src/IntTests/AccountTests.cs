using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class AccountTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task Account_Creation_Succeeds()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            var response = await Assembly.Client.Account.CreateAsync(account);

            // Assert
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.CreatedFrom.Should().NotBeNullOrWhiteSpace();
            response.Key.Should().BeNull();
            response.ModifiedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Tier.Should().Be(AccountTier.Small);
        }
    }
}
