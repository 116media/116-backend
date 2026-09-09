using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Domain.Entities;

namespace _116.Tests.Fixtures.Factories.Core;

/// <summary>
/// Builds <see cref="StoredFile" /> handles, the uploaded-but-unrecorded shape the storage
/// contract returns.
/// </summary>
public static class StoredFileFactory
{
    /// <summary>
    /// Wraps a reference in a handle.
    /// </summary>
    /// <param name="reference">The reference the handle carries.</param>
    /// <returns>The handle.</returns>
    public static StoredFile From(FileReferenceDto reference)
    {
        return new StoredFile { Reference = reference };
    }

    /// <summary>
    /// Wraps a file fixture in a handle.
    /// </summary>
    /// <param name="file">The file fixture.</param>
    /// <returns>The handle.</returns>
    public static StoredFile From(FileEntity file)
    {
        return From(file.ToFileReferenceDto());
    }

    /// <summary>
    /// Creates a handle with default values.
    /// </summary>
    /// <returns>The handle.</returns>
    public static StoredFile Create()
    {
        return From(FileReferenceDtoFactory.Create());
    }
}
