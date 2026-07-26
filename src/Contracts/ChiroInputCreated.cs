namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The result of successfully submitting a <see cref="ChiroInput"/>.
    /// </summary>
    public sealed class ChiroInputCreated
    {
        /// <summary>
        /// The id of the recorded row. Shows up as the id on the pdf status page.
        /// </summary>
        public string RowId { get; set; }

        /// <summary>
        /// The id of the orchestration that will generate and send the pdf.
        /// </summary>
        public string OrchestrationInstanceId { get; set; }
    }
}
