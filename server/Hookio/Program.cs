using System.Text;
using System.Threading.RateLimiting;
using Hookio;
using Hookio.Database;
using Hookio.Database.Interfaces;
using Hookio.DataManagers.Utils.Interfaces;
using Hookio.Discord;
using Hookio.Discord.Interfaces;
using Hookio.Extensions;
using Hookio.Feeds;
using Hookio.Feeds.Interfaces;
using Hookio.Health;
using Hookio.Shared;
using Hookio.Twitch;
using Hookio.Twitch.Interfaces;
using Hookio.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var isTesting = builder.Environment.IsEnvironment("Testing");

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "O";
    options.UseUtcTimestamp = true;
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();

if (!isTesting)
{
    builder.Services.AddPooledDbContextFactory<HookioContext>(opt =>
        opt.UseNpgsql(Environment.GetEnvironmentVariable(EnvNames.PgConnectionString)));
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(EnvNames.RedisConnectionString)!));
    builder.Services.AddHostedService<RssWatcherService>();
    builder.Services.AddHostedService<RssCleanupService>();
}

builder.Services.AddSingleton<TaskQueue>();
builder.Services.AddSingleton<ITaskQueue>(sp => sp.GetRequiredService<TaskQueue>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<TaskQueue>());
builder.Services.AddSingleton<IDiscordRequestManager, DiscordRequestManager>();
builder.Services.AddSingleton<IFeedsCacheService, FeedsCacheService>();
builder.Services.AddSingleton<IUserAuthService, UserAuthService>();
builder.Services.AddSingleton<ISubscriptionService, SubscriptionService>();
builder.Services.AddSingleton<IFeedRepository, FeedRepository>();
builder.Services.AddSingleton<ITwitchEventSubService, TwitchEventSubService>();
builder.Services.AddSingleton<TwitchEventSubHandler>();

builder.Services.AddHealthChecks()
    .AddCheck<PostgresHealthCheck>("postgres")
    .AddCheck<RedisHealthCheck>("dragonfly");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("subscriptions", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.FindFirst(AuthConstants.IdClaim)?.Value
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(10),
                QueueLimit = 0
            }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { message = "Too many requests, please try again later..." }, cancellationToken: token);
    };
});

var jwtSecret = Environment.GetEnvironmentVariable(EnvNames.JwtSecret)
    ?? (isTesting ? "hookio-test-jwt-secret-key-32bytes-min" : null)
    ?? throw new InvalidOperationException($"{EnvNames.JwtSecret} is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.FromMinutes(5),
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = AuthConstants.Issuer,
        ValidAudience = AuthConstants.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!isTesting)
{
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Request-Id"] = context.TraceIdentifier;
    using (app.Logger.BeginScope(new Dictionary<string, object> { ["RequestId"] = context.TraceIdentifier }))
    {
        await next();
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (!isTesting)
{
    using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
    context.Database.Migrate();
}

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapControllers();

app.Run();

public partial class Program;
