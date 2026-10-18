using School.Domain;

namespace School.Data.Contracts.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByLoginAsync(string login);
    Task AddAsync(User user);
    Task<bool> UpdateRoleAsync(int id, string role);
}