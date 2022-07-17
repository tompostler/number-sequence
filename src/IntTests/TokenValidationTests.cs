using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using number_sequence.Extensions;
using number_sequence.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;
using TcpWtf.NumberSequence.Contracts;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class TokenValidationTests
    {
        private TokenValue token;
        private NsTcpWtfClient client;

        [TestInitialize]
        public async Task TestInitializeAsync()
        {
            await Assembly.Client.Ping.SendWithAuthAsync();

            this.token = Assembly.Token.Value.FromBase64JsonString<TokenValue>();

            this.client = new NsTcpWtfClient(
                Assembly.LoggerFactory.CreateLogger<NsTcpWtfClient>(),
                (_) => Task.FromResult(this.token.ToBase64JsonString()),
                Stamp.LocalDev);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithAccountNull()
        {
            // Arrange
            this.token.Account = null;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithAccountTooShort()
        {
            // Arrange
            this.token.Account = this.token.Account.Substring(0, 2);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithAccountTooLong()
        {
            // Arrange
            this.token.Account = string.Concat(Enumerable.Repeat(this.token.Account, 2));

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithAccountWrong()
        {
            // Arrange
            this.token.Account = (this.token.Account + '1').Substring(1);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithAccountTierWrong()
        {
            // Arrange
            this.token.AccountTier = AccountTier.Medium;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithCreatedAtWrong()
        {
            // Arrange
            this.token.CreatedAt = DateTimeOffset.UtcNow;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithExpiresAtWrong()
        {
            // Arrange
            this.token.ExpiresAt = DateTimeOffset.UtcNow;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithKeyNull()
        {
            // Arrange
            this.token.Key = null;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithKeyTooShort()
        {
            // Arrange
            this.token.Key = this.token.Key.Substring(0, 120);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithKeyTooLong()
        {
            // Arrange
            this.token.Key = string.Concat(Enumerable.Repeat(this.token.Key, 2));

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithKeyWrong()
        {
            // Arrange
            this.token.Key = (this.token.Key + '1').Substring(1);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithNameNull()
        {
            // Arrange
            this.token.Name = null;

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithNameTooShort()
        {
            // Arrange
            this.token.Name = this.token.Name.Substring(0, 2);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithNameTooLong()
        {
            // Arrange
            this.token.Name = string.Concat(Enumerable.Repeat(this.token.Name, 2));

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task TokenValidation_FailsSuccessfully_WithNameWrong()
        {
            // Arrange
            this.token.Name = (this.token.Name + '1').Substring(1);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
