namespace Hookio.Exceptions
{
    public class FailedToSubscribeException : Exception
    {
        public FailedToSubscribeException() { }
        public FailedToSubscribeException(string message) : base(message) { }
        public FailedToSubscribeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
