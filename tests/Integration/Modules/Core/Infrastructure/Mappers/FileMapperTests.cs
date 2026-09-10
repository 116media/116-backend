using _116.Core.Application.Shared.Mappers;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Domain.Entities;
using _116.Core.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Core;
using MapsterMapper;
using CoreMappingRegistration = _116.Core.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Core.Mappers;

/// <summary>
/// Integration tests for <see cref="FileMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class FileMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(CoreMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToFileReferenceDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<CoreDbContext>();
        FileEntity entity = FileFactory.CreateJpeg();
        seedContext.Files.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity loaded = await readContext.Files.FirstAsync(f => f.Id == entity.Id);

        FileReferenceDto dto = loaded.ToFileReferenceDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.FileName.Should().Be(loaded.FileName);
        dto.OriginalFileName.Should().Be(loaded.OriginalFileName);
        dto.MimeType.Should().Be(loaded.MimeType);
        dto.StorageUrl.Should().Be(loaded.StorageUrl);
        dto.SizeInBytes.Should().Be(loaded.SizeInBytes);
        dto.StorageKey.Should().Be(loaded.StorageKey);
    }

    [Fact]
    public async Task ToFileReferenceDto_ShouldMapTheExtractedColours()
    {
        await using var seedContext = CreateDbContext<CoreDbContext>();
        FileEntity entity = FileFactory.CreateWithColors(dominantColorHex: "#101010", foregroundColorHex: "#f0f0f0");
        seedContext.Files.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity loaded = await readContext.Files.FirstAsync(f => f.Id == entity.Id);

        FileReferenceDto dto = loaded.ToFileReferenceDto(_mapper);

        dto.DominantColorHex.Should().Be("#101010");
        dto.ForegroundColorHex.Should().Be("#f0f0f0");
    }

    [Fact]
    public async Task ToFileReferenceDtoOrNull_WithAPersistedFile_ShouldMapIt()
    {
        await using var seedContext = CreateDbContext<CoreDbContext>();
        FileEntity entity = FileFactory.CreatePng();
        seedContext.Files.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity? loaded = await readContext.Files.FirstOrDefaultAsync(f => f.Id == entity.Id);

        FileReferenceDto? dto = loaded.ToFileReferenceDtoOrNull(_mapper);

        dto.Should().NotBeNull();
        dto!.Id.Should().Be(entity.Id);
    }

    [Fact]
    public async Task ToFileReferenceDtoOrNull_WhenTheFileIsAbsent_ShouldReturnNull()
    {
        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity? missing = await readContext.Files.FirstOrDefaultAsync(f => f.Id == Guid.NewGuid());

        FileReferenceDto? dto = missing.ToFileReferenceDtoOrNull(_mapper);

        dto.Should().BeNull();
    }

    [Fact]
    public async Task MapReferenceToFileDto_ShouldPreserveTheWireShape()
    {
        await using var seedContext = CreateDbContext<CoreDbContext>();
        FileEntity entity = FileFactory.CreateJpeg();
        seedContext.Files.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity loaded = await readContext.Files.FirstAsync(f => f.Id == entity.Id);

        FileReferenceDto reference = loaded.ToFileReferenceDto(_mapper);
        FileDto dto = _mapper.Map<FileDto>(reference);

        dto.Id.Should().Be(reference.Id);
        dto.FileName.Should().Be(reference.FileName);
        dto.OriginalFileName.Should().Be(reference.OriginalFileName);
        dto.MimeType.Should().Be(reference.MimeType);
        dto.StorageUrl.Should().Be(reference.StorageUrl);
        dto.SizeInBytes.Should().Be(reference.SizeInBytes);
    }

    [Fact]
    public async Task MapReferenceToFileDto_ShouldNeverReportAStoredFileAsDeleted()
    {
        await using var seedContext = CreateDbContext<CoreDbContext>();
        FileEntity entity = FileFactory.CreateJpeg();
        seedContext.Files.Add(entity);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<CoreDbContext>();
        FileEntity loaded = await readContext.Files.FirstAsync(f => f.Id == entity.Id);

        FileDto dto = _mapper.Map<FileDto>(loaded.ToFileReferenceDto(_mapper));

        dto.IsDeleted.Should().BeFalse();
    }
}
