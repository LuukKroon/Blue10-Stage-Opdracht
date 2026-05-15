using Microsoft.EntityFrameworkCore;
using PasswordManager.Domain.Entities;
using PasswordManager.Domain.Interfaces;
using PasswordManager.Infrastructure.Data;

namespace PasswordManager.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> HasAnyUsersAsync()
    {
        return await _dbContext.Users.AnyAsync();
    }
    
    public async Task<User?> GetUserAsync()
    {
        return await _dbContext.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync();
    }
}