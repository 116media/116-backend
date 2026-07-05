using _116.Identity;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace _116.Integration.Tests.Modules.Identity;

/// <summary>
/// Integration coverage for the seeding branch of <c>UseIdentityModule</c>. The integration
/// host runs under the Testing environment, where seeding is disabled, so that branch is
/// never exercised at startup. This test drives the extension against the real host service
/// provider with a non-Testing environment so the branch — and the SuperAdmin/VisitorRole
/// seeders it invokes — actually run.
/// </summary>
[Collection("Database")]
public class IdentityModuleSeedingTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task UseIdentityModule_WhenSeedingEnabled_RunsIdentitySeeders()
    {
        string? previousEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        try
        {
            var applicationBuilder = new Mock<IApplicationBuilder>();
            applicationBuilder.Setup(builder => builder.ApplicationServices).Returns(Api.Services);

            IApplicationBuilder result = applicationBuilder.Object.UseIdentityModule();

            result.Should().BeSameAs(applicationBuilder.Object);

            await using IdentityDbContext context = CreateDbContext<IdentityDbContext>();
            (await context.Roles.AnyAsync()).Should().BeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", previousEnvironment);
        }
    }
}
