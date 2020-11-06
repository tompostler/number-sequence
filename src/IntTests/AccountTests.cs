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

        [TestMethod]
        public async Task Account_Creation_Fails_WithNull()
        {
            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(null);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithKeyNull()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithKeyTooShort()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5().Substring(0, 10)
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithKeyTooLong()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName,
                Key = string.Concat(Enumerable.Repeat(this.TestContext.TestName.ComputeMD5(), 5))
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithNameNull()
        {
            // Arrange
            var account = new Account
            {
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithNameTooShort()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName.Substring(0, 2),
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithNameTooLong()
        {
            // Arrange
            var account = new Account
            {
                Name = string.Concat(Enumerable.Repeat(this.TestContext.TestName, 2)),
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_GetsThrottled()
        {
            // Arrange
            await Assembly.ResetCosmosEmulatorAsync();
            var account = new Account
            {
                Name = this.TestContext.TestName + '1',
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            await Assembly.Client.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '2';
            await Assembly.Client.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '3';
            await Assembly.Client.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '4';
            Func<Task> act = () => Assembly.Client.Account.CreateAsync(account);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            await Assembly.ResetCosmosEmulatorAsync();
        }

        [TestMethod]
        public async Task Account_Get_Succeeds()
        {
            // Arrange
            var account = new Account
            {
                Name = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            await Assembly.Client.Account.CreateAsync(account);
            var response = await Assembly.Client.Account.GetAsync(account.Name);

            // Assert
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.CreatedFrom.Should().BeNull();
            response.Key.Should().BeNull();
            response.ModifiedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Tier.Should().Be(AccountTier.Small);
        }

        [TestMethod]
        public async Task Account_Get_Fails_OnNonexistentAccount()
        {
            // Act
            Func<Task> act = () => Assembly.Client.Account.GetAsync(this.TestContext.TestName);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
