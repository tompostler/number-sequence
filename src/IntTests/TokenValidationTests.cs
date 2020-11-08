using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using number_sequence.Extensions;
using number_sequence.Models;
using System;
using System.Net;
using System.Threading.Tasks;
using TcpWtf.NumberSequence.Client;

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
        public async Task TokenValidation_FailsSuccessfully_WithAccountTooShort()
        {
            // Arrange
            this.token.Account = this.token.Account.Substring(0, 2);

            // Act
            Func<Task> act = () => this.client.Ping.SendWithAuthAsync();

            // Assert
            (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
