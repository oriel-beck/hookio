namespace Hookio.Exceptions
{
    public class EmbedTooLongException : Exception
    {
        public EmbedTooLongException() { }
        public EmbedTooLongException(string message) : base(message) { }
        public EmbedTooLongException(string message, Exception innerException) : base(message, innerException) { }
    }
}
