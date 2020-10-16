namespace TcpWtf.NumberSequence.Client
{
    /// <summary>
    /// Used to pick the target URI of the service.
    /// </summary>
    public enum Stamp
    {
        /// <summary>
        /// Local development against the emulator.
        /// </summary>
        LocalDev,

        /// <summary>
        /// Cloud deployment of the service. The default.
        /// </summary>
        Public
    }
}
