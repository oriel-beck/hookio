using Microsoft.AspNetCore.Mvc;

namespace Hookio.Shared
{
    public class GeneralError : ObjectResult
    {
        public GeneralError(int statusCode, string message) : base(new { statusCode, message }) // This initializes the `Value` property with a custom object
        {
            StatusCode = statusCode;
        }
    }
}
