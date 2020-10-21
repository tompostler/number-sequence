﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class PingTests
    {
        [TestMethod]
        public async Task Ping_ReturnsSuccess()
        {
            // Assert
            await Assembly.Client.Ping.SendAsync();
        }
    }
}