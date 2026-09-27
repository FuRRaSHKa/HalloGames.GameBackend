namespace GameBackend.HalloGames.Auth.Api.Models.Requests
{
    using System.ComponentModel.DataAnnotations;
    using GameBackend.HalloGames.Auth.Api.Validation;

    public sealed record EmailRegisterRequest
    (
        [Required, EmailAddress, StringLength(254)] string Email,
        [Required, MinLength(8), BcryptPasswordLength] string Password
    );
}
