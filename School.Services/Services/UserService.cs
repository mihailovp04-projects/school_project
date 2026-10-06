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
}