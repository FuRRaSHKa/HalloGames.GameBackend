using GameBackend.HalloGames.Auth.Api.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GameBackend.Tests;

public class AuthRequestValidationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("        ")]
    public void Requests_RejectMissingPasswords(string? password)
    {
        AssertInvalid(new EmailRegisterRequest("player@example.com", password!), "Password");
        AssertInvalid(new EmailLoginRequest("player@example.com", password!), "Password");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    public void Requests_RejectInvalidEmails(string? email)
    {
        AssertInvalid(new EmailRegisterRequest(email!, "password"), "Email");
        AssertInvalid(new EmailLoginRequest(email!, "password"), "Email");
    }

    [Fact]
    public void Requests_RejectEmailsExceedingMaximumLength()
    {
        var email = new string('a', 243) + "@example.com";
        AssertInvalid(new EmailRegisterRequest(email, "password"), "Email");
        AssertInvalid(new EmailLoginRequest(email, "password"), "Email");
    }

    [Fact]
    public void Registration_RequiresEightCharacters_ButLoginAcceptsExistingShortPasswords()
    {
        AssertInvalid(new EmailRegisterRequest("player@example.com", "1234567"), "Password");
        Assert.True(Validate(new EmailRegisterRequest("player@example.com", "12345678")).IsValid);
        Assert.True(Validate(new EmailLoginRequest("player@example.com", "1234567")).IsValid);
    }

    [Theory]
    [InlineData('a', 72)]
    [InlineData('я', 36)]
    public void Requests_EnforceBcryptLimitInUtf8Bytes(char character, int count)
    {
        var password = new string(character, count);
        Assert.True(Validate(new EmailRegisterRequest("player@example.com", password)).IsValid);
        Assert.True(Validate(new EmailLoginRequest("player@example.com", password)).IsValid);
        AssertInvalid(new EmailRegisterRequest("player@example.com", password + character), "Password");
        AssertInvalid(new EmailLoginRequest("player@example.com", password + character), "Password");
    }

    [Fact]
    public void Requests_PreservePasswordWhitespace()
    {
        const string password = " pass word ";
        var register = new EmailRegisterRequest("player@example.com", password);
        var login = new EmailLoginRequest("player@example.com", password);
        Assert.True(Validate(register).IsValid);
        Assert.True(Validate(login).IsValid);
        Assert.Equal(password, register.Password);
        Assert.Equal(password, login.Password);
    }

    private static void AssertInvalid(object request, string field)
    {
        var state = Validate(request);
        Assert.False(state.IsValid);
        Assert.NotEmpty(state[field]!.Errors);
    }

    private static ModelStateDictionary Validate(object request)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        using var provider = services.BuildServiceProvider();
        var context = new ActionContext(
            new DefaultHttpContext { RequestServices = provider },
            new RouteData(), new ActionDescriptor());
        provider.GetRequiredService<IObjectModelValidator>()
            .Validate(context, null, string.Empty, request);
        return context.ModelState;
    }
}
