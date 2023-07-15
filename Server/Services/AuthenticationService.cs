using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Library;
using Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Server.Services
{


    public class AuthenticationService : IAuthenticationService
    {
            /*
            private readonly IUserRepository _userRepository;

            public AuthenticationService(IUserRepository userRepository)
            {
                _userRepository = userRepository;
            }

            public bool Authenticate(string username, string password)
            {
                var user = _userRepository.GetUserByUsername(username);

                if (user == null)
                {
                    return false;
                }

                var hashedPassword = ComputeHash(password, user.Salt);

                return hashedPassword == user.HashedPassword;
            }
            private static string ComputeHash(string password, string saltString)
            {
                byte[] salt = Encoding.UTF8.GetBytes(saltString);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                byte[] combinedBytes = new byte[salt.Length + passwordBytes.Length];
                Buffer.BlockCopy(salt, 0, combinedBytes, 0, salt.Length);
                Buffer.BlockCopy(passwordBytes, 0, combinedBytes, salt.Length, passwordBytes.Length);

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            */

        private readonly Settings _settings;
        private readonly GameDBContext _context;

        public AuthenticationService(Settings settings, GameDBContext context)
        {
            _settings = settings;
            _context = context;
        }

        public (bool success, string content) Register(string username, string password)
        {
            if (_context.Users.Any(u => u.Username == username)) return (false, "Username not available");

            var user = new User { Username = username, PasswordHash=password };
            user.ProvideSaltAndHash();

            _context.Users.Add(user);
            _context.SaveChanges();

            return (true, "");
        }

        public (bool success, string token) Login(string username, string password)
        {
            var user = _context.Users.Include(u=>u.Heroes).FirstOrDefault(u => u.Username == username);
            if (user == null) return (false, "Invalid");
            
            if (user.PasswordHash != AuthenticationHelpers.ComputeHash(password, user.Salt)) return (false, "Invalid");

            return (true, GenerateJwtToken(AssembleClaimsIdentity(user)));
        }

        private ClaimsIdentity AssembleClaimsIdentity(User user)
        {
            var subject = new ClaimsIdentity(new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("heroes", JsonConvert.SerializeObject(user.Heroes.Select(h=> h.Id)))
            }); ;
            return subject;
        }
        private string GenerateJwtToken(ClaimsIdentity subject)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_settings.BearerKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.Now.AddYears(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)

            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public interface IAuthenticationService
    {
        (bool success, string content) Register(string username, string password);
        (bool success, string token) Login(string username, string password);
    }

    public static class AuthenticationHelpers
    {
        public static void ProvideSaltAndHash(this User user)
        {
            var salt = GenerateSalt();
            user.Salt = Convert.ToBase64String(salt);
            user.PasswordHash = ComputeHash(user.PasswordHash, user.Salt);
        }
        private static byte[] GenerateSalt()
        {
            var rng = RandomNumberGenerator.Create();
            var salt = new byte[24];
            rng.GetBytes(salt);
            return salt;

        }

        public static string ComputeHash(string password, string saltString) { 
            var salt = Convert.FromBase64String(saltString);

            using var hashGenerator = new Rfc2898DeriveBytes(password, salt);
            hashGenerator.IterationCount = 10101;
            var bytes = hashGenerator.GetBytes(24);
            return Convert.ToBase64String(bytes);
        }
    }
}
