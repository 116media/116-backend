using _116.Core.Application.Shared.Mappers;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Core;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="FileMapper"/>, the single owner of the aggregate-to-reference and
/// reference-to-wire projections.
/// </summary>
public class FileMapperTests
{
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of <see cref="FileMapperTests"/>.
    /// </summary>
    public FileMapperTests()
    {
        _mapper = new Mapper(MappingRegistration.CreateConfiguration());
    }

    [Fact]
    public void ToFileReferenceDto_ShouldCarryEveryFieldTheAggregateHolds()
    {
        // Arrange
        FileEntity file = FileFactory.CreateJpeg();

        // Act
        FileReferenceDto reference = file.ToFileReferenceDto(_mapper);

        // Assert
        reference.Id.Should().Be(file.Id);
        reference.FileName.Should().Be(file.FileName);
        reference.OriginalFileName.Should().Be(file.OriginalFileName);
        reference.MimeType.Should().Be(file.MimeType);
        reference.StorageUrl.Should().Be(file.StorageUrl);
        reference.SizeInBytes.Should().Be(file.SizeInBytes);
        reference.StorageKey.Should().Be(file.StorageKey);
    }

    [Fact]
    public void ToFileReferenceDto_ShouldCarryTheExtractedColours()
    {
        // Arrange
        FileEntity file = FileFactory.CreateWithColors(dominantColorHex: "#101010", foregroundColorHex: "#f0f0f0");

        // Act
        FileReferenceDto reference = file.ToFileReferenceDto(_mapper);

        // Assert
        reference.DominantColorHex.Should().Be("#101010");
        reference.ForegroundColorHex.Should().Be("#f0f0f0");
    }

    [Fact]
    public void ToFileReferenceDtoOrNull_WithAFile_ShouldProject()
    {
        // Arrange
        FileEntity file = FileFactory.CreatePng();

        // Act
        FileReferenceDto? reference = file.ToFileReferenceDtoOrNull(_mapper);

        // Assert
        reference.Should().NotBeNull();
        reference!.Id.Should().Be(file.Id);
    }

    [Fact]
    public void ToFileReferenceDtoOrNull_WithoutAFile_ShouldReturnNull()
    {
        // Arrange
        FileEntity? file = null;

        // Act
        FileReferenceDto? reference = file.ToFileReferenceDtoOrNull(_mapper);

        // Assert
        reference.Should().BeNull();
    }

    [Fact]
    public void Map_ReferenceToWireDto_ShouldPreserveTheShapeConsumersSerialize()
    {
        // Arrange
        FileReferenceDto reference = FileReferenceDtoFactory.CreateJpeg();

        // Act
        FileDto dto = _mapper.Map<FileDto>(reference);

        // Assert
        dto.Id.Should().Be(reference.Id);
        dto.FileName.Should().Be(reference.FileName);
        dto.OriginalFileName.Should().Be(reference.OriginalFileName);
        dto.MimeType.Should().Be(reference.MimeType);
        dto.StorageUrl.Should().Be(reference.StorageUrl);
        dto.SizeInBytes.Should().Be(reference.SizeInBytes);
    }

    [Fact]
    public void Map_ReferenceToWireDto_ShouldNeverReportTheFileAsDeleted()
    {
        // A reference only exists for a live file, so the wire shape must not inherit a
        // deleted flag from anywhere.
        FileReferenceDto reference = FileReferenceDtoFactory.Create();

        // Act
        FileDto dto = _mapper.Map<FileDto>(reference);

        // Assert
        dto.IsDeleted.Should().BeFalse();
    }
}
