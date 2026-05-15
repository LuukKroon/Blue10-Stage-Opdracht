using PasswordManager.Domain.Entities;

namespace PasswordManager.Domain.Interfaces;

public interface ICredentialRepository
{
    Task AddCredentialAsync(Credential credential);
    Task<List<Credential>> GetCredentialsByUserIdAsync(Guid userId);
    Task<Credential?> GetCredentialByIdAsync(Guid id, Guid userId);
    Task DeleteCredentialAsync(Credential credential);
}