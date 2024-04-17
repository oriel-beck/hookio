namespace Hookio.Exceptions
{
    public class InvalidChannelURLException : Exception
    {
        public InvalidChannelURLException() { }
        public InvalidChannelURLException(string message) : base(message) { }
        public InvalidChannelURLException(string message, Exception innerException) : base(message, innerException) { }
    }
}
