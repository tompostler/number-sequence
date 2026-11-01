using FluentAssertions;
using number_sequence.Utilities;
using System.Diagnostics;

namespace number_sequence.UnitTests.UtilityTests
{
    [TestClass]
    public sealed class DelaysTests
    {
        /// <summary>
        /// How early a timer may fire. Waking late is common on a busy build agent, so the upper bounds only check that
        /// the delay ended well short of where it would have otherwise.
        /// </summary>
        private static readonly TimeSpan Early = TimeSpan.FromMilliseconds(50);

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
            var delay = TimeSpan.FromSeconds(2);
            var sw = Stopwatch.StartNew();

            // Act
            await this.delay.Delay(delay, CancellationToken.None);

            // Assert
            _ = sw.Elapsed.Should().BeGreaterThanOrEqualTo(delay - Early);
        }

        [TestMethod]
        public async Task DelayWithInterruptShouldDelayUntilInterrupt()
        {
            // Arrange
            var delay = TimeSpan.FromSeconds(2);
            var sw = Stopwatch.StartNew();

            // Act
            Task delayTask = this.delay.Delay(delay, CancellationToken.None);
            await Task.Delay((int)delay.TotalMilliseconds / 2);
            this.delay.CancelDelay();
            await delayTask;

            // Assert
            _ = sw.Elapsed.Should().BeGreaterThanOrEqualTo(delay / 2 - Early).And.BeLessThan(delay);
            _ = delayTask.IsCompletedSuccessfully.Should().BeTrue();
        }

        [TestMethod]
        public async Task DelayWithCancellationShouldThrow()
        {
            // Arrange
            var delay = TimeSpan.FromSeconds(2);
            var sw = Stopwatch.StartNew();
            var cts = new CancellationTokenSource(delay / 2);

            // Act
            Func<Task> act = () => this.delay.Delay(delay, cts.Token);

            // Assert
            _ = await act.Should().ThrowAsync<OperationCanceledException>();
            _ = sw.Elapsed.Should().BeGreaterThanOrEqualTo(delay / 2 - Early).And.BeLessThan(delay);
        }
    }
}
