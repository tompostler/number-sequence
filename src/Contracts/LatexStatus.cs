namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The status of the background services for latex generation.
    /// </summary>
    public sealed class LatexStatus
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public sealed class LatexTemplateSpreadsheetRow
        {
            public string RowId { get; set; }
            public string LatexDocumentId { get; set; }
            public string CreatedDate { get; set; }
        }
        public sealed class LatexDocument
        {
            public string Id { get; set; }
            public string CreatedDate { get; set; }
            public string ProcessedAt { get; set; }
            public string Delay { get; set; }
            public string Successful { get; set; }
        }
        public sealed class EmailDocument
        {
            public string Id { get; set; }
            public string Subject { get; set; }
            public string AttachmentName { get; set; }
            public string CreatedDate { get; set; }
            public string ProcessedAt { get; set; }
            public string Delay { get; set; }
        }

        public List<LatexTemplateSpreadsheetRow> LatexTemplateSpreadsheetRows { get; set; }
        public List<LatexDocument> LatexDocuments { get; set; }
        public List<EmailDocument> EmailDocuments { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
