using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using number_sequence.Utilities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class DelaysTests
    {
        private Delays.DelayImpelmentation delay;

        [TestInitialize]
        public void TestInit()
        {
            this.delay = new Delays.DelayImpelmentation();
        }

        [TestMethod]
        public async Task DelayWithNoInterruptShouldDelay()
        {
            // Arrange
            var delay = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            // Act
            await this.delay.Delay(delay, CancellationToken.None);

            // Assert
            _ = sw.Elapsed.Should().BeCloseTo(delay, TimeSpan.FromSeconds(0.1));
        }

        [TestMethod]
        public async Task DelayWithInterruptShouldDelayUntilInterrupt()
        {
            // Arrange
            var delay = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();

            // Act
            Task delayTask = this.delay.Delay(delay, CancellationToken.None);
            await Task.Delay((int)delay.TotalMilliseconds / 2);
            this.delay.CancelDelay();
            await delayTask;

            // Assert
            _ = sw.Elapsed.Should().BeCloseTo(delay / 2, TimeSpan.FromSeconds(0.1));
            _ = delayTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [TestMethod]
        public async Task DelayWithCancellationShouldThrow()
        {
            // Arrange
            var delay = TimeSpan.FromSeconds(1);
            var sw = Stopwatch.StartNew();
            var cts = new CancellationTokenSource(delay / 2);

            // Act
            Func<Task> act = () => this.delay.Delay(delay, cts.Token);

            // Assert
            _ = await act.Should().ThrowAsync<OperationCanceledException>();
            _ = sw.Elapsed.Should().BeCloseTo(delay / 2, TimeSpan.FromSeconds(0.1));
        }
    }
}
