namespace number_sequence.Exceptions
{
    public sealed class ForbiddenException : Exception
    {
        public ForbiddenException() { }
        public ForbiddenException(string message) : base(message) { }
        public ForbiddenException(string message, Exception inner) : base(message, inner) { }
    }
}
