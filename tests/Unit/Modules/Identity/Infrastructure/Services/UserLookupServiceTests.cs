using _116.Identity.Contracts.Application;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Services;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="UserLookupService"/>.
/// </summary>
public class UserLookupServiceTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly UserLookupService _sut;

    public UserLookupServiceTests()
    {
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new IdentityDbContext(options);
        _sut = new UserLookupService(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region GetUserNameByIdAsync Tests

    [Fact]
    public async Task GetUserNameByIdAsync_WhenUserExists_ShouldReturnUserName()
    {
        // Arrange
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, TestConstants.User.ValidUserName);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        string? result = await _sut.GetUserNameByIdAsync(user.Id);

        // Assert
        result.Should().Be(TestConstants.User.ValidUserName);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        string? result = await _sut.GetUserNameByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserNameByIdAsync_WithCancellationToken_ShouldRespectCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = async () => await _sut.GetUserNameByIdAsync(Guid.NewGuid(), cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetUserNameByIdAsync_WithMultipleUsers_ShouldReturnCorrectUserName()
    {
        // Arrange
        UserEntity user1 = UserFactory.Create("user1@example.com", "user_one");
        UserEntity user2 = UserFactory.Create("user2@example.com", "user_two");
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        // Act
        string? result = await _sut.GetUserNameByIdAsync(user2.Id);

        // Assert
        result.Should().Be("user_two");
    }

    #endregion

    #region GetAuthorInfoByIdAsync Tests

    [Fact]
    public async Task GetAuthorInfoByIdAsync_WhenUserExists_ShouldReturnAuthorInfo()
    {
        // Arrange
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, TestConstants.User.ValidUserName);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        AuthorInfo? result = await _sut.GetAuthorInfoByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().Be(TestConstants.User.ValidUserName);
        result.Email.Should().Be(TestConstants.User.ValidEmail);
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        AuthorInfo? result = await _sut.GetAuthorInfoByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_WhenUserHasNoAvatar_ShouldReturnNullAvatarFileId()
    {
        // Arrange
        UserEntity user = UserFactory.Create(TestConstants.User.ValidEmail, TestConstants.User.ValidUserName);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        AuthorInfo? result = await _sut.GetAuthorInfoByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.AvatarFileId.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthorInfoByIdAsync_WhenUserHasRole_ShouldReturnRoleName()
    {
        // Arrange
        UserEntity user = UserFactory.CreateSuperAdmin();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        AuthorInfo? result = await _sut.GetAuthorInfoByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Role.Should().NotBeNullOrEmpty();
    }

    #endregion
}
