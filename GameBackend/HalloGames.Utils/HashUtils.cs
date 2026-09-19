using System.Security.Cryptography;
using System.Text;

namespace GameBackend.HalloGames.Utils
{
    public static class HashUtils
    {
        public static string HashRefrestToken(string refreshToken)
        {
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
            return tokenHash;
        }

        public static string HashRefrestToken(byte[] refreshToken)
        {
            var tokenHash = Convert.ToHexString(SHA256.HashData(refreshToken));
            return tokenHash;
        }
    }
}
