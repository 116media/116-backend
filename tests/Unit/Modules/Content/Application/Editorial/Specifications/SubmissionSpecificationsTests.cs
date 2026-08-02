using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for lyrics submission specification classes.
/// </summary>
public class SubmissionSpecificationsTests
{
    #region SubmissionByIdSpecification

    [Fact]
    public void SubmissionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        var spec = new SubmissionByIdSpecification(submission.Id);

        // Act
        bool result = spec.IsSatisfiedBy(submission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SubmissionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        var spec = new SubmissionByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(submission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SubmissionByStatusSpecification

    [Fact]
    public void SubmissionByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.Create();
        var spec = new SubmissionByStatusSpecification(EnumSubmissionStatus.Pending);

        // Act
        bool result = spec.IsSatisfiedBy(submission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SubmissionByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
    {
        // Arrange
        LyricsSubmissionEntity submission = LyricsSubmissionFactory.CreateRejected(Guid.NewGuid());
        var spec = new SubmissionByStatusSpecification(EnumSubmissionStatus.Pending);

        // Act
        bool result = spec.IsSatisfiedBy(submission);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
