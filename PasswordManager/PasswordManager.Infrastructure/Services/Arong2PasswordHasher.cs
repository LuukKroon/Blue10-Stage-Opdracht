using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using PasswordManager.Domain.Interfaces;

namespace PasswordManager.Infrastructure.Services;

public class Argon2PasswordHasher : IPasswordHasher
{
    private readonly string _pepper;
    private const int DegreeOfParallelism = 8;
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB

    public Argon2PasswordHasher(IConfiguration configuration)
    {
        _pepper = configuration["Security:Pepper"] 
                  ?? throw new ArgumentNullException("De Pepper is niet geconfigureerd in appsettings.json");
    }

    public string HashPassword(string password, byte[] salt)
    {
        var passwordWithPepper = password + _pepper;
        
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passwordWithPepper));
        
        argon2.Salt = salt;
        argon2.DegreeOfParallelism = DegreeOfParallelism;
        argon2.Iterations = Iterations;
        argon2.MemorySize = MemorySize;

        byte[] hash = argon2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyPassword(string password, string hash, byte[] salt)
    {
        var newHash = HashPassword(password, salt);
        return newHash == hash;
    }

    public byte[] GenerateSalt()
    {
        return RandomNumberGenerator.GetBytes(16);
    }
}