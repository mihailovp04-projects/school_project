using Microsoft.Extensions.Logging;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class UserService : IUserService
{
    private const string DuplicateLoginMessage = "Не удалось завершить регистрацию. Попробуйте другой логин.";

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
        catch (DuplicateNameException ex)
        {
            throw new ArgumentException(DuplicateLoginMessage, ex);
        }

        _logger.LogInformation("User added: {Login} (Role: {Role})", user.Login, user.Role);
    }
}
