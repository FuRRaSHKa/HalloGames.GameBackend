using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    public class PasswordAuthProvider : IAuthProvider<PasswordCredentials>
    {
        private readonly AuthDbContext _authDbContext;

        public PasswordAuthProvider(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        async Task<User?> IAuthProvider<PasswordCredentials>.Login(PasswordCredentials credentials)
        {
            var passwordIdentity = await _authDbContext.Passwords.FirstOrDefaultAsync(u => u.ExternalId == credentials.Email);
            if(passwordIdentity == null)
            {
                return null;
            }

            var user = await _authDbContext.Users.FirstOrDefaultAsync(u => u.Id == passwordIdentity.UserId);
            if(!BCrypt.Net.BCrypt.Verify(credentials.Password, passwordIdentity.PasswordHash))
            {
                return null;
            }

            return user;
        }

        async Task<User?> IAuthProvider<PasswordCredentials>.Register(PasswordCredentials credentials)
        {
            if(await _authDbContext.Passwords.AnyAsync(u => u.ExternalId == credentials.Email))
            {
                return null;
            }

            var user = new User()
            {
                Id = Guid.NewGuid(),
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(credentials.Password);
            var passwordIdentity = new PasswordUserIdentity()
            {
                UserId = user.Id,
                ExternalId = credentials.Email,
                PasswordHash = hashedPassword
            };

            _authDbContext.Users.Add(user);
            _authDbContext.Passwords.Add(passwordIdentity);

            await _authDbContext.SaveChangesAsync();
            return user;
        }
    }
}
