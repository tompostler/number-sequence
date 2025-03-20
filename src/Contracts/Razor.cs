namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// <see href="https://en.wikipedia.org/wiki/Philosophical_razor"/>
    /// </summary>
    public sealed class Razor
    {
        /// <summary>
        /// The name of the razor.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Also known as.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The text.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// A link to more information.
        /// </summary>
        public string Reference { get; set; }
    }
}
