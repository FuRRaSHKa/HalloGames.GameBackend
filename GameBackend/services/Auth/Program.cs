using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Time;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

void ConfigureDatabase(DbContextOptionsBuilder options)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings:DefaultConnection.");

    options.UseNpgsql(connectionString);
}

builder.Services.AddDbContext<AuthDbContext>(ConfigureDatabase);

builder.Services.AddScoped<IAuthProvider<PasswordCredentials>, PasswordAuthProvider>();
builder.Services.AddScoped<IAuthProviderSelector, AuthProviderSelector>();
builder.Services.AddSingleton<ITimeService, SystemTimeService>();
builder.Services.AddScoped<ITokenService>(services => new TokenService(
    services.GetRequiredService<IConfiguration>(),
    services.GetRequiredService<AuthDbContext>(),
    services.GetRequiredService<ITimeService>()));

builder.Services.AddScoped<IAuthService>(services => new AuthService(
    services.GetRequiredService<IAuthProviderSelector>(),
    services.GetRequiredService<ITokenService>()));

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();
