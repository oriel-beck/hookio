using Discord.Rest;
using Hookio.Data;
using Hookio.DataManagers;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Configuration;
using Hookio.WebApi.Middlewares;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("HookioContext")));

// Data managers
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<IUserService, UserService>();

// Discord REST client for OAuth2
builder.Services.AddSingleton<DiscordRestClient>();

builder.Services.Configure<OAuth2>(builder.Configuration.GetSection(nameof(OAuth2)));

// OAuth2 http client to centralize requests
builder.Services.AddHttpClient("OAuth2", client =>
{
    client.BaseAddress = new Uri("https://discord.com");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Hookio v0");
});

// Session data (guilds, user info, etc)
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Hookio.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(30);
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
});


// Auth cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Custom middlewares
app.UseExtractUser();

// Built in middlewares
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseSession();
app.UseCookiePolicy();

app.MapControllers();

// db migration
Console.WriteLine("Migrating database");
using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
context.Database.Migrate();
context.SaveChanges();

app.Run();
