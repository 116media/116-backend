using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Application.Configurations;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Logging;
using Polly;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Cloudinary-backed <see cref="ICloudStorageClient" />. The only type in the solution that
/// touches the provider SDK: it translates to and from CloudinaryDotNet, addresses each asset
/// under the resource type its kind implies, and runs every call through one shared resilience
/// pipeline.
/// </summary>
/// <param name="settings">Cloudinary account credentials.</param>
/// <param name="pipeline">The resilience pipeline wrapping every provider call.</param>
/// <param name="httpClient">The transport the SDK sends over.</param>
/// <param name="logger">Logger for provider diagnostics.</param>
public class CloudinaryStorageClient(
    CloudinarySettings settings,
    ResiliencePipeline pipeline,
    HttpClient httpClient,
    ILogger<CloudinaryStorageClient> logger
) : ICloudStorageClient
{
    /// <summary>
    /// Cloudinary accepts at most this many public ids in one batch-delete request.
    /// </summary>
    private const int BatchSize = 100;

    private readonly Cloudinary _cloudinary = Connect(settings, httpClient);

    /// <summary>
    /// Builds the provider client over the supplied transport. The SDK creates its own
    /// <see cref="HttpClient" /> otherwise, which leaves its traffic outside the host's
    /// connection pooling and makes the provider unreachable from a test.
    /// </summary>
    /// <param name="settings">Cloudinary account credentials.</param>
    /// <param name="httpClient">The transport to send over.</param>
    /// <returns>The provider client.</returns>
    private static Cloudinary Connect(CloudinarySettings settings, HttpClient httpClient)
    {
        var cloudinary = new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret))
        {
            Api = { Secure = true },
        };

        cloudinary.Api.Client = httpClient;

        return cloudinary;
    }

    /// <inheritdoc />
    public async Task<CloudStorageAsset> UploadAsync(
        CloudStorageUpload upload,
        CancellationToken cancellationToken = default
    )
    {
        var file = new FileDescription(upload.FileName, upload.Content);

        return upload.Kind switch
        {
            EnumStoredFileKind.Video => Describe(
                await ExecuteAsync(ct => _cloudinary.UploadAsync(BuildVideoParams(file, upload), ct), cancellationToken)
            ),
            EnumStoredFileKind.Raw => Describe(
                await ExecuteAsync(
                    ct => Task.Run(() => _cloudinary.Upload(BuildRawParams(file, upload)), ct),
                    cancellationToken
                )
            ),
            _ => Describe(
                await ExecuteAsync(ct => _cloudinary.UploadAsync(BuildImageParams(file, upload), ct), cancellationToken)
            ),
        };
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(
        string publicId,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var deletionParams = new DeletionParams(publicId) { ResourceType = ToResourceType(kind) };
            DeletionResult result = await ExecuteAsync(
                _ => _cloudinary.DestroyAsync(deletionParams),
                cancellationToken
            );

            if (result.Error is not null)
            {
                logger.LogWarning(
                    "Cloudinary deletion warning for publicId {PublicId}: {ErrorMessage}",
                    publicId,
                    result.Error.Message
                );

                return false;
            }

            logger.LogInformation(
                "Cloudinary deletion result for publicId {PublicId}: {Result}",
                publicId,
                result.Result
            );

            return result.Result == "ok";
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected error deleting Cloudinary resource {PublicId}", publicId);

            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteManyAsync(
        IEnumerable<string> publicIds,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        List<string> keys = [.. publicIds];

        if (keys.Count == 0)
        {
            return true;
        }

        IEnumerable<Task<bool>> batches = Enumerable
            .Range(0, (keys.Count + BatchSize - 1) / BatchSize)
            .Select(index => DeleteBatchAsync([.. keys.Skip(index * BatchSize).Take(BatchSize)], index, kind));

        bool[] results = await Task.WhenAll(batches);

        return results.All(deleted => deleted);
    }

    /// <summary>
    /// Sends one batch-delete request for up to <see cref="BatchSize" /> public ids.
    /// </summary>
    /// <param name="batch">The ids in this batch.</param>
    /// <param name="batchIndex">The batch's position, for diagnostics.</param>
    /// <param name="kind">What the assets are.</param>
    /// <returns>True when the provider removed the batch.</returns>
    private async Task<bool> DeleteBatchAsync(List<string> batch, int batchIndex, EnumStoredFileKind kind)
    {
        try
        {
            var parameters = new DelResParams
            {
                PublicIds = batch,
                Type = "upload",
                ResourceType = ToResourceType(kind),
            };

            DelResResult result = await ExecuteAsync(
                _ => _cloudinary.DeleteResourcesAsync(parameters),
                CancellationToken.None
            );

            if (result.Error is not null)
            {
                logger.LogWarning(
                    "Cloudinary batch deletion warning (batch {BatchIndex}): {ErrorMessage}",
                    batchIndex,
                    result.Error.Message
                );

                return false;
            }

            logger.LogInformation(
                "Successfully deleted {Count} Cloudinary resources in batch {BatchIndex}",
                batch.Count,
                batchIndex
            );

            return true;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unexpected error during Cloudinary batch deletion (batch {BatchIndex})",
                batchIndex
            );

            return false;
        }
    }

    /// <summary>
    /// Maps a stored-file kind to the resource type Cloudinary addresses it under. Deleting a
    /// video or raw asset as an image is a silent no-op that leaks the asset forever.
    /// </summary>
    /// <param name="kind">The stored-file kind.</param>
    /// <returns>The provider resource type.</returns>
    private static ResourceType ToResourceType(EnumStoredFileKind kind) =>
        kind switch
        {
            EnumStoredFileKind.Video => ResourceType.Video,
            EnumStoredFileKind.Raw => ResourceType.Raw,
            _ => ResourceType.Image,
        };

    /// <summary>
    /// Projects a provider upload result onto the neutral shape callers see.
    /// </summary>
    /// <param name="result">The provider result.</param>
    /// <returns>The stored asset.</returns>
    /// <exception cref="InvalidOperationException">The provider reported an error.</exception>
    private static CloudStorageAsset Describe(RawUploadResult result)
    {
        if (result.Error is not null)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        // Raw assets carry no dimensions; the media results override these.
        (int width, int height) = result switch
        {
            ImageUploadResult image => (image.Width, image.Height),
            VideoUploadResult video => (video.Width, video.Height),
            _ => (0, 0),
        };

        return new CloudStorageAsset(
            PublicId: result.PublicId,
            SecureUrl: result.SecureUrl.ToString(),
            Format: result.Format,
            Width: width,
            Height: height,
            Bytes: result.Bytes,
            ResourceType: result.ResourceType
        );
    }

    /// <summary>
    /// The upload parameters every kind shares.
    /// </summary>
    /// <param name="parameters">The parameters to populate.</param>
    /// <param name="file">The asset's bytes and name.</param>
    /// <param name="upload">The upload request.</param>
    /// <typeparam name="TParams">The provider parameter type.</typeparam>
    /// <returns>The populated parameters.</returns>
    private static TParams Populate<TParams>(TParams parameters, FileDescription file, CloudStorageUpload upload)
        where TParams : RawUploadParams
    {
        parameters.File = file;
        parameters.PublicId = upload.PublicId;
        parameters.Folder = upload.Folder;
        parameters.Overwrite = true;
        parameters.UniqueFilename = false;
        parameters.UseFilename = false;

        return parameters;
    }

    /// <summary>
    /// The provider parameters for a image upload.
    /// </summary>
    /// <param name="file">The asset's bytes and name.</param>
    /// <param name="upload">The upload request.</param>
    /// <returns>The parameters.</returns>
    private static ImageUploadParams BuildImageParams(FileDescription file, CloudStorageUpload upload)
    {
        return Populate(new ImageUploadParams(), file, upload);
    }

    /// <summary>
    /// The provider parameters for a video upload.
    /// </summary>
    /// <param name="file">The asset's bytes and name.</param>
    /// <param name="upload">The upload request.</param>
    /// <returns>The parameters.</returns>
    private static VideoUploadParams BuildVideoParams(FileDescription file, CloudStorageUpload upload)
    {
        return Populate(new VideoUploadParams(), file, upload);
    }

    /// <summary>
    /// The provider parameters for a raw upload.
    /// </summary>
    /// <param name="file">The asset's bytes and name.</param>
    /// <param name="upload">The upload request.</param>
    /// <returns>The parameters.</returns>
    private static RawUploadParams BuildRawParams(FileDescription file, CloudStorageUpload upload)
    {
        return Populate(new RawUploadParams(), file, upload);
    }

    /// <summary>
    /// Runs one provider call through the resilience pipeline.
    /// </summary>
    /// <typeparam name="TResult">The provider result type.</typeparam>
    /// <param name="call">The provider call, taking the pipeline's cancellation token.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The provider result.</returns>
    private async Task<TResult> ExecuteAsync<TResult>(
        Func<CancellationToken, Task<TResult>> call,
        CancellationToken cancellationToken
    )
    {
        return await pipeline.ExecuteAsync(async (state, ct) => await state(ct), call, cancellationToken);
    }
}
