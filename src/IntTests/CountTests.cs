using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Net;
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
                await Assembly.ResetDataStorageAsync();
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
            Count response = await this.client.Count.CreateAsync(this.count);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(0);
        }

        [TestMethod]
        public async Task Count_Creation_Succeeds_AtNonZero()
        {
            // Arrange
            this.count.Value = 1;

            // Act
            Count response = await this.client.Count.CreateAsync(this.count);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(1);
        }

        [TestMethod]
        public async Task Count_Increment_Succeeds()
        {
            // Arrange
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementAsync(this.TestContext.TestName);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(1);
        }

        [TestMethod]
        public async Task Count_Increment_Succeeds_AtNonZero()
        {
            // Arrange
            this.count.Value = 1;
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementAsync(this.TestContext.TestName);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(2);
        }

        [TestMethod]
        public async Task Count_Increment_Succeeds_AtMaxIntegerDouble()
        {
            // Arrange
            this.count.Value = 9_007_199_254_740_992;
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementAsync(this.TestContext.TestName);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(9_007_199_254_740_993);
        }

        [TestMethod]
        public async Task Count_Increment_Succeeds_AtMaxULong()
        {
            // Arrange
            this.count.Value = ulong.MaxValue;
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementAsync(this.TestContext.TestName);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(0);
        }

        [TestMethod]
        public async Task Count_IncrementByAmount_Succeeds()
        {
            // Arrange
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementByAmountAsync(this.TestContext.TestName, 3);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(3);
        }

        [TestMethod]
        public async Task Count_IncrementByAmount_Succeeds_AtMaxULong()
        {
            // Arrange
            this.count.Value = ulong.MaxValue;
            _ = await this.client.Count.CreateAsync(this.count);

            // Act
            Count response = await this.client.Count.IncrementByAmountAsync(this.TestContext.TestName, 3);

            // Assert
            _ = response.Account.Should().Be(Assembly.Account.Name.ToLower());
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Value.Should().Be(2);
        }

        [TestMethod]
        public async Task Count_Creation_Fails_WithNameNull()
        {
            // Arrange
            this.count.Name = null;

            // Act
            Func<Task> act = () => this.client.Count.CreateAsync(this.count);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Count_Creation_Fails_WithNameTooShort()
        {
            // Arrange
            this.count.Name = this.count.Name.Substring(0, 2);

            // Act
            Func<Task> act = () => this.client.Count.CreateAsync(this.count);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Count_Creation_Fails_WithNameTooLong()
        {
            // Arrange
            this.count.Name = string.Concat(Enumerable.Repeat(this.count.Name, 2));

            // Act
            Func<Task> act = () => this.client.Count.CreateAsync(this.count);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Count_Creation_GetsThrottled()
        {
            // Act
            this.count.Name = this.TestContext.TestName + '1';
            _ = await this.client.Count.CreateAsync(this.count);
            this.count.Name = this.TestContext.TestName + '2';
            _ = await this.client.Count.CreateAsync(this.count);
            this.count.Name = this.TestContext.TestName + '3';
            _ = await this.client.Count.CreateAsync(this.count);
            this.count.Name = this.TestContext.TestName + '4';
            Func<Task> act = () => this.client.Count.CreateAsync(this.count);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Cleanup
            await Assembly.ResetDataStorageAsync();
        }
    }
}
