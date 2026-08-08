using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType.V1;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Builders.Requests.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using FluentValidation;
using FluentValidation.Results;

namespace _116.Integration.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType.V1;

/// <summary>
/// Integration tests for the AdminUpdateContentType endpoint.
/// </summary>
[Collection("Database")]
public class AdminUpdateContentTypeEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private static string ValidationDetail(string property, string message) =>
        new ValidationException([new ValidationFailure(property, message)]).Message;

    [Fact]
    public async Task UpdateContentType_WithNoAuth_ReturnsUnauthorized()
    {
        Client.ClearAuthentication();
        var request = new AdminUpdateContentTypeRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateContentType_AsSuperAdmin_NonExistent_ReturnsNotFound()
    {
        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateContentTypeRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{Guid.NewGuid()}", request);

        await response.ShouldBeProblem<NotFoundException>(
            HttpStatusCode.NotFound,
            Localized<SharedExceptionMessage>(m => m.EntityNotFound("ContentType"))
        );
    }

    [Fact]
    public async Task UpdateContentType_AsSuperAdmin_WithValidData_ReturnsOk()
    {
        ContentTypeEntity contentType = await SeedAsync<ContentDbContext, ContentTypeEntity>(ctx =>
        {
            ContentTypeEntity entity = ContentTypeFactory.Create();
            ctx.ContentTypes.Add(entity);
            return entity;
        });

        Client.AuthenticateAsSuperAdmin();
        var request = new AdminUpdateContentTypeRequestBuilder().Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{contentType.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.ReadAsAsync<AdminUpdateContentTypeResponse>();
        body.ContentType.Id.Should().Be(contentType.Id);
        body.ContentType.Name.Should().Be(request.Name);

        await using ContentDbContext context = CreateDbContext<ContentDbContext>();
        ContentTypeEntity? persisted = await context.ContentTypes.FindAsync(contentType.Id);
        persisted!.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateContentType_WithNameTooLong_ReturnsBadRequest()
    {
        Client.AuthenticateAsSuperAdmin();
        Guid id = Guid.NewGuid();
        var request = new AdminUpdateContentTypeRequestBuilder()
            .WithName(new string('X', TestConstants.ContentType.NameMaxLength + 1))
            .Build();

        var response = await Client.PutAsJsonAsync($"{ApiRoutes.Admin.ContentTypes}/{id}", request);

        await response.ShouldBeProblem<ValidationException>(
            HttpStatusCode.BadRequest,
            ValidationDetail(
                "Name",
                Localized<ContentTypeErrorMessage>(m => m.NameTooLong(ContentConstants.MaxContentTypeNameLength))
            )
        );
    }
}
