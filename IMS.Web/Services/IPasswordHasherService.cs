using IMS.Web.Models;

namespace IMS.Web.Services
{
    /// <summary>
    /// Contract for secure password hashing and verification services.
    /// </summary>
    public interface IPasswordHasherService
    {
        /// <summary>
        /// Hashes a plain-text password using ASP.NET Core Identity PBKDF2 cryptography.
        /// </summary>
        string HashPassword(User user, string password);

        /// <summary>
        /// Verifies a provided plain-text password against an existing hash.
        /// </summary>
        bool VerifyHashedPassword(User user, string hashedPassword, string providedPassword);
    }
}