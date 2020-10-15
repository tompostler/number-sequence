namespace TcpWtf.NumberSequence.Models
{
    /// <summary>
    /// The tier for an account. This plays into the limit for numbers, requests, and other resources.
    /// </summary>
    public enum AccountTier
    {
        /// <summary>
        /// Small tier should be fine for most usage scenarios.
        /// </summary>
        Small,

        /// <summary>
        /// Medium tier provides more burstability on requests with some additional resources.
        /// </summary>
        Medium,

        /// <summary>
        /// Large tier allows a sizeable number of resources.
        /// </summary>
        Large,

        /// <summary>
        /// ALL THE RESOURCES POSSIBLE!
        /// </summary>
        Infinite
    }
}
