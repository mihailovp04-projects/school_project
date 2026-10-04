using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using School.Data.Repositories;
using School.Domain;
using School.Services.Validation;

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
        EntityValidator.Validate(user);

        try
        {
            await _userRepository.AddAsync(user);
        }
        catch (DbUpdateException)
        {
            throw new ArgumentException("Не удалось завершить регистрацию. Попробуйте другой логин.");
        }

        _logger.LogInformation("User added: {Login} (Role: {Role})", user.Login, user.Role);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        EntityValidator.Validate(user);
        var updated = await _userRepository.UpdateAsync(user);
        if (updated)
        {
            _logger.LogInformation("User updated: Id {Id}", user.Id);
        }
        return updated;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _userRepository.DeleteAsync(id);
        if (deleted)
        {
            _logger.LogInformation("User deleted: Id {Id}", id);
        }
        return deleted;
    }
}