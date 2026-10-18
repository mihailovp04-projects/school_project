using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Contracts.Exceptions;
using School.Data.Contracts.Repositories;
using School.Domain;
using School.Services.Services;

namespace School.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _service = new UserService(_repositoryMock.Object, NullLogger<UserService>.Instance);
    }

    [Fact]
    public async Task AddAsync_EmptyLogin_ThrowsArgumentException()
    {
        var user = new User { Login = "", Role = "Admin" };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(user));
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("teacher")]
    [InlineData("")]
    public async Task AddAsync_InvalidRole_ThrowsArgumentException(string role)
    {
        var user = new User { Login = "ivanov", Role = role };

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(user));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Teacher")]
    public async Task AddAsync_ValidRole_CallsRepository(string role)
    {
        var user = new User { Login = "ivanov", Role = role };

        await _service.AddAsync(user);

        _repositoryMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangeRoleAsync_UnknownRole_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangeRoleAsync(2, "SuperAdmin", "admin"));

        _repositoryMock.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRoleAsync_MissingUser_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((User?)null);

        var result = await _service.ChangeRoleAsync(2, Roles.Teacher, "admin");

        Assert.False(result);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("ADMIN")]
    public async Task ChangeRoleAsync_OwnAccount_ThrowsArgumentException(string currentLogin)
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1, Login = "admin", Role = Roles.Admin });

        await Assert.ThrowsAsync<ArgumentException>(() => _service.ChangeRoleAsync(1, Roles.Teacher, currentLogin));

        _repositoryMock.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRoleAsync_SameRole_DoesNotUpdate()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2, Login = "teacher", Role = Roles.Teacher });

        var result = await _service.ChangeRoleAsync(2, Roles.Teacher, "admin");

        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateRoleAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ChangeRoleAsync_AnotherUser_UpdatesRole()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new User { Id = 2, Login = "teacher", Role = Roles.Teacher });
        _repositoryMock.Setup(r => r.UpdateRoleAsync(2, Roles.Admin)).ReturnsAsync(true);

        var result = await _service.ChangeRoleAsync(2, Roles.Admin, "admin");

        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateRoleAsync(2, Roles.Admin), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsRepositoryData()
    {
        var users = new List<User> { new() { Login = "admin", Role = Roles.Admin } };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        Assert.Same(users, await _service.GetAllAsync());
    }

    [Fact]
    public async Task GetByLoginAsync_ReturnsRepositoryData()
    {
        var user = new User { Login = "admin", Role = Roles.Admin };
        _repositoryMock.Setup(r => r.GetByLoginAsync("admin")).ReturnsAsync(user);

        Assert.Same(user, await _service.GetByLoginAsync("admin"));
    }

    [Fact]
    public async Task AddAsync_DuplicateLogin_ThrowsArgumentException()
    {
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new DuplicateNameException("duplicate"));

        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(new User { Login = "ivanov", Role = "Teacher" }));
    }
}