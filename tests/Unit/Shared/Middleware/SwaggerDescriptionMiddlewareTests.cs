using System.Text;
using _116.Shared.Application.Middleware;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Middleware;

/// <summary>
/// Unit tests for <see cref="SwaggerDescriptionMiddleware"/>.
/// </summary>
public class SwaggerDescriptionMiddlewareTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidRequestDelegate_ShouldCreateInstance()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act
        SwaggerDescriptionMiddleware middleware = new(next);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region InvokeAsync - Swagger JSON Processing Tests

    [Fact]
    public async Task InvokeAsync_WithSwaggerJsonPath_ShouldProcessDescriptions()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson = @"{""description"": ""Line 1\\nLine 2""}";
        string expectedJson = @"{""description"": ""Line 1\nLine 2""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(expectedJson);
    }

    [Fact]
    public async Task InvokeAsync_WithSwaggerJsonPath_ShouldReplaceDoubleBackslashN()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v2/swagger.json";

        string originalJson = @"{""description"": ""First line\\nSecond line\\nThird line""}";
        string expectedJson = @"{""description"": ""First line\nSecond line\nThird line""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(expectedJson);
    }

    [Fact]
    public async Task InvokeAsync_WithMultipleDescriptions_ShouldProcessAll()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson = @"{""description"": ""First\\nDesc"", ""another"": {""description"": ""Second\\nDesc""}}";
        string expectedJson = @"{""description"": ""First\nDesc"", ""another"": {""description"": ""Second\nDesc""}}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(expectedJson);
    }

    #endregion

    #region InvokeAsync - Non-Swagger Paths Tests

    [Fact]
    public async Task InvokeAsync_WithNonSwaggerPath_ShouldNotProcessResponse()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";

        string originalJson = @"{""description"": ""Line 1\\nLine 2""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(originalJson, "non-swagger paths should not be processed");
    }

    [Fact]
    public async Task InvokeAsync_WithSwaggerPathButNotJson_ShouldNotProcessResponse()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/index.html";

        string originalContent = "<html>Swagger UI</html>";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalContent);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(originalContent, "non-JSON swagger paths should not be processed");
    }

    [Fact]
    public async Task InvokeAsync_WithJsonButNotSwaggerPath_ShouldNotProcessResponse()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/data.json";

        string originalJson = @"{""description"": ""Line 1\\nLine 2""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(originalJson, "non-swagger JSON paths should not be processed");
    }

    #endregion

    #region InvokeAsync - Content Length Tests

    [Fact]
    public async Task InvokeAsync_WithSwaggerJson_ShouldUpdateContentLength()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson = @"{""description"": ""Test\\nDescription""}";
        string expectedJson = @"{""description"": ""Test\nDescription""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        long expectedLength = Encoding.UTF8.GetByteCount(expectedJson);
        context
            .Response.ContentLength.Should()
            .Be(expectedLength, "content length should be updated to match processed content");
    }

    #endregion

    #region InvokeAsync - Edge Cases Tests

    [Fact]
    public async Task InvokeAsync_WithEmptyResponse_ShouldHandleGracefully()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        RequestDelegate next = _ => Task.CompletedTask; // No response body

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync("should handle empty response");
    }

    [Fact]
    public async Task InvokeAsync_WithNoDescriptionField_ShouldReturnUnchangedJson()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson = @"{""title"": ""API"", ""version"": ""1.0""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(originalJson, "JSON without description field should remain unchanged");
    }

    [Fact]
    public async Task InvokeAsync_WithSingleBackslashN_ShouldNotModify()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson = @"{""description"": ""Already formatted\ntext""}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(originalJson, "already properly escaped newlines should not be modified");
    }

    [Fact]
    public async Task InvokeAsync_WithComplexJson_ShouldOnlyModifyDescriptions()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/swagger/v1/swagger.json";

        string originalJson =
            @"{
  ""paths"": {
    ""/api/test"": {
      ""description"": ""Test endpoint\\nwith newlines"",
      ""summary"": ""Test summary\\nignored"",
      ""responses"": {
        ""200"": {
          ""description"": ""Success\\nresponse""
        }
      }
    }
  }
}";
        string expectedJson =
            @"{
  ""paths"": {
    ""/api/test"": {
      ""description"": ""Test endpoint\nwith newlines"",
      ""summary"": ""Test summary\\nignored"",
      ""responses"": {
        ""200"": {
          ""description"": ""Success\nresponse""
        }
      }
    }
  }
}";

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(expectedJson, "should only modify description fields");
    }

    #endregion

    #region Path Matching Tests

    [Theory]
    [InlineData("/swagger/v1/swagger.json", true)]
    [InlineData("/swagger/v2/swagger.json", true)]
    [InlineData("/swagger/swagger.json", true)]
    [InlineData("/swagger/v1/SWAGGER.JSON", true)]
    [InlineData("/swagger/index.html", false)]
    [InlineData("/api/swagger.json", false)]
    [InlineData("/swagger", false)]
    [InlineData("/swagger/", false)]
    public async Task InvokeAsync_WithVariousPaths_ShouldProcessOnlySwaggerJson(string path, bool shouldProcess)
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = path;

        string originalJson = @"{""description"": ""Test\\nLine""}";
        string expectedJson = shouldProcess ? @"{""description"": ""Test\nLine""}" : originalJson;

        RequestDelegate next = async ctx =>
        {
            byte[] bytes = Encoding.UTF8.GetBytes(originalJson);
            await ctx.Response.Body.WriteAsync(bytes);
        };

        using var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        responseStream.Seek(0, SeekOrigin.Begin);
        string result = await new StreamReader(responseStream).ReadToEndAsync();
        result.Should().Be(expectedJson, $"path {path} processing expectation: {shouldProcess}");
    }

    #endregion

    #region Pipeline Tests

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        bool nextWasCalled = false;

        RequestDelegate next = ctx =>
        {
            nextWasCalled = true;
            return Task.CompletedTask;
        };

        SwaggerDescriptionMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextWasCalled.Should().BeTrue("middleware should call next in pipeline");
    }

    #endregion
}
