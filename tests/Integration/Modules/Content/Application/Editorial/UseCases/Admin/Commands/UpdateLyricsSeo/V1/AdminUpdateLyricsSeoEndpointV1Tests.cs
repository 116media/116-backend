using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Factories.Content;

namespace _116.Integration.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsSeo.V1;

/// <summary>
/// Integration tests for the AdminUpdateLyricsSeo endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateLyricsSeoEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UpdateLyricsSeo_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateLyricsSeo_AsVisitor_ReturnsForbidden()
    {
        Client.AuthenticateAsVisitor();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdateLyricsSeo_AsAdmin_IsAllowed()
    {
        Client.AuthenticateAsAdmin();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Lyrics, Guid.NewGuid()),
            new { }
        );

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("Lyrics"))
        );
    }

    [Fact]
    public async Task UpdateLyricsSeo_AsSuperAdmin_WithValidData_PersistsMeta()
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
        AdminUpdateLyricsSeoRequest request = new AdminUpdateLyricsSeoRequestBuilder().Build();

        var response = await Client.PatchAsJsonAsync(
            Routes.Admin.Editorial.Seo(EditorialRouteConstants.Lyrics, lyrics.Id),
            request
        );

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminUpdateLyricsSeoResponse>();
        body.Lyrics.Id.Should().Be(lyrics.Id);
        body.Lyrics.MetaTitle.Should().Be(request.MetaTitle);
        body.Lyrics.MetaDescription.Should().Be(request.MetaDescription);

        await using ContentDbContext ctx = CreateDbContext<ContentDbContext>();
        LyricsEntity? persisted = await ctx.Lyrics.FindAsync(lyrics.Id);
        persisted.Should().NotBeNull();
        persisted!.MetaTitle.Should().Be(request.MetaTitle);
        persisted.MetaDescription.Should().Be(request.MetaDescription);
    }
}
