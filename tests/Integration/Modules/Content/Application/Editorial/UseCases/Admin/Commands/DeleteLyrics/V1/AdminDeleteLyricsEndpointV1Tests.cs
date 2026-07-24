using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics.V1;

/// <summary>
/// Integration tests for the AdminDeleteLyrics endpoint.
/// </summary>
[Collection("Database")]
public class AdminDeleteLyricsEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task DeleteLyrics_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteLyrics_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsAdmin_ReturnsForbidden()
    {
        Client.AuthenticateAsAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_WithNonExistentId_ReturnsError()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid nonExistentId = Guid.NewGuid();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{nonExistentId}");

        await response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifies that deleting an existing standalone lyrics page succeeds, returns
    /// IsSuccess true, and removes the row from the database.
    /// </summary>
    [Fact]
    public async Task DeleteLyrics_AsSuperAdmin_RemovesLyrics()
    {
        LyricsEntity lyrics = await SeedAsync<ContentDbContext, LyricsEntity>(ctx =>
        {
            ContentTypeEntity contentType = ContentTypeFactory.Create();
            CategoryEntity category = CategoryFactory.Create(contentType.Id);
            LyricsEntity l = LyricsFactory.Create(category.Id);
            ctx.ContentTypes.Add(contentType);
            ctx.Categories.Add(category);
            ctx.Lyrics.Add(l);
            return l;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.DeleteAsync($"{ApiRoutes.Admin.Lyrics}/{lyrics.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminDeleteLyricsResponse>();
        body.IsSuccess.Should().BeTrue();

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().BeNull();
    }
}
