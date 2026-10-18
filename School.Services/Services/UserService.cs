using Microsoft.Extensions.Logging;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Validation;

namespace School.Services.Services;

public class UserService : IUserService
{
    private const string DuplicateLoginMessage = "Не удалось завершить регистрацию. Попробуйте другой логин.";
    private const string UnknownRoleMessage = "Неизвестная роль.";
    private const string OwnRoleMessage = "Нельзя изменить роль самому себе: система может остаться без администратора.";

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

    public async Task<bool> ChangeRoleAsync(int userId, string newRole, string currentLogin)
    {
        if (!Roles.IsKnown(newRole))
        {
            throw new ArgumentException(UnknownRoleMessage);
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (string.Equals(user.Login, currentLogin, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(OwnRoleMessage);
        }

        if (user.Role == newRole)
        {
            return true;
        }

        var updated = await _userRepository.UpdateRoleAsync(userId, newRole);
        if (updated)
        {
            _logger.LogInformation("User {Login} role changed from {OldRole} to {NewRole} by {ChangedBy}", user.Login, user.Role, newRole, currentLogin);
        }
        return updated;
    }
}
