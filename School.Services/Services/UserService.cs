using School.Data.Repositories;
using School.Domain;

namespace School.Services.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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
    }

    public async Task UpdateAsync(User user)
    {
        ValidateUser(user);
        await _userRepository.UpdateAsync(user);
    }

    public async Task DeleteAsync(int id)
    {
        await _userRepository.DeleteAsync(id);
    }

    private void ValidateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Login))
        {
            throw new ArgumentException("Логин обязателен для заполнения.");
        }

        if (user.Role != "Admin" && user.Role != "Teacher")
        {
            throw new ArgumentException("Роль должна быть либо 'Admin', либо 'Teacher'.");
        }
    }
}