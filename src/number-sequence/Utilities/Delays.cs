﻿namespace number_sequence.Utilities
{
    public sealed class Delays
    {
        public sealed class DelayImpelmentation
        {
            private TaskCompletionSource earlyTaskCompletionSource;

            /// <summary>
            /// Gracefully cancel the delay.
            /// </summary>
            public void CancelDelay() => this.earlyTaskCompletionSource?.TrySetResult();

            /// <summary>
            /// Intended to only be called by one location at a time and not in parallel.
            /// </summary>
            public async Task Delay(TimeSpan delay, CancellationToken cancellationToken)
            {
                this.earlyTaskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

                var cts = new CancellationTokenSource();
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

                var delayTask = Task.Delay(delay, linkedCts.Token);

                // Whichever one returned, cancel the other
                Task returnedTask = await Task.WhenAny(delayTask, this.earlyTaskCompletionSource.Task);
                if (returnedTask == delayTask)
                {
                    _ = this.earlyTaskCompletionSource.TrySetCanceled(CancellationToken.None);
                    this.earlyTaskCompletionSource = null;
                    await delayTask;
                }
                else
                {
                    cts.Cancel();
                }
            }
        }
    }
}
