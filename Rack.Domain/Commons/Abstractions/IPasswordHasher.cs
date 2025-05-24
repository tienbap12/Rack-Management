namespace Rack.Domain.Commons.Abstractions
{
    /// <summary>
    /// Service for securely hashing and verifying passwords
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes a password with a new random salt
        /// </summary>
        /// <param name="password">The plain text password to hash</param>
        /// <returns>A tuple containing the hashed password and salt</returns>
        (string hashedPassword, string salt) HashPassword(string password);

        /// <summary>
        /// Verifies a password against a stored hash and salt
        /// </summary>
        /// <param name="password">The plain text password to verify</param>
        /// <param name="storedHash">The stored password hash</param>
        /// <param name="storedSalt">The stored salt</param>
        /// <returns>True if the password matches, false otherwise</returns>
        bool VerifyPassword(string password, string storedHash, string storedSalt);
    }
}