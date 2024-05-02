using System.Text;
using Hookio;
using Hookio.Database;
using Hookio.Database.Interfaces;
using Hookio.Discord;
using Hookio.Discord.Interfaces;
using Hookio.Extensions;
using Hookio.Youtube;
using Hookio.Youtube.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using StackExchange.Redis;
using System.Threading.RateLimiting;
using Polly.Retry;

var root = Directory.GetCurrentDirectory();
DotEnv.Load(Path.Combine(root, ".env"));

//var retryOptions = new RetryStrategyOptions
//{
//    MaxRetryAttempts = 1,
//    DelayGenerator = static args =>
//    {
//        if (args.Outcome.Result is HttpResponseMessage responseMessage &&
//            (int)responseMessage.StatusCode == 429)
//        {
//            var retryAt = long.TryParse(responseMessage.Headers.GetValues("x-ratelimit-reset").FirstOrDefault(), out var ms);
//            if (!retryAt) return new ValueTask<TimeSpan?>((TimeSpan?)null);
//            return new ValueTask<TimeSpan?>(TimeSpan.FromMilliseconds(ms));
//        }

//        // Returning null means the retry strategy will use its internal delay for this attempt.
//        return new ValueTask<TimeSpan?>((TimeSpan?)null);
//    }
//};

//var ratelimitPipeline = new ResiliencePipelineBuilder()
//    .AddRateLimiter(new SlidingWindowRateLimiter(
//        new SlidingWindowRateLimiterOptions
//        {
//            PermitLimit = 50,
//            Window = TimeSpan.FromSeconds(1)
//        })).AddRetry(retryOptions).Build();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")));
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DRAGONFLY_CONNECTION_STRING")!));
builder.Services.AddSingleton<IDiscordClientManager, DiscordClientManager>();
builder.Services.AddSingleton<IYoutubeService, YoutubeService>();
builder.Services.AddSingleton<IDataManager, DataManager>();
builder.Services.AddSingleton<TaskQueue>();
builder.Services.AddSingleton<DiscordRequestManager>();
//builder.Services.AddHttpClient("discord-api").ConfigureHttpClient(
//    (serviceProvider, httpClient) =>
//    {

//    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "subscriptions", options =>
    {
        options.PermitLimit = 3;
        options.Window = TimeSpan.FromSeconds(10);
        options.QueueLimit = 0;
    });
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(new { Message = "Too many requests, please try again later..." }, cancellationToken: token);
    };
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ClockSkew = TimeSpan.FromMinutes(5), // Set a reasonable clock skew
        ValidateLifetime = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = false,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!)
        )
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

Console.WriteLine("Migrating database");
using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
context.Database.Migrate();
context.SaveChanges();

app.MapControllers();

app.Run();
