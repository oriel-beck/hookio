using Discord.Rest;
using Hookio.Data;
using Hookio.DataManagers;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Configuration;
using Hookio.Shared.Options;
using Hookio.WebApi.Middlewares;
using Hookio.YouTube;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add configurations
builder.Services.Configure<OAuth2>(builder.Configuration.GetSection(nameof(OAuth2)));
builder.Services.Configure<YouTube>(builder.Configuration.GetSection(nameof(YouTube)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add databases
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("HookioContext")));
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// Caching managers
builder.Services.AddSingleton<IVideosCacheManager, VideosCacheManager>();
builder.Services.AddSingleton<IYouTubeSubscriptionCache, YouTubeSubscriptionCache>();

// Data managers
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IYouTubeManager, YouTubeManager>();

// Actions managers
builder.Services.AddSingleton<INotificationsManager, NotificationsManager>();

// YT Subscriptions cache
builder.Services.AddSingleton<IYouTubeSubscriptionCache, YouTubeSubscriptionCache>();

// Discord REST client for OAuth2
builder.Services.AddSingleton<DiscordRestClient>();

// Http client to centralize requests
builder.Services.AddHttpClient("OAuth2", client =>
{
    client.BaseAddress = new Uri("https://discord.com");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Hookio v0");
});
builder.Services.AddHttpClient("WebhooksCheck");
builder.Services.AddHttpClient("PubSubHubBub", client =>
{
    client.BaseAddress = new Uri("https://pubsubhubbub.appspot.com");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Hookio v0");
});

// Add memory cache for YT subscription requests
builder.Services.AddMemoryCache(options =>
{
    options.ExpirationScanFrequency = TimeSpan.FromHours(1);
});

// Add caching for user sessions
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "hookio_session";
    options.ConfigurationOptions = new ConfigurationOptions()
    {
        AbortOnConnectFail = true,
        EndPoints = { options.Configuration! }
    };
});

// Session data (guilds, user info, etc)
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Hookio.Session";
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Auth cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            }
        };
    });

// Add hosted services
builder.Services.AddHostedService<RefreshSubs>();

// Add controllers 
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Built in middlewares
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseSession();
app.UseCookiePolicy();

// Custom middlewars
app.UseSessionRefresh();

app.MapControllers();

// db migration
Console.WriteLine("Migrating database");
using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
context.Database.Migrate();
context.SaveChanges();

app.Run();
