using GameBackend.HalloGames.Auth.Api.Models.Requests;
using GameBackend.HalloGames.Auth.Api.Models.Results;
using GameBackend.HalloGames.Users.Models;
using GameBackend.Tests.Contexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.Tests;

public class PasswordWhitespaceFlowTests
{
    [Theory]
    [InlineData(" pass word")]
    [InlineData("pass word ")]
    [InlineData(" pass word ")]
    public async Task RegisterAndLogin_PreservePasswordWhitespaceThroughoutAuthFlow(string password)
    {
        const string email = "player@example.com";
        using var context = new PasswordAuthTestContext(new StubTokenService());

        var registration = await context.Controller.RegisterByEmail(new EmailRegisterRequest(email, password));

        var user = Assert.IsType<User>(Assert.IsType<OkObjectResult>(registration).Value);
        context.PasswordDb.ChangeTracker.Clear();
        var identity = await context.PasswordDb.Passwords.SingleAsync();
        Assert.Equal(user.Id, identity.UserId);
        Assert.True(BCrypt.Net.BCrypt.Verify(password, identity.PasswordHash));
        Assert.False(BCrypt.Net.BCrypt.Verify(password.Trim(), identity.PasswordHash));

        var login = await context.Controller.LoginByEmail(new EmailLoginRequest(email, password));

        Assert.IsType<TokenResult>(Assert.IsType<OkObjectResult>(login).Value);

        var trimmedLogin = await context.Controller.LoginByEmail(new EmailLoginRequest(email, password.Trim()));

        Assert.IsType<UnauthorizedResult>(trimmedLogin);
    }
}
