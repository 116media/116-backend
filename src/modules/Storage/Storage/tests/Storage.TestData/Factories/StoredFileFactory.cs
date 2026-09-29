using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Domain.Entities;

namespace _116.Storage.TestData.Factories;

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
}
