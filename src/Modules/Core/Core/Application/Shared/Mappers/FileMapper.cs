using _116.Core.Contracts.Application.DTOs;
using _116.Core.Domain.Entities;
using Mapster;
using MapsterMapper;

namespace _116.Core.Application.Shared.Mappers;

/// <summary>
/// Mapster configuration for file mappings: the aggregate to the reference other modules hold,
/// and that reference to the shape that goes on the wire.
/// </summary>
public static class FileMapper
{
    /// <summary>
    /// Registers file mappings into the provided TypeAdapterConfig.
    /// This method does NOT mutate global state.
    /// </summary>
    /// <param name="config">The TypeAdapterConfig to register mappings into.</param>
    public static void Register(TypeAdapterConfig config)
    {
        config.NewConfig<FileEntity, FileReferenceDto>();
        config.NewConfig<FileReferenceDto, FileDto>().Map(dest => dest.IsDeleted, _ => false);
    }

    /// <summary>
    /// Maps a file aggregate to the reference other modules hold.
    /// </summary>
    /// <param name="file">The file entity to map.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto ToFileReferenceDto(this FileEntity file, IMapper mapper)
    {
        return mapper.Map<FileReferenceDto>(file);
    }

    /// <summary>
    /// Maps a file aggregate to its reference, or null when there is no file.
    /// </summary>
    /// <param name="file">The file entity to map, or null.</param>
    /// <param name="mapper">Injected IMapper instance</param>
    /// <returns>The reference, or null.</returns>
    public static FileReferenceDto? ToFileReferenceDtoOrNull(this FileEntity? file, IMapper mapper)
    {
        return file is null ? null : mapper.Map<FileReferenceDto>(file);
    }
}
