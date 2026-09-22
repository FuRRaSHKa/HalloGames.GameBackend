using GameBackend.HalloGames.Auth.Api.AuthProviders;
using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Services;
using GameBackend.HalloGames.Tokens;
using GameBackend.HalloGames.Tokens.Data;
using GameBackend.HalloGames.Users.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

void ConfigureDatabase(DbContextOptionsBuilder options)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not configured. Set ConnectionStrings:DefaultConnection.");

    options.UseNpgsql(connectionString);
}

builder.Services.AddDbContext<UserDbContext>(ConfigureDatabase);
builder.Services.AddDbContext<PasswordDbContext>(ConfigureDatabase);
builder.Services.AddDbContext<RefreshTokenDbContext>(ConfigureDatabase);

builder.Services.AddScoped<IAuthProvider<PasswordCredentials>, PasswordAuthProvider>();
builder.Services.AddScoped<IAuthProviderSelector, AuthProviderSelector>();
builder.Services.AddScoped<ITokenService>(services => new TokenService(
    services.GetRequiredService<IConfiguration>(),
    services.GetRequiredService<RefreshTokenDbContext>(), 
    services.GetRequiredService<UserDbContext>()));

builder.Services.AddScoped<AuthService>(services => new AuthService(
    services.GetRequiredService<IAuthProviderSelector>(),
    services.GetRequiredService<ITokenService>(),
    services.GetRequiredService<UserDbContext>()));

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
