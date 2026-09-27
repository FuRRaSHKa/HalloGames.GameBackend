namespace GameBackend.HalloGames.Auth.Api.Models.Requests
{
    using System.ComponentModel.DataAnnotations;
    using GameBackend.HalloGames.Auth.Api.Validation;

    public sealed record EmailLoginRequest
    (
        [Required, EmailAddress, StringLength(254)] string Email,
        [Required, BcryptPasswordLength] string Password
    );
}
