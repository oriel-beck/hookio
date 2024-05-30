namespace Hookio.Exceptions
{
    public class RequiresPremiumException : Exception
    {
        public RequiresPremiumException() { }
        public RequiresPremiumException(string message) : base(message) { }
        public RequiresPremiumException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
