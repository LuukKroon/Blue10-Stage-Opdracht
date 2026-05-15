using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using System.Security.Cryptography;

namespace PasswordManager.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task SetupMasterPasswordAsync(string masterPassword)
    {
        if (masterPassword.Length < 6 || !masterPassword.Any(char.IsUpper) || !masterPassword.Any(char.IsDigit))
        {
            throw new ArgumentException("Wachtwoord voldoet niet aan de eisen: minimaal 6 tekens, 1 hoofdletter en 1 cijfer vereist.");
        }

        if (await _userRepository.HasAnyUsersAsync())
        {
            throw new InvalidOperationException("Er is al een master password ingesteld!");
        }

        byte[] passwordSalt = _passwordHasher.GenerateSalt();
        string hash = _passwordHasher.HashPassword(masterPassword, passwordSalt);
        byte[] encryptionSalt = RandomNumberGenerator.GetBytes(16);

        var user = new User
        {
            Id = Guid.NewGuid(),
            MasterPasswordHash = hash,
            PasswordSalt = passwordSalt,
            EncryptionSalt = encryptionSalt
        };

        await _userRepository.AddUserAsync(user);
    }

    public async Task<User?> VerifyMasterPasswordAsync(string masterPassword)
    {
        var user = await _userRepository.GetUserAsync();
        if (user == null) return null;

        bool isValid = _passwordHasher.VerifyPassword(masterPassword, user.MasterPasswordHash, user.PasswordSalt);
        
        return isValid ? user : null;
    }
}