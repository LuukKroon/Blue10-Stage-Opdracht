using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;

namespace PasswordManager.Application.Services;

public class VaultService
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IEncryptionService _encryptionService;
    
    private Guid? _currentUserId;
    private byte[]? _sessionKey; 

    public VaultService(ICredentialRepository credentialRepository, IEncryptionService encryptionService)
    {
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
    }

    public void UnlockVault(User user, string masterPassword)
    {
        _currentUserId = user.Id;
        _sessionKey = _encryptionService.DeriveKeyFromMasterPassword(masterPassword, user.EncryptionSalt);
    }

    private void EnsureVaultUnlocked()
    {
        if (_currentUserId == null || _sessionKey == null)
        {
            throw new UnauthorizedAccessException("De kluis is niet ontgrendeld!");
        }
    }

    public async Task AddCredentialAsync(string title, string username, string url, string plainPassword)
    {
        EnsureVaultUnlocked();
        
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(plainPassword))
        {
            throw new ArgumentException("Titel en Wachtwoord mogen niet leeg zijn!");
        }

        var (cipherText, nonce, tag) = _encryptionService.Encrypt(plainPassword, _sessionKey!);

        var credential = new Credential
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserId!.Value,
            Title = title,
            Username = username,
            Url = url,
            EncryptedPassword = cipherText,
            Nonce = nonce,
            Tag = tag,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _credentialRepository.AddCredentialAsync(credential);
    }

    public async Task<List<(Guid Id, string Title, string Username, string Url, string PlainPassword)>> GetAllCredentialsAsync()
    {
        EnsureVaultUnlocked();

        var credentials = await _credentialRepository.GetCredentialsByUserIdAsync(_currentUserId!.Value);
        var decryptedList = new List<(Guid, string, string, string, string)>();

        foreach (var cred in credentials)
        {
            try
            {
                string plainPassword = _encryptionService.Decrypt(cred.EncryptedPassword, cred.Nonce, cred.Tag, _sessionKey!);
                decryptedList.Add((cred.Id, cred.Title, cred.Username, cred.Url ?? "", plainPassword));
            }
            catch (Exception)
            {
                decryptedList.Add((cred.Id, cred.Title, cred.Username, cred.Url ?? "", "[CORRUPT OF ONGELDIGE DATA]"));
            }
        }

        return decryptedList;
    }

    public async Task DeleteCredentialAsync(Guid credentialId)
    {
        EnsureVaultUnlocked();
        var cred = await _credentialRepository.GetCredentialByIdAsync(credentialId, _currentUserId!.Value);
        
        if (cred == null) throw new ArgumentException("Wachtwoord niet gevonden.");

        await _credentialRepository.DeleteCredentialAsync(cred);
    }
}