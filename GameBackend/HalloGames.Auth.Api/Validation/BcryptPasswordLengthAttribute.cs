using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GameBackend.HalloGames.Auth.Api.Validation;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
public sealed class BcryptPasswordLengthAttribute : ValidationAttribute
{
    public BcryptPasswordLengthAttribute()
        : base("The {0} field must not exceed 72 UTF-8 bytes.")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || value is string password && Encoding.UTF8.GetByteCount(password) <= 72;
}
