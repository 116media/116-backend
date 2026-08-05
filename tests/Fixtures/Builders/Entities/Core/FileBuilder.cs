using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Core;

/// <summary>
/// Fluent builder for creating <see cref="FileEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; FileFactory only names chains three or more tests share.
/// </summary>
public class FileBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _fileName;
    private string _originalFileName;
    private string _mimeType;
    private string _storageUrl;
    private long _sizeInBytes;
    private bool _isDeleted;
    private string? _storageKey;
    private string? _dominantColorHex;
    private string? _foregroundColorHex;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileBuilder"/> class with random default values.
    /// </summary>
    public FileBuilder()
    {
        _id = Guid.NewGuid();
        string extension = _faker.PickRandom("jpg", "png", "pdf", "docx");
        _fileName = $"{Guid.NewGuid()}.{extension}";
        _originalFileName = $"{_faker.Lorem.Word()}.{extension}";
        _mimeType = GetMimeTypeForExtension(extension);
        _storageUrl =
            $"https://res.cloudinary.com/test/image/upload/v{_faker.Random.Number(100000000, 999999999)}/{_fileName}";
        _sizeInBytes = _faker.Random.Int(1024, (int)TestConstants.File.ValidSizeInBytes);
    }

    /// <summary>
    /// Sets the file ID.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the file name.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithFileName(string fileName)
    {
        _fileName = fileName;
        return this;
    }

    /// <summary>
    /// Sets the original file name.
    /// </summary>
    /// <param name="originalFileName">The original file name.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithOriginalFileName(string originalFileName)
    {
        _originalFileName = originalFileName;
        return this;
    }

    /// <summary>
    /// Sets the MIME type.
    /// </summary>
    /// <param name="mimeType">The MIME type.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithMimeType(string mimeType)
    {
        _mimeType = mimeType;
        return this;
    }

    /// <summary>
    /// Sets the storage URL.
    /// </summary>
    /// <param name="storageUrl">The storage URL.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithStorageUrl(string storageUrl)
    {
        _storageUrl = storageUrl;
        return this;
    }

    /// <summary>
    /// Sets the file size in bytes.
    /// </summary>
    /// <param name="sizeInBytes">The file size in bytes.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithSizeInBytes(long sizeInBytes)
    {
        _sizeInBytes = sizeInBytes;
        return this;
    }

    /// <summary>
    /// Sets the storage key (Cloudinary public ID).
    /// </summary>
    /// <param name="storageKey">The storage key.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithStorageKey(string storageKey)
    {
        _storageKey = storageKey;
        return this;
    }

    /// <summary>
    /// Sets the extracted dominant and foreground color hex values.
    /// </summary>
    /// <param name="dominantColorHex">The dominant color as <c>#RRGGBB</c>, or null.</param>
    /// <param name="foregroundColorHex">The contrasting foreground color as <c>#RRGGBB</c>, or null.</param>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder WithColors(string? dominantColorHex, string? foregroundColorHex)
    {
        _dominantColorHex = dominantColorHex;
        _foregroundColorHex = foregroundColorHex;
        return this;
    }

    /// <summary>
    /// Marks the file as deleted.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder AsDeleted()
    {
        _isDeleted = true;
        return this;
    }

    /// <summary>
    /// Configures the file as a JPEG image.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder AsJpegImage()
    {
        _fileName = $"{Guid.NewGuid()}.jpg";
        _originalFileName = $"{_faker.Lorem.Word()}.jpg";
        _mimeType = "image/jpeg";
        return this;
    }

    /// <summary>
    /// Configures the file as a PNG image.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder AsPngImage()
    {
        _fileName = $"{Guid.NewGuid()}.png";
        _originalFileName = $"{_faker.Lorem.Word()}.png";
        _mimeType = "image/png";
        return this;
    }

    /// <summary>
    /// Configures the file as a PDF document.
    /// </summary>
    /// <returns>The builder instance for chaining.</returns>
    public FileBuilder AsPdfDocument()
    {
        _fileName = $"{Guid.NewGuid()}.pdf";
        _originalFileName = $"{_faker.Lorem.Word()}.pdf";
        _mimeType = "application/pdf";
        return this;
    }

    /// <summary>
    /// Builds the <see cref="FileEntity"/> instance.
    /// </summary>
    /// <returns>A configured FileEntity instance.</returns>
    public FileEntity Build()
    {
        var file = FileEntity.Create(
            _id,
            _fileName,
            _originalFileName,
            _mimeType,
            _storageUrl,
            _sizeInBytes,
            TestErrorsFactory.CreateCoreI18n(),
            _storageKey,
            _dominantColorHex,
            _foregroundColorHex
        );

        if (_isDeleted)
        {
            file.Delete();
        }

        return file;
    }

    /// <summary>
    /// Gets the MIME type for a file extension.
    /// </summary>
    private static string GetMimeTypeForExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream",
        };
    }
}
