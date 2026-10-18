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

        Assert.Equal(new[] { "admin", "teacher" }, users.Select(u => u.Login).ToArray());
    }

    [Fact]
    public async Task GetByIdAsync_ExistingAndMissing()
    {
        var user = new User { Login = "teacher", PasswordHash = "hash", Role = "Teacher" };
        await _repository.AddAsync(user);

        var existing = await _repository.GetByIdAsync(user.Id);
        var missing = await _repository.GetByIdAsync(999);

        Assert.NotNull(existing);
        Assert.Equal("teacher", existing.Login);
        Assert.Null(missing);
    }

    [Fact]
    public async Task UpdateRoleAsync_ExistingUser_ChangesRole()
    {
        var user = new User { Login = "teacher", PasswordHash = "hash", Role = "Teacher" };
        await _repository.AddAsync(user);

        var updated = await _repository.UpdateRoleAsync(user.Id, "Admin");

        Assert.True(updated);
        var saved = await _repository.GetByIdAsync(user.Id);
        Assert.Equal("Admin", saved?.Role);
    }

    [Fact]
    public async Task UpdateRoleAsync_MissingUser_ReturnsFalse()
    {
        var updated = await _repository.UpdateRoleAsync(999, "Admin");

        Assert.False(updated);
    }
}
