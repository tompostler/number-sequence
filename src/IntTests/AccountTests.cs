using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
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
            Account response = await Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.CreatedFrom.Should().NotBeNullOrWhiteSpace();
            _ = response.Key.Should().BeNull();
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Tier.Should().Be(AccountTier.Small);
        }

        [TestMethod]
        public async Task Account_Creation_Fails_WithNull()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(null);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Account_Creation_GetsThrottled()
        {
            // Arrange
            await Assembly.ResetDataStorageAsync();
            var account = new Account
            {
                Name = this.TestContext.TestName + '1',
                Key = this.TestContext.TestName.ComputeMD5()
            };

            // Act
            _ = await Assembly.UnauthedClient.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '2';
            _ = await Assembly.UnauthedClient.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '3';
            _ = await Assembly.UnauthedClient.Account.CreateAsync(account);
            account.Name = this.TestContext.TestName + '4';
            Func<Task> act = () => Assembly.UnauthedClient.Account.CreateAsync(account);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Cleanup
            await Assembly.ResetDataStorageAsync();
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
            _ = await Assembly.UnauthedClient.Account.CreateAsync(account);
            Account response = await Assembly.UnauthedClient.Account.GetAsync(account.Name);

            // Assert
            _ = response.CreatedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.CreatedFrom.Should().BeNull();
            _ = response.Key.Should().BeNull();
            _ = response.ModifiedDate.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            _ = response.Name.Should().Be(this.TestContext.TestName.ToLower());
            _ = response.Tier.Should().Be(AccountTier.Small);
        }

        [TestMethod]
        public async Task Account_Get_Fails_OnNonexistentAccount()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Account.GetAsync(this.TestContext.TestName);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
