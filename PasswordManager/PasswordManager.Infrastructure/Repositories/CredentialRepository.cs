using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Infrastructure.Repositories;

public class CredentialRepository : ICredentialRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CredentialRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCredentialAsync(Credential credential)
    {
        await _dbContext.Credentials.AddAsync(credential);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<List<Credential>> GetCredentialsByUserIdAsync(Guid userId)
    {
        return await _dbContext.Credentials
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Title)
            .ToListAsync();
    }
    
    public async Task<Credential?> GetCredentialByIdAsync(Guid id, Guid userId)
    {
        return await _dbContext.Credentials
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task DeleteCredentialAsync(Credential credential)
    {
        _dbContext.Credentials.Remove(credential);
        await _dbContext.SaveChangesAsync();
    }
}