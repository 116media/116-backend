# 03 — Poster Upload Endpoint

## New Use Case

Create a new use case at:
```
src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Commands/UploadCategoryPoster/
├── AdminUploadCategoryPosterCommand.cs
├── AdminUploadCategoryPosterHandler.cs
├── AdminUploadCategoryPosterValidator.cs
├── AdminUploadCategoryPosterMetaField.cs
└── V1/
    └── AdminUploadCategoryPosterEndpointV1.cs
```

## Endpoint

```
PUT /api/v1/admin/categories/{id}/poster
```

| Field | Value |
|-------|-------|
| Method | `PUT` |
| Auth | `RequireAuthorization` (admin) |
| Rate limit | `FileUpload` |
| Content type | `multipart/form-data` |
| Request body | `IFormFile file` |
| Response | `AdminUploadCategoryPosterResult` containing the updated `CategoryDto` |

## Command

```csharp
public record AdminUploadCategoryPosterCommand(string Id, IFormFile File)
    : ICommand<AdminUploadCategoryPosterResult>;

public record AdminUploadCategoryPosterResult(CategoryDto Category);
```

## Handler

Follows the `AdminUploadArticleImageHandler` cover image pattern:

```csharp
public class AdminUploadCategoryPosterHandler(
    ICategoryRepository categoryRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository coreFileRepository,
    IMapper mapper
) : ICommandHandler<AdminUploadCategoryPosterCommand, AdminUploadCategoryPosterResult>
{
    public async Task<AdminUploadCategoryPosterResult> Handle(
        AdminUploadCategoryPosterCommand command,
        CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id, cancellationToken: cancellationToken);

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: category.PosterFileId,
            file: command.File,
            publicId: id.ToString(),
            folder: "content/category-posters",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken);

        category.SetPosterFileId(posterFileId: fileEntity.Id);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id, cancellationToken: cancellationToken);

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUploadCategoryPosterResult(Category: dto);
    }
}
```

Key points:
- Uses `IFileRepository.ReplaceImageFileAsync` — soft-deletes old poster if exists, uploads new one
- Cloudinary folder: `content/category-posters`
- Public ID: the category UUID (same file gets overwritten on re-upload)
- Sets `PosterFileId` on entity via `SetPosterFileId()`
- Returns updated `CategoryDto` with resolved `PosterUrl`

## Validator

```csharp
/// <summary>
/// Validator for the <see cref="AdminUploadCategoryPosterCommand" /> ensuring required fields are provided.
/// </summary>
public class AdminUploadCategoryPosterValidator : AbstractValidator<AdminUploadCategoryPosterCommand>
{
    /// <summary>
    /// Initializes a new instance of <see cref="AdminUploadCategoryPosterValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public AdminUploadCategoryPosterValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Id).IsValidGuid(i18n.Category.Msg.Localizer);

        RuleFor(x => x.File).ValidArticleImageFile(i18n.Category.Msg);
    }
}
```

## Delete Poster

Poster deletion is handled by uploading a new one (old one is soft-deleted via `ReplaceImageFileAsync`). If an explicit "remove poster" action is needed later, add a `DELETE /api/v1/admin/categories/{id}/poster` endpoint — but this is out of scope for now.
