namespace number_sequence
{
    public static class Options
    {
        public sealed class CosmosDB
        {
            public string Endpoint { get; set; }
            public string Key { get; set; }
            public string DatabaseId { get; set; }
            public string ContainerId { get; set; }
        }
    }
}
