using System.Text;
using Hookio;
using Hookio.Database;
using Hookio.Database.Interfaces;
using Hookio.Discord;
using Hookio.Discord.Interfaces;
using Hookio.Extensions;
using Hookio.Utils;
using Hookio.Utils.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var root = Directory.GetCurrentDirectory();
DotEnv.Load(Path.Combine(root, ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DRAGONFLY_CONNECTION_STRING")!));
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")));
builder.Services.AddSingleton<ITaskQueue, TaskQueue>();
builder.Services.AddSingleton<IDiscordRequestManager, DiscordRequestManager>();
builder.Services.AddSingleton<IYoutubeService, YoutubeService>();
builder.Services.AddSingleton<IDataManager, DataManager>();

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
/*
 * System.AggregateException: 'Some services are not able to be constructed (Error while validating the service descriptor 'ServiceType: Hookio.Discord.Interfaces.IDiscordRequestManager Lifetime: Singleton ImplementationType: Hookio.Discord.DiscordRequestManager': Unable to resolve service for type 'StackExchange.Redis.ConnectionMultiplexer' while attempting to activate 'Hookio.Discord.DiscordRequestManager'.) (Error while validating the service descriptor 'ServiceType: Hookio.Utils.Interfaces.IYoutubeService Lifetime: Singleton ImplementationType: Hookio.Utils.YoutubeService': Unable to resolve service for type 'StackExchange.Redis.ConnectionMultiplexer' while attempting to activate 'Hookio.Discord.DiscordRequestManager'.) (Error while validating the service descriptor 'ServiceType: Hookio.Database.Interfaces.IDataManager Lifetime: Singleton ImplementationType: Hookio.Database.DataManager': Unable to resolve service for type 'StackExchange.Redis.ConnectionMultiplexer' while attempting to activate 'Hookio.Discord.DiscordRequestManager'.) (Error while validating the service descriptor 'ServiceType: Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler Lifetime: Transient ImplementationType: Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler': Unable to resolve service for type 'StackExchange.Redis.ConnectionMultiplexer' while attempting to activate 'Hookio.Discord.DiscordRequestManager'.) (Error while validating the service descriptor 'ServiceType: Microsoft.Extensions.Options.IConfigureOptions`1[Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions] Lifetime: Singleton ImplementationType: Hookio.Extensions.ConfigureJwtBearerOptions': Unable to resolve service for type 'StackExchange.Redis.ConnectionMultiplexer' while attempting to activate 'Hookio.Discord.DiscordRequestManager'.)'
 */
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
