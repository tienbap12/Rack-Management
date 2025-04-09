using Rack.Application.Commons.Abstractions;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Entities;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rack.MainInfrastructure.Common.Cryptography
{
    internal sealed class PasswordHasher : IPasswordHasher, IPasswordHashChecker
    {
        public (string, string) HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            }
            try
            {
                byte[] saltBytes = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(saltBytes);
                }
                var salt = Convert.ToBase64String(saltBytes);

                var hash = GenerateSaltedHash(password, salt);

                return (hash, salt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public bool HashesMatch(string password, Account user)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(user.Password))
            {
                return false;
            }

            var computedHash = GenerateSaltedHash(password, user.Salt);

            return string.Equals(user.Password, computedHash);
        }

        private static string GenerateSaltedHash(string plainText, string salt)
        {
            using var algorithm = SHA512.Create();

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] combined = plainBytes.Concat(saltBytes).ToArray();

            return Convert.ToBase64String(algorithm.ComputeHash(combined));
        }
    }
}