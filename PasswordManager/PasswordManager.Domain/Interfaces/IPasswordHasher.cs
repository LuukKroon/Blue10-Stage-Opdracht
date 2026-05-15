namespace PasswordManager.Domain.Interfaces;

public interface IPasswordHasher
{
    string HashPassword(string password, byte[] salt);
    bool VerifyPassword(string password, string hash, byte[] salt);
    byte[] GenerateSalt();
}