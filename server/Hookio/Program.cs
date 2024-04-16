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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var root = Directory.GetCurrentDirectory();
DotEnv.Load(Path.Combine(root, ".env"));

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(Environment.GetEnvironmentVariable("PG_CONNECTION_STRING")));
builder.Services.AddSingleton<IConnectionMultiplexer>(provider => ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DRAGONFLY_CONNECTION_STRING")!));
builder.Services.AddSingleton<IDataManager, DataManager>();
builder.Services.AddSingleton<IDiscordClientManager, DiscordClientManager>();
builder.Services.AddSingleton<IYoutubeService, YoutubeService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    //options.Events = new JwtBearerEvents()
    //{
    //    OnMessageReceived = context =>
    //    {
    //        context.Token = context.Request.Cookies["Authorization"];
    //        return Task.CompletedTask;
    //    },
    //    OnTokenValidated = context =>
    //    {
    //        if (DateTime.UtcNow.Subtract(context.SecurityToken.ValidFrom).TotalMinutes > 10)
    //        {

    //        }

    //        return Task.CompletedTask;
    //    }
    //};

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

Console.WriteLine("Migrating database");
using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
context.Database.Migrate();
context.SaveChanges();

app.MapControllers();

app.Run();
