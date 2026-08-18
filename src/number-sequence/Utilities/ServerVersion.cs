namespace number_sequence.Utilities
{
    public static class ServerVersion
    {
        // Both this project and the referenced Client project have an nbgv-generated ThisAssembly, hence the conflict.
#pragma warning disable CS0436 // Type conflicts with imported type
        public static string Current => ThisAssembly.AssemblyInformationalVersion;
#pragma warning restore CS0436 // Type conflicts with imported type
    }
}
