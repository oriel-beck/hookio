using Discord.Rest;
using Hookio.Data;
using Hookio.DataManagers;
using Hookio.DataManagers.Interfaces;
using Hookio.Shared.Configuration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddPooledDbContextFactory<HookioContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("HookioContext")));
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<DiscordRestClient>();

builder.Services.Configure<OAuth2>(builder.Configuration.GetSection(nameof(OAuth2)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Console.WriteLine("Migrating database");
using var context = app.Services.GetRequiredService<IDbContextFactory<HookioContext>>().CreateDbContext();
context.Database.Migrate();
context.SaveChanges();

app.Run();
