using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace number_sequence.Utilities
{
    public sealed class ProgressLogger : IProgress<long>
    {
        private readonly long expectedLength;
        private readonly ILogger logger;

        private readonly object reportingLock = new();
        private readonly Queue<(DateTimeOffset, long)> byteProgress = new(3);
        private readonly DateTimeOffset started = DateTimeOffset.UtcNow;

        private DateTimeOffset? lastReported = default;

        public ProgressLogger(long expectedLength, ILogger logger)
        {
            this.expectedLength = expectedLength;
            this.logger = logger;
        }

        /// <summary>
        /// Log the current progress. Will only emit values once every 3 seconds regardless of the number of calls.
        /// </summary>
        public void Report(long value)
        {
            const int intervalSecondsToReport = 3;
            const int lookbackSliceCount = 5;
            DateTimeOffset now = DateTimeOffset.UtcNow;

            // If we've just started or once every couple of seconds, then continue with reporting
            if (!this.lastReported.HasValue || (now - this.lastReported.Value).TotalSeconds > intervalSecondsToReport)
            {
                // Locks during the calculation/generation of the actual report in order to ensure only one thread is reporting on the object at a time.
                // If multiple thread reports came in here, they'll just log a couple times which is fine. So we're not re-checking the condition to log.
                lock (this.reportingLock)
                {
                    if (this.byteProgress.Count >= lookbackSliceCount)
                    {
                        (DateTimeOffset, long) ago = this.byteProgress.Dequeue();
                        long bytesProgress = value - ago.Item2;
                        decimal duration = (decimal)(now - ago.Item1).TotalSeconds;
                        this.logger.LogInformation($"Progress: {BytesToFriendlyString(value),9}/{BytesToFriendlyString(this.expectedLength)} ({1.0 * value / this.expectedLength:p}). Average over last {duration:0} seconds: {BytesToFriendlyBitString(bytesProgress / duration)}ps");
                    }
                    else
                    {
                        this.logger.LogInformation($"Progress: {BytesToFriendlyString(value),9}/{BytesToFriendlyString(this.expectedLength)} ({1.0 * value / this.expectedLength:p})");
                    }
                    this.lastReported = now;
                    this.byteProgress.Enqueue((this.lastReported.Value, value));
                }
            }
        }

        public void ReportComplete()
        {
            // Report 100% based on expected length
            TimeSpan duration = DateTimeOffset.UtcNow - this.started;
            decimal totalSeconds = (decimal)duration.TotalSeconds;
            this.logger.LogInformation($"Progress: {BytesToFriendlyString(this.expectedLength),9}/{BytesToFriendlyString(this.expectedLength)} ({1.0:p}). Averaged {BytesToFriendlyBitString(this.expectedLength / totalSeconds)}ps over {duration:mm\\:ss}.");
        }

        private static string BytesToFriendlyString(decimal bytes)
        {
            if (bytes < 1_048_576)
            {
                return $"{bytes / 1_024:0.00}KiB";
            }
            else if (bytes >= 1_048_576 && bytes < 1_073_741_824)
            {
                return $"{bytes / 1_048_576:0.00}MiB";
            }
            else if (bytes >= 1_073_741_824 && bytes < 1_099_511_627_776)
            {
                return $"{bytes / 1_073_741_824:0.00}GiB";
            }
            else
            {
                return $"{bytes / 1_099_511_627_776:0.00}TiB";
            }
        }

        private static string BytesToFriendlyBitString(decimal bytes)
        {
            decimal bits = bytes * 8;
            if (bits < 1_000_000)
            {
                return $"{bits / 1_000:0.00}Kb";
            }
            else if (bits >= 1_000_000 && bits < 1_000_000_000)
            {
                return $"{bits / 1_000_000:0.00}Mb";
            }
            else if (bits >= 1_000_000_000 && bits < 1_000_000_000_000)
            {
                return $"{bits / 1_000_000_000:0.00}Gb";
            }
            else
            {
                return $"{bits / 1_000_000_000_000:0.00}Tb";
            }
        }
    }
}
