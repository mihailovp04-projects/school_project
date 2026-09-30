using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _userRepository.GetByLoginAsync(login);
    }

    public async Task AddAsync(User user)
    {
        ValidateUser(user);
        await _userRepository.AddAsync(user);
        _logger.LogInformation("User added: {Login} (Role: {Role})", user.Login, user.Role);
    }

    public async Task UpdateAsync(User user)
    {
        ValidateUser(user);
        await _userRepository.UpdateAsync(user);
        _logger.LogInformation("User updated: Id {Id}", user.Id);
    }

    public async Task DeleteAsync(int id)
    {
        await _userRepository.DeleteAsync(id);
        _logger.LogInformation("User deleted: Id {Id}", id);
    }

    private void ValidateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login))
        {
            _logger.LogWarning("User validation failed: empty login");
            throw new ArgumentException("Логин обязателен для заполнения.");
        }

        if (user.Role != "Admin" && user.Role != "Teacher")
        {
            _logger.LogWarning("User validation failed: invalid role {Role}", user.Role);
            throw new ArgumentException("Роль должна быть либо 'Admin', либо 'Teacher'.");
        }
    }
}