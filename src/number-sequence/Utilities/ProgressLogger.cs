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

        public ProgressLogger(long expectedLength, ILogger logger)
        {
            this.expectedLength = expectedLength;
            this.logger = logger;
        }

        private readonly DateTimeOffset started = DateTimeOffset.UtcNow;
        private DateTimeOffset? lastReported = default;

        public void Report(long value)
        {
            lock (this.reportingLock)
            {
                const int intervalSecondsToReport = 2;
                const int lookbackSliceCount = 5;
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // If we appear to be at the end, also add some summary info
                if (value >= this.expectedLength)
                {
                    TimeSpan duration = now - this.started;
                    decimal totalSeconds = (decimal)duration.TotalSeconds;
                    this.logger.LogInformation($"Progress: {BytesToFriendlyString(value),9}/{BytesToFriendlyString(this.expectedLength)} ({1.0 * value / this.expectedLength:p}). Averaged {BytesToFriendlyBitString(value / totalSeconds)}ps over {duration:mm\\:ss}.");
                }
                // Else if we've just started or once every couple of seconds.
                else if (!this.lastReported.HasValue || (now - this.lastReported.Value).TotalSeconds > intervalSecondsToReport)
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

        private static string BytesToFriendlyString(decimal bytes)
        {
            if (bytes < 1_000_000)
            {
                return $"{bytes / 1_000:0.00}KiB";
            }
            else if (bytes >= 1_000_000 && bytes < 1_000_000_000)
            {
                return $"{bytes / 1_000_000:0.00}MiB";
            }
            else if (bytes >= 1_000_000_000 && bytes < 1_000_000_000_000)
            {
                return $"{bytes / 1_000_000_000:0.00}GiB";
            }
            else
            {
                return $"{bytes / 1_000_000_000_000:0.00}TiB";
            }
        }

        private static string BytesToFriendlyBitString(decimal bytes)
        {
            decimal bits = bytes * 8;
            if (bits < 1_048_576)
            {
                return $"{bits / 1_024:0.00}Kb";
            }
            else if (bits >= 1_048_576 && bits < 1_073_741_824)
            {
                return $"{bits / 1_048_576:0.00}Mb";
            }
            else if (bits >= 1_073_741_824 && bits < 1_099_511_627_776)
            {
                return $"{bits / 1_073_741_824:0.00}Gb";
            }
            else
            {
                return $"{bits / 1_099_511_627_776:0.00}Tb";
            }
        }
    }
}
