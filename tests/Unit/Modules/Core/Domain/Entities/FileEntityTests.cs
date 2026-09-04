using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Domain.Entities;
using _116.Core.Domain.Enums;
using _116.Core.Domain.Events;
using _116.Core.Domain.Exceptions;
using _116.Core.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="FileEntity"/>.
/// </summary>
public class FileEntityTests
{
    private static readonly DateTime Now = new(2026, 9, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly CoreI18n _coreErrors = TestErrorsFactory.CreateCoreI18n();

    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateFile()
    {
        // Arrange
        var id = Guid.NewGuid();
        string fileName = TestConstants.File.ValidFileName;
        string originalFileName = TestConstants.File.ValidOriginalFileName;
        string mimeType = TestConstants.File.ValidMimeType;
        string storageUrl = TestConstants.File.ValidStorageUrl;
        long sizeInBytes = TestConstants.File.ValidSizeInBytes;

        // Act
        var file = FileEntity.Create(id, fileName, originalFileName, mimeType, storageUrl, sizeInBytes);

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().Be(fileName);
        file.OriginalFileName.Should().Be(originalFileName);
        file.MimeType.Should().Be(mimeType);
        file.StorageUrl.Should().Be(storageUrl);
        file.SizeInBytes.Should().Be(sizeInBytes);
        file.IsDeleted.Should().BeFalse();
        file.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutColors_ShouldDefaultColorsToNull()
    {
        // Act
        var file = FileEntity.Create(
            Guid.NewGuid(),
            TestConstants.File.ValidFileName,
            TestConstants.File.ValidOriginalFileName,
            TestConstants.File.ValidMimeType,
            TestConstants.File.ValidStorageUrl,
            TestConstants.File.ValidSizeInBytes
        );

        // Assert
        file.DominantColorHex.Should().BeNull();
        file.ForegroundColorHex.Should().BeNull();
    }

    [Fact]
    public void Create_WithColors_ShouldStoreDominantAndForegroundColors()
    {
        // Act
        var file = FileEntity.Create(
            Guid.NewGuid(),
            TestConstants.File.ValidFileName,
            TestConstants.File.ValidOriginalFileName,
            TestConstants.File.ValidMimeType,
            TestConstants.File.ValidStorageUrl,
            TestConstants.File.ValidSizeInBytes,
            storageKey: TestConstants.File.ValidStorageKey,
            dominantColorHex: "#FFEB3B",
            foregroundColorHex: "#000000"
        );

        // Assert
        file.DominantColorHex.Should().Be("#FFEB3B");
        file.ForegroundColorHex.Should().Be("#000000");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFileName_ShouldThrowBadRequestException(string? invalidFileName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                invalidFileName!,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<CoreRuleException>().Which.Code.Should().Be(CoreRuleCodes.FileNameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidOriginalFileName_ShouldThrowBadRequestException(string? invalidOriginalFileName)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                invalidOriginalFileName!,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<CoreRuleException>().Which.Code.Should().Be(CoreRuleCodes.OriginalFileNameRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMimeType_ShouldThrowBadRequestException(string? invalidMimeType)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                invalidMimeType!,
                TestConstants.File.ValidStorageUrl,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<CoreRuleException>().Which.Code.Should().Be(CoreRuleCodes.MimeTypeRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidStorageUrl_ShouldThrowBadRequestException(string? invalidStorageUrl)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                invalidStorageUrl!,
                TestConstants.File.ValidSizeInBytes
            );

        // Assert
        act.Should().Throw<CoreRuleException>().Which.Code.Should().Be(CoreRuleCodes.StorageUrlRequired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidSizeInBytes_ShouldThrowBadRequestException(long invalidSize)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        Action act = () =>
            FileEntity.Create(
                id,
                TestConstants.File.ValidFileName,
                TestConstants.File.ValidOriginalFileName,
                TestConstants.File.ValidMimeType,
                TestConstants.File.ValidStorageUrl,
                invalidSize
            );

        // Assert
        act.Should().Throw<CoreRuleException>().Which.Code.Should().Be(CoreRuleCodes.FileSizeMustBePositive);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_WhenNotDeleted_ShouldMarkAsDeletedAndReturnTrue()
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        bool result = file.Delete(Now);

        // Assert
        result.Should().BeTrue();
        file.State.Should().Be(EnumFileState.Deleted);
        file.IsDeleted.Should().BeTrue();
        file.DeletedAt.Should().Be(Now);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldReturnFalse()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();

        // Act
        bool result = file.Delete(Now);

        // Assert
        result.Should().BeFalse();
        file.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotUpdateDeletedAt()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        DateTime? originalDeletedAt = file.DeletedAt;

        // Act
        file.Delete(Now);

        // Assert
        file.DeletedAt.Should().Be(originalDeletedAt);
    }

    [Fact]
    public void Delete_WhenNotDeleted_ShouldRaiseSoftDeletedEventWithCapturedStorageKey()
    {
        // Arrange
        FileEntity file = FileFactory.CreateWithStorageKey("avatars/user-1");

        // Act
        file.Delete(Now);

        // Assert
        file.DomainEvents.OfType<FileSoftDeletedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new FileSoftDeletedEvent(file.Id, "avatars/user-1"));
    }

    [Fact]
    public void Delete_WhenFileHasNoStorageKey_ShouldRaiseSoftDeletedEventWithNullKey()
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        file.Delete(Now);

        // Assert
        file.DomainEvents.OfType<FileSoftDeletedEvent>().Should().ContainSingle().Which.StorageKey.Should().BeNull();
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ShouldNotRaiseSoftDeletedEvent()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        file.ClearDomainEvents();

        // Act
        file.Delete(Now);

        // Assert
        file.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region MarkReplaced Tests

    [Fact]
    public void MarkReplaced_WhenNotDeleted_ShouldSoftDeleteAndRaiseReplacedEvent()
    {
        // Arrange
        FileEntity file = FileFactory.CreateWithStorageKey("content/covers/cover-1");

        // Act
        bool result = file.MarkReplaced(Now);

        // Assert
        result.Should().BeTrue();
        file.IsDeleted.Should().BeTrue();
        file.DeletedAt.Should().NotBeNull();
        file.DomainEvents.OfType<FileReplacedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new FileReplacedEvent(file.Id, "content/covers/cover-1"));
        file.DomainEvents.OfType<FileSoftDeletedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void MarkReplaced_WhenFileHasNoStorageKey_ShouldRaiseReplacedEventWithNullKey()
    {
        // Arrange
        FileEntity file = FileFactory.Create();

        // Act
        bool result = file.MarkReplaced(Now);

        // Assert
        result.Should().BeTrue();
        file.DomainEvents.OfType<FileReplacedEvent>().Should().ContainSingle().Which.OldStorageKey.Should().BeNull();
    }

    [Fact]
    public void MarkReplaced_WhenAlreadyDeleted_ShouldReturnFalseAndRaiseNothing()
    {
        // Arrange
        FileEntity file = FileFactory.CreateDeleted();
        file.ClearDomainEvents();

        // Act
        bool result = file.MarkReplaced(Now);

        // Assert
        result.Should().BeFalse();
        file.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Builder Tests

    [Fact]
    public void Factory_CreateWithId_ShouldCreateFileWithSpecifiedId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        FileEntity file = FileFactory.CreateWithId(id);

        // Assert
        file.Id.Should().Be(id);
        file.FileName.Should().NotBeNullOrEmpty();
        file.OriginalFileName.Should().NotBeNullOrEmpty();
        file.MimeType.Should().NotBeNullOrEmpty();
        file.StorageUrl.Should().NotBeNullOrEmpty();
        file.SizeInBytes.Should().BePositive();
    }

    [Fact]
    public void Factory_Create_ShouldCreateValidFile()
    {
        // Arrange & Act
        FileEntity file = FileFactory.Create();

        // Assert
        file.Id.Should().NotBeEmpty();
        file.FileName.Should().NotBeNullOrEmpty();
        file.OriginalFileName.Should().NotBeNullOrEmpty();
        file.MimeType.Should().NotBeNullOrEmpty();
        file.StorageUrl.Should().NotBeNullOrEmpty();
        file.SizeInBytes.Should().BePositive();
        file.IsDeleted.Should().BeFalse();
    }

    #endregion

    #region Claim

    [Fact]
    public void Claim_OnAFreshUpload_ShouldTakeOwnershipAndStampTheTime()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;
        FileEntity file = FileFactory.CreateImage();

        // Act
        bool claimed = file.Claim(Now);

        // Assert
        claimed.Should().BeTrue();
        file.State.Should().Be(EnumFileState.Claimed);
        file.ClaimedAt.Should().Be(Now);
    }

    [Fact]
    public void Claim_CalledTwice_ShouldKeepTheFirstClaim()
    {
        // Arrange
        FileEntity file = FileFactory.CreateImage();
        file.Claim(Now);
        DateTime? firstClaim = file.ClaimedAt;

        // Act
        bool reclaimed = file.Claim(Now);

        // Assert
        reclaimed.Should().BeFalse();
        file.ClaimedAt.Should().Be(firstClaim);
    }

    [Fact]
    public void Claim_ShouldBeUnsetOnAnUploadNothingReferences()
    {
        // Arrange & Act
        FileEntity file = FileFactory.CreateImage();

        // Assert
        file.ClaimedAt.Should().BeNull();
    }

    #endregion


    [Fact]
    public void Claim_AfterDeletion_ShouldBeRefused()
    {
        // Arrange
        FileEntity file = FileFactory.CreateImage();
        file.Delete(Now);

        // Act
        bool claimed = file.Claim(Now);

        // Assert
        claimed.Should().BeFalse();
        file.State.Should().Be(EnumFileState.Deleted);
    }

    [Fact]
    public void MarkReplaced_AfterDeletion_ShouldNotOverwriteTheDeletedState()
    {
        // Arrange
        FileEntity file = FileFactory.CreateImage();
        file.Delete(Now);

        // Act
        bool replaced = file.MarkReplaced(Now.AddMinutes(5));

        // Assert
        replaced.Should().BeFalse();
        file.State.Should().Be(EnumFileState.Deleted);
        file.DeletedAt.Should().Be(Now);
    }
}
