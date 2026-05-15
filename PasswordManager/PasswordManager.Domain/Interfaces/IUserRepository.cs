using PasswordManager.Domain.Entities;

namespace PasswordManager.Domain.Interfaces;

public interface IUserRepository
{
    Task AddUserAsync(User user);
    Task<bool> HasAnyUsersAsync();
    Task<User?> GetUserAsync();
}