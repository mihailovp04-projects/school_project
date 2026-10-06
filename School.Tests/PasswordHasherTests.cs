using School.Services.Security;

namespace School.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_SamePassword_ProducesDifferentHashes()
    {
        var first = PasswordHasher.Hash("password123");
        var second = PasswordHasher.Hash("password123");

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("password123");

        Assert.True(PasswordHasher.Verify("password123", hash));
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("password123");

        Assert.False(PasswordHasher.Verify("password124", hash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-hash")]
    [InlineData("0.AAAA.AAAA")]
    [InlineData("210000.%%%.%%%")]
    public void Verify_MalformedHash_ReturnsFalse(string hash)
    {
        Assert.False(PasswordHasher.Verify("password123", hash));
    }
}