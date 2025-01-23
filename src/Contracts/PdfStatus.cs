namespace TcpWtf.NumberSequence.Contracts
{
    /// <summary>
    /// The status of the background services for pdf generation.
    /// </summary>
    public sealed class PdfStatus
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public sealed class TemplateSpreadsheetRow
        {
            public string RowId { get; set; }
            public string DocumentId { get; set; }
            public string CreatedDate { get; set; }
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

        public List<TemplateSpreadsheetRow> TemplateSpreadsheetRows { get; set; }
        public List<EmailDocument> EmailDocuments { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
