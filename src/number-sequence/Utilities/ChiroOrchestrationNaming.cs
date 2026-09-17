namespace number_sequence.Utilities
{
    /// <summary>
    /// The durable orchestration instance id for a chiro record's pdf generation.
    /// Shared by every place that creates one because the id is deterministic from the row id and would otherwise collide with itself on a retry.
    /// Folding in <see cref="Models.ChiroRecord.ProcessAttempt"/> is what makes a retry's id distinct, the same way <c>Invoice.FriendlyId</c> folds in its own attempt counter.
    /// </summary>
    public static class ChiroOrchestrationNaming
    {
        public static string InstanceId(string rowId, string templateId, int processAttempt) => $"{rowId.MakeHumanFriendly()}_{templateId}_{processAttempt:000}";
    }
}
