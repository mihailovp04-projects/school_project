using School.Data.Contracts.Exceptions;
using School.Data.Repositories;
using School.Domain;

namespace School.Tests.Integration;

public class UserRepositoryTests : SqliteTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(ContextFactory);
    }

    [Fact]
    public async Task AddAsync_DuplicateLogin_ThrowsDuplicateNameException()
    {
        await _repository.AddAsync(new User { Login = "teacher", PasswordHash = "hash", Role = "Teacher" });

        await Assert.ThrowsAsync<DuplicateNameException>(() =>
            _repository.AddAsync(new User { Login = "teacher", PasswordHash = "other", Role = "Teacher" }));
    }

    [Fact]
    public async Task GetByLoginAsync_ExistingLogin_ReturnsUser()
    {
        await _repository.AddAsync(new User { Login = "admin", PasswordHash = "hash", Role = "Admin" });

        var user = await _repository.GetByLoginAsync("admin");

        Assert.NotNull(user);
        Assert.Equal("Admin", user.Role);
    }

    [Fact]
    public async Task GetByLoginAsync_MissingLogin_ReturnsNull()
    {
        var user = await _repository.GetByLoginAsync("nobody");

        Assert.Null(user);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await _repository.AddAsync(new User { Login = "admin", PasswordHash = "hash", Role = "Admin" });
        await _repository.AddAsync(new User { Login = "teacher", PasswordHash = "hash", Role = "Teacher" });

        var users = await _repository.GetAllAsync();

        Assert.Equal(2, users.Count);
    }
}
