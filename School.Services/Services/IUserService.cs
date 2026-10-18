using School.Domain;

namespace School.Services.Services;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByLoginAsync(string login);
    Task AddAsync(User user);
    Task<bool> ChangeRoleAsync(int userId, string newRole, string currentLogin);
}