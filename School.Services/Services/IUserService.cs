using School.Domain;

namespace School.Services.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByLoginAsync(string login);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}