using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for lyrics submission specification classes.
/// </summary>
public class SubmissionSpecificationsTests
{
    #region SubmissionByIdSpecification

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
