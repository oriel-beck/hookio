using Hookio.Database.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Hookio.Extensions
{
    public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly IDataManager _dataManager;

        public ConfigureJwtBearerOptions(IDataManager dataManager)
        {
            // ConfigureJwtBearerOptionsis constructed from DI, so we can inject anything here
            _dataManager = dataManager;
        }

        public void Configure(string? name, JwtBearerOptions options)
        {
            // check that we are currently configuring the options for the correct scheme
            if (name == JwtBearerDefaults.AuthenticationScheme)
            {
                options.Events = new JwtBearerEvents()
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["Authorization"] ?? context.Request.Headers.FirstOrDefault(h => h.Key == "Authorization").Value;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = async context =>
                     {
                         var totalTimeFromCreation = DateTime.UtcNow.Subtract(context.SecurityToken.ValidFrom).TotalMinutes;
                         if (totalTimeFromCreation > 10)
                         {
                             await _dataManager.RefreshUserAuthentication(context, context.SecurityToken);
                         }
                     }
                };
            }
        }

        public void Configure(JwtBearerOptions options)
        {
            // default case: no scheme name was specified
            Configure(string.Empty, options);
        }
    }
}
