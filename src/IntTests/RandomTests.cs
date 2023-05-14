using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using number_sequence.Controllers;
using System.Net;
using TcpWtf.NumberSequence.Client;

namespace number_sequence.IntTests
{
    [TestClass]
    public sealed class RandomTests
    {
        [TestMethod]
        public async Task Random_8Ball_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                string response = await Assembly.UnauthedClient.Random.Get8BallAsync();

                // Assert
                _ = RandomController.EightBallResponses.Should().Contain(response);
            }
        }

        [TestMethod]
        public async Task Random_Guid_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            Guid previous = Guid.Empty;
            for (uint i = 0; i < 100; i++)
            {
                // Act
                Guid response = await Assembly.UnauthedClient.Random.GetGuidAsync();

                // Assert
                _ = response.Should().NotBe(previous);
                previous = response;
            }
        }

        [TestMethod]
        public async Task Random_Name_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            string previous = default;
            for (uint i = 0; i < 100; i++)
            {
                // Act
                string response = await Assembly.UnauthedClient.Random.GetNameAsync();

                // Assert
                _ = response.Should().NotBe(previous);
                previous = response;
            }
        }

        [TestMethod]
        public async Task Random_ULong_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongAsync();

                // Assert
                _ = response.Should().BeInRange(0, 100);
            }
        }

        [TestMethod]
        public async Task Random_ULong_WithMin_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongAsync(min: 90);

                // Assert
                _ = response.Should().BeInRange(90, 100);
            }
        }

        [TestMethod]
        public async Task Random_ULong_WithMax_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongAsync(max: 10);

                // Assert
                _ = response.Should().BeInRange(0, 10);
            }
        }

        [TestMethod]
        public async Task Random_ULong_WithMinMax_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongAsync(min: 1_000_000_000, max: 1_000_000_010);

                // Assert
                _ = response.Should().BeInRange(1_000_000_000, 1_000_000_010);
            }
        }

        [TestMethod]
        public async Task Random_ULong_WithMinEqualToMax_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongAsync(min: i, max: i);

                // Assert
                _ = response.Should().Be(i);
            }
        }

        [TestMethod]
        public async Task Random_ULong_WithMinGreaterThanMax()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Random.GetULongAsync(min: 10, max: 0);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Random_ULong01_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong01Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x0000000000000001);
            }
        }

        [TestMethod]
        public async Task Random_ULong02_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong02Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x0000000000000003);
            }
        }

        [TestMethod]
        public async Task Random_ULong04_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong04Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x000000000000000F);
            }
        }

        [TestMethod]
        public async Task Random_ULong08_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong08Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x00000000000000FF);
            }
        }

        [TestMethod]
        public async Task Random_ULong16_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong16Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x000000000000FFFF);
            }
        }

        [TestMethod]
        public async Task Random_ULong32_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong32Async();

                // Assert
                _ = response.Should().BeInRange(0, 0x00000000FFFFFFFF);
            }
        }

        [TestMethod]
        public async Task Random_ULong64_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULong64Async();

                // Assert
                _ = response.Should().BeInRange(0, 0xFFFFFFFFFFFFFFFF);
            }
        }

        [TestMethod]
        public async Task Random_ULongBits_ReturnsSuccess()
        {
            for (byte bits = 1; bits < 64; bits++)
            {
                // Run 100 times to make sure it's not a fluke
                for (uint i = 0; i < 100; i++)
                {
                    // Act
                    ulong response = await Assembly.UnauthedClient.Random.GetULongBitsAsync(bits);

                    // Assert
                    _ = response.Should().BeInRange(0, 0x1UL << bits, because: $"we asked for {bits} bits");
                }
            }
        }

        [TestMethod]
        public async Task Random_ULongBitsMax_ReturnsSuccess()
        {
            // Run 100 times to make sure it's not a fluke
            for (uint i = 0; i < 100; i++)
            {
                // Act
                ulong response = await Assembly.UnauthedClient.Random.GetULongBitsAsync(64);

                // Assert
                _ = response.Should().BeInRange(0, 0xFFFFFFFFFFFFFFFF);
            }
        }

        [TestMethod]
        public async Task Random_ULongBits_WithBitsTooLow()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Random.GetULongBitsAsync(0);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task Random_ULongBits_WithBitsTooHigh()
        {
            // Act
            Func<Task> act = () => Assembly.UnauthedClient.Random.GetULongBitsAsync(65);

            // Assert
            _ = (await act.Should().ThrowExactlyAsync<NsTcpWtfClientException>()).And.Response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
