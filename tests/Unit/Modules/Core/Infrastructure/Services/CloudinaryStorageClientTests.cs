using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Polly;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CloudinaryStorageClient"/>, the one type that speaks to the provider
/// SDK. The SDK is driven over a stub transport, so its own request building and response parsing
/// run for real while nothing leaves the process.
/// </summary>
public class CloudinaryStorageClientTests
{
    private readonly StubCloudinaryTransport _transport = new();
    private readonly Mock<ILogger<CloudinaryStorageClient>> _loggerMock = new();

    private static readonly CloudinarySettings Settings = new()
    {
        CloudName = "test-cloud",
        ApiKey = "test-key",
        ApiSecret = "test-secret",
    };

    /// <summary>
    /// Builds the client over the stub transport and a pass-through pipeline, so a test measures
    /// the adapter rather than the retry policy.
    /// </summary>
    /// <returns>The client.</returns>
    private CloudinaryStorageClient CreateClient() =>
        new(Settings, ResiliencePipeline.Empty, new HttpClient(_transport), _loggerMock.Object);

    /// <summary>
    /// An upload request for the given kind.
    /// </summary>
    /// <param name="kind">What the asset is.</param>
    /// <returns>The request.</returns>
    private static CloudStorageUpload Upload(EnumStoredFileKind kind) =>
        new(new MemoryStream([1, 2, 3]), "asset.jpg", "asset", "folder", kind);

    [Theory]
    [InlineData(EnumStoredFileKind.Image, "/image/upload")]
    [InlineData(EnumStoredFileKind.Video, "/video/upload")]
    [InlineData(EnumStoredFileKind.Raw, "/raw/upload")]
    public async Task UploadAsync_ShouldSendEachKindToItsOwnEndpoint(EnumStoredFileKind kind, string expectedPath)
    {
        await CreateClient().UploadAsync(Upload(kind));

        _transport.Requests.Should().ContainSingle().Which.Should().Contain(expectedPath);
    }

    [Fact]
    public async Task UploadAsync_ShouldProjectEveryFieldTheProviderReturns()
    {
        CloudStorageAsset asset = await CreateClient().UploadAsync(Upload(EnumStoredFileKind.Image));

        asset.PublicId.Should().Be("folder/asset");
        asset.SecureUrl.Should().Be("https://res.cloudinary.test/folder/asset.jpg");
        asset.Format.Should().Be("jpg");
        asset.Width.Should().Be(800);
        asset.Height.Should().Be(600);
        asset.Bytes.Should().Be(1024);
        asset.ResourceType.Should().Be("image");
    }

    [Fact]
    public async Task UploadAsync_ForARawAsset_ShouldReportNoDimensions()
    {
        CloudStorageAsset asset = await CreateClient().UploadAsync(Upload(EnumStoredFileKind.Raw));

        asset.Width.Should().Be(0);
        asset.Height.Should().Be(0);
    }

    [Fact]
    public async Task UploadAsync_WhenTheProviderRefuses_ShouldThrow()
    {
        _transport.NextBody = StubCloudinaryTransport.ProviderError;

        Func<Task> act = () => CreateClient().UploadAsync(Upload(EnumStoredFileKind.Image));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*provider said no*");
    }

    [Theory]
    [InlineData(EnumStoredFileKind.Image, "/image/destroy")]
    [InlineData(EnumStoredFileKind.Video, "/video/destroy")]
    [InlineData(EnumStoredFileKind.Raw, "/raw/destroy")]
    public async Task DeleteAsync_ShouldAddressTheAssetUnderItsOwnResourceType(
        EnumStoredFileKind kind,
        string expectedPath
    )
    {
        bool deleted = await CreateClient().DeleteAsync("folder/asset", kind);

        deleted.Should().BeTrue();
        _transport.Requests.Should().ContainSingle().Which.Should().Contain(expectedPath);
    }

    [Fact]
    public async Task DeleteAsync_WhenTheProviderRefuses_ShouldReportFailureRatherThanThrow()
    {
        _transport.NextBody = StubCloudinaryTransport.ProviderError;

        bool deleted = await CreateClient().DeleteAsync("folder/asset", EnumStoredFileKind.Image);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenTheTransportThrows_ShouldReportFailureRatherThanThrow()
    {
        _transport.NextFailure = new HttpRequestException("cloudinary unreachable");

        bool deleted = await CreateClient().DeleteAsync("folder/asset", EnumStoredFileKind.Image);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenTheProviderReportsNotFound_ShouldReportFailure()
    {
        _transport.NextBody = """{"result":"not found"}""";

        bool deleted = await CreateClient().DeleteAsync("folder/asset", EnumStoredFileKind.Image);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteManyAsync_WithNoKeys_ShouldNotCallTheProvider()
    {
        bool deleted = await CreateClient().DeleteManyAsync([], EnumStoredFileKind.Image);

        deleted.Should().BeTrue();
        _transport.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteManyAsync_ShouldSendOneRequestPerProviderBatch()
    {
        // The provider caps a batch at 100 ids, so 250 must go out as three requests.
        IReadOnlyList<string> keys = [.. Enumerable.Range(0, 250).Select(index => $"folder/asset-{index}")];

        bool deleted = await CreateClient().DeleteManyAsync(keys, EnumStoredFileKind.Video);

        deleted.Should().BeTrue();
        _transport.Requests.Should().HaveCount(3);
        _transport.Requests.Should().AllSatisfy(uri => uri.Should().Contain("/resources/video"));
    }

    [Fact]
    public async Task DeleteManyAsync_WhenTheProviderRefusesABatch_ShouldReportFailure()
    {
        _transport.NextBody = StubCloudinaryTransport.ProviderError;

        bool deleted = await CreateClient().DeleteManyAsync(["folder/asset"], EnumStoredFileKind.Image);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteManyAsync_WhenTheTransportThrows_ShouldReportFailureRatherThanThrow()
    {
        _transport.NextFailure = new HttpRequestException("cloudinary unreachable");

        bool deleted = await CreateClient().DeleteManyAsync(["folder/asset"], EnumStoredFileKind.Image);

        deleted.Should().BeFalse();
    }
}
