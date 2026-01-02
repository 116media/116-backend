using System.Reflection;

using _116.BuildingBlocks.Constants;

using FluentValidation;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.Auth.Validators;

/// <summary>
/// FluentValidation extensions for file upload validation (avatar, images).
/// </summary>
public static class FileValidation
{
    /// <summary>
    /// Validates avatar file with size, type, and extension constraints.
    /// </summary>
    /// <typeparam name="T">The type being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the avatar file property.</param>
    /// <param name="isRequired">Whether the avatar file is required (default: false).</param>
    /// <returns>The configured rule builder.</returns>
    public static IRuleBuilderOptions<T, IFormFile?> ValidAvatar<T>(
        this IRuleBuilder<T, IFormFile?> ruleBuilder,
        bool isRequired = false
    )
    {
        IRuleBuilderOptions<T, IFormFile?> builder;
        if (isRequired)
        {
            builder = ruleBuilder
                .NotNull().WithMessage("Avatar file is required")
                .Must(predicate: BeValidFileSize)
                .WithMessage($"File size must not exceed {FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)}MB")
                .Must(predicate: BeValidImageType)
                .WithMessage(
                    $"Only image files are allowed: {string.Join(", ", value: FileConstants.AllowedAvatarMimeTypes)}")
                .Must(predicate: BeValidFileExtension)
                .WithMessage(
                    $"Only these extensions are allowed: {string.Join(", ", value: FileConstants.AllowedAvatarExtensions)}");
        }
        else
        {
            builder = ruleBuilder
                .Must(predicate: BeValidFileSize)
                .WithMessage($"File size must not exceed {FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)}MB")
                .Must(predicate: BeValidImageType)
                .WithMessage(
                    $"Only image files are allowed: {string.Join(", ", value: FileConstants.AllowedAvatarMimeTypes)}")
                .Must(predicate: BeValidFileExtension)
                .WithMessage(
                    $"Only these extensions are allowed: {string.Join(", ", value: FileConstants.AllowedAvatarExtensions)}")
                .When(x => GetAvatarFileValue(instance: x) != null);
        }

        return builder;
    }

    /// <summary>
    /// Validates that a file has valid size constraints.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if file size is valid, false otherwise.</returns>
    private static bool BeValidFileSize(IFormFile? file)
    {
        return file?.Length is > 0 and <= FileConstants.MaxAvatarFileSizeBytes;
    }

    /// <summary>
    /// Validates that a file has a valid image MIME type.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if MIME type is valid, false otherwise.</returns>
    private static bool BeValidImageType(IFormFile? file)
    {
        if (file == null)
        {
            return false;
        }

        // Extract content type without parameters (e.g., "image/jpeg" from "image/jpeg; boundary=...")
        string contentType = file.ContentType?.Split(';')[0].Trim().ToLowerInvariant() ?? string.Empty;
        // Accept if content type is in the allowed list
        if (FileConstants.AllowedAvatarMimeTypes.Contains(value: contentType))
        {
            return true;
        }

        // For generic/missing content types (common with mobile uploads), validate by extension
        bool isGenericContentType = string.IsNullOrEmpty(value: contentType) ||
                                    contentType == "application/octet-stream" ||
                                    contentType == "multipart/form-data";
        return isGenericContentType && BeValidFileExtension(file: file);
    }

    /// <summary>
    /// Validates that a file has valid extension.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <returns>True if extension is valid, false otherwise.</returns>
    private static bool BeValidFileExtension(IFormFile? file)
    {
        if (file == null)
        {
            return false;
        }

        string extension = Path.GetExtension(path: file.FileName).ToLowerInvariant();
        return FileConstants.AllowedAvatarExtensions.Contains(value: extension);
    }

    /// <summary>
    /// Gets the AvatarFile property value from an instance using reflection.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="instance">The instance to get the AvatarFile property from.</param>
    /// <returns>The AvatarFile property value, or null if not found.</returns>
    private static IFormFile? GetAvatarFileValue<T>(T instance)
    {
        PropertyInfo? property = typeof(T).GetProperty("AvatarFile");
        return property?.GetValue(obj: instance) as IFormFile;
    }
}
