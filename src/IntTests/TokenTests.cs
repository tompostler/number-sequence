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
    public sealed class TokenTests
    {
        private static int accountCount;

        public TestContext TestContext { get; set; }

        private Account account;
        private Token token;

        [TestInitialize]
        public async Task TestInitialize()
        {
            if (accountCount++ % TierLimits.AccountsPerCreatedFrom[AccountTier.Small] == 0)
            {
                await Assembly.ResetCosmosEmulatorAsync();
            }

            this.account = new Account
            {
                Name = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5()
            };
            _ = await Assembly.UnauthedClient.Account.CreateAsync(this.account);

            this.token = new Token
            {
                Account = this.TestContext.TestName,
                Key = this.TestContext.TestName.ComputeMD5(),
                Name = this.TestContext.TestName
            };
        }

        [TestMethod]
        public async Task Token_Creation_Succeeds()
        {
            // Act
            var response = await Assembly.UnauthedClient.Token.CreateAsync(this.token);

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
            this.token.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5);

            // Act
            var response = await Assembly.UnauthedClient.Token.CreateAsync(this.token);

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
            this.token.ExpiresAt = DateTimeOffset.UtcNow.AddYears(5);

            // Act
            var response = await Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            response.Account.Should().Be(this.TestContext.TestName.ToLower());
            response.CreatedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
            response.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddYears(5), TimeSpan.FromMinutes(1));
            response.Key.Should().BeNull();
            response.Name.Should().Be(this.TestContext.TestName.ToLower());
            response.Value.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithNull()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(null);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithAccountNull()
        {
            // Arrange
            this.token.Account = null;

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithAccountTooShort()
        {
            // Arrange
            this.token.Account = this.token.Account.Substring(0, 2);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithAccountTooLong()
        {
            // Arrange
            this.token.Account = string.Concat(Enumerable.Repeat(this.token.Account, 2));

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithAccountNotFound()
        {
            // Arrange
            this.token.Account += '1';

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitKeyNull()
        {
            // Arrange
            this.token.Key = null;

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitKeyTooShort()
        {
            // Arrange
            this.token.Key = this.token.Key.Substring(0, 10);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitKeyTooLong()
        {
            // Arrange
            this.token.Key = string.Concat(Enumerable.Repeat(this.token.Key, 5));

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitKeyWrong()
        {
            // Arrange
            this.token.Key += '1';

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitNameNull()
        {
            // Arrange
            this.token.Name = null;

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitNameTooShort()
        {
            // Arrange
            this.token.Name = this.token.Name.Substring(0, 2);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitNameTooLong()
        {
            // Arrange
            this.token.Name = string.Concat(Enumerable.Repeat(this.token.Name, 2));

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WitNameAlreadyTaken()
        {
            // Arrange
            _ = await Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithExpiresAtTooSoon()
        {
            // Arrange
            this.token.ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(1);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_Fails_WithExpiresAtTooFar()
        {
            // Arrange
            this.token.ExpiresAt = DateTimeOffset.UtcNow.AddYears(42);

            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Token_Creation_GetsThrottled()
        {
            // Act
            this.token.Name = this.TestContext.TestName + '1';
            await Assembly.UnauthedClient.Token.CreateAsync(this.token);
            this.token.Name = this.TestContext.TestName + '2';
            await Assembly.UnauthedClient.Token.CreateAsync(this.token);
            this.token.Name = this.TestContext.TestName + '3';
            await Assembly.UnauthedClient.Token.CreateAsync(this.token);
            this.token.Name = this.TestContext.TestName + '4';
            Func<Task> act = () => Assembly.UnauthedClient.Token.CreateAsync(this.token);

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);

            // Cleanup
            await Assembly.ResetCosmosEmulatorAsync();
        }
    }
}
