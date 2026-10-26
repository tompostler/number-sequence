namespace number_sequence.Exceptions
{
    /// <summary>
    /// An outside service this request depends on failed or refused, such as a library lookup provider timing out. Mapped to
    /// 424 rather than a 5xx because the client retries 5xx responses five times over about half a minute, which turns one
    /// slow provider into a page that hangs.
    /// </summary>
    public sealed class FailedDependencyException : Exception
    {
        public FailedDependencyException() { }
        public FailedDependencyException(string message) : base(message) { }
        public FailedDependencyException(string message, Exception inner) : base(message, inner) { }
    }
}
