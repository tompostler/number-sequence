using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class CountTests
    {
        private static int countCount;

        public TestContext TestContext { get; set; }
        
        private NsTcpWtfClient client;
        private Count count;

        [TestInitialize]
        public async Task TestInitialize()
        {
            if (countCount++ % TierLimits.AccountsPerCreatedFrom[AccountTier.Small] == 0)
            {
                await Assembly.ResetCosmosEmulatorAsync();
            }

            this.client = Assembly.Client;
            this.count = new Count
            {
                Name = this.TestContext.TestName
            };
        }

        [TestMethod]
        public async Task Count_Creation_Succeeds()
        {
            // Act
            var response = await this.client.Count.CreateAsync(this.count);

            // Assert
            response.Account.Should().Be(Assembly.Account.Name.ToLower());
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.ModifiedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Value.Should().Be(0);
        }
    }
}
