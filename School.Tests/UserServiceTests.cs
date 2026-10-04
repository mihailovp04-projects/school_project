using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using School.Data.Repositories;
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
}