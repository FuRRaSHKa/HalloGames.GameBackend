using GameBackend.HalloGames.Auth.Api.Data;
using GameBackend.HalloGames.Auth.Api.Models.Credentials;
using GameBackend.HalloGames.Auth.Api.Models.Identities;
using GameBackend.HalloGames.Users.Data;
using GameBackend.HalloGames.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace GameBackend.HalloGames.Auth.Api.AuthProviders
{
    public class PasswordAuthProvider : IAuthProvider<PasswordCredentials>
    {
        private readonly UserDbContext _userDbContext;
        private readonly PasswordDbContext _passwordDbContext;

        public PasswordAuthProvider(UserDbContext userDbContext, PasswordDbContext passwordDbContext)
        {
            _userDbContext = userDbContext;
            _passwordDbContext = passwordDbContext;
        }

        async Task<User?> IAuthProvider<PasswordCredentials>.Login(PasswordCredentials credentials)
        {
            var passwordIdentity = await _passwordDbContext.Passwords.FirstOrDefaultAsync(u => u.ExternalId == credentials.Email);
            if(passwordIdentity == null)
            {
                return null;
            }

            var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.Id == passwordIdentity.UserId);
            if(!BCrypt.Net.BCrypt.Verify(credentials.Password, passwordIdentity.PasswordHash))
            {
                return null;
            }

            return user;
        }

        async Task<User?> IAuthProvider<PasswordCredentials>.Register(PasswordCredentials credentials)
        {
            if(await _passwordDbContext.Passwords.AnyAsync(u => u.ExternalId == credentials.Email))
            {
                return null;
            }

            var user = new User()
            {

            };

             _userDbContext.Users.Add(user);
            await _userDbContext.SaveChangesAsync();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(credentials.Password);
            var passwordIdentity = new PasswordUserIdentity()
            {
                UserId = user.Id,
                ExternalId = credentials.Email,
                PasswordHash = hashedPassword
            };

            _passwordDbContext.Passwords.Add(passwordIdentity);
            await _passwordDbContext.SaveChangesAsync();
            return user;
        }
    }
}
