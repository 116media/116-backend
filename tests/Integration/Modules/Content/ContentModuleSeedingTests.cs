using _116.Content;
using _116.Content.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace _116.Integration.Tests.Modules.Content;

/// <summary>
/// Integration coverage for the seeding branch of <c>UseContentModule</c>. The integration
/// host runs under the Testing environment, where seeding is disabled, so that branch is
/// never exercised at startup. This test drives the extension against the real host service
/// provider with a non-Testing environment so the branch — and the ContentTypeSeeder it
/// invokes — actually runs.
/// </summary>
[Collection("Database")]
public class ContentModuleSeedingTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UseContentModule_WhenSeedingEnabled_RunsContentTypeSeeder()
    {
        string? previousEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            var applicationBuilder = new Mock<IApplicationBuilder>();
            applicationBuilder.Setup(builder => builder.ApplicationServices).Returns(Api.Services);

            IApplicationBuilder result = applicationBuilder.Object.UseContentModule();

            result.Should().BeSameAs(applicationBuilder.Object);

            await using ContentDbContext context = CreateDbContext<ContentDbContext>();
            (await context.ContentTypes.AnyAsync()).Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousEnvironment);
        }
    }
}
