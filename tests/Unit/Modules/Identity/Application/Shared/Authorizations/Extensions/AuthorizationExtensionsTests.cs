using _116.BuildingBlocks.Constants.Authorization.Policies;
using _116.Identity.Application.Shared.Authorizations.Configuration;
using _116.Identity.Application.Shared.Authorizations.Extensions;
using _116.Identity.Application.Shared.Authorizations.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Authorizations.Extensions;

/// <summary>
/// Unit tests for <see cref="AuthorizationExtensions"/>.
/// </summary>
public class AuthorizationExtensionsTests
{
    #region AddIdentityModuleAuthorization Tests

    [Fact]
    public void AddIdentityModuleAuthorization_ShouldRegisterRequirementHandlers()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging(); // Required for handlers
        services.AddAuthorization(); // Required for IAuthorizationHandler registration
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>()); // Required for AccountStatusRequirementHandler

        // Act
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        IEnumerable<IAuthorizationHandler> handlers = provider.GetServices<IAuthorizationHandler>();
        List<IAuthorizationHandler> authorizationHandlers = handlers.ToList();

        authorizationHandlers.Should().NotBeEmpty("authorization handlers should be registered");
        authorizationHandlers.Should().Contain(h => h.GetType() == typeof(UserRoleRequirementHandler));
        authorizationHandlers.Should().Contain(h => h.GetType() == typeof(AccountStatusRequirementHandler));
    }

    [Fact]
    public void AddIdentityModuleAuthorization_ShouldReturnServiceCollection()
    {
        // Arrange
        ServiceCollection services = [];

        // Act
        IServiceCollection result = services.AddIdentityModuleAuthorization();

        // Assert
        ReferenceEquals(result, services)
            .Should()
            .BeTrue("method should return the same service collection for chaining");
    }

    [Fact]
    public void AddIdentityModuleAuthorization_ShouldConfigureAuthorizationPolicies()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());

        // Act
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        // Assert
        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        policyProvider.Should().NotBeNull("authorization policy provider should be configured");
    }

    [Fact]
    public void AddIdentityModuleAuthorization_ShouldRegisterHandlersAsScoped()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddIdentityModuleAuthorization();

        // Act
        ServiceDescriptor? userRoleHandler = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IAuthorizationHandler) && s.ImplementationType == typeof(UserRoleRequirementHandler)
        );

        ServiceDescriptor? accountStatusHandler = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IAuthorizationHandler)
            && s.ImplementationType == typeof(AccountStatusRequirementHandler)
        );

        // Assert
        userRoleHandler.Should().NotBeNull();
        userRoleHandler.Lifetime.Should().Be(ServiceLifetime.Scoped);

        accountStatusHandler.Should().NotBeNull();
        accountStatusHandler.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task AddIdentityModuleAuthorization_ShouldRegisterSuperAdminPolicy()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        // Act
        AuthorizationPolicy? policy = await policyProvider.GetPolicyAsync(UserRolePolicies.RequireSuperAdminOnly);

        // Assert
        policy.Should().NotBeNull("SuperAdmin policy should be registered");
    }

    [Fact]
    public async Task AddIdentityModuleAuthorization_ShouldRegisterAdminPolicy()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        // Act
        AuthorizationPolicy? policy = await policyProvider.GetPolicyAsync(UserRolePolicies.RequireAdminOnly);

        // Assert
        policy.Should().NotBeNull("Admin policy should be registered");
    }

    [Fact]
    public async Task AddIdentityModuleAuthorization_ShouldRegisterVisitorPolicy()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        // Act
        AuthorizationPolicy? policy = await policyProvider.GetPolicyAsync(UserRolePolicies.RequireVisitorOnly);

        // Assert
        policy.Should().NotBeNull("Visitor policy should be registered");
    }

    [Fact]
    public async Task AddIdentityModuleAuthorization_ShouldRegisterAdminOrSuperAdminPolicy()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();

        // Act
        AuthorizationPolicy? policy = await policyProvider.GetPolicyAsync(UserRolePolicies.RequireAdminOrSuperAdmin);

        // Assert
        policy.Should().NotBeNull("AdminOrSuperAdmin policy should be registered");
    }

    [Fact]
    public async Task AddIdentityModuleAuthorization_ShouldRegisterAccountStatusPolicies()
    {
        // Arrange
        ServiceCollection services = [];
        services.AddLogging();
        services.AddAuthorization();
        services.AddScoped(_ => Mock.Of<_116.Identity.Application.Shared.Repositories.IAuthRepository>());
        services.AddIdentityModuleAuthorization();
        ServiceProvider provider = services.BuildServiceProvider();

        var policyProvider = provider.GetRequiredService<IAuthorizationPolicyProvider>();
        AuthorizationConfiguration config = AuthorizationPolicyConfiguration.GetConfiguration();

        // Act & Assert - Check all account status policies are registered
        foreach (string policyName in config.AccountStatusPolicies.Keys)
        {
            AuthorizationPolicy? policy = await policyProvider.GetPolicyAsync(policyName);
            policy.Should().NotBeNull($"Account status policy '{policyName}' should be registered");
        }
    }

    #endregion

    #region ConfigureJwtBearerEvents Tests

    [Fact]
    public void ConfigureJwtBearerEvents_ShouldConfigureEvents()
    {
        // Arrange
        JwtBearerOptions options = new();

        // Act
        options.ConfigureJwtBearerEvents();

        // Assert
        options.Events.Should().NotBeNull("JWT Bearer events should be configured");
    }

    [Fact]
    public void ConfigureJwtBearerEvents_ShouldConfigureOnChallenge()
    {
        // Arrange
        JwtBearerOptions options = new();

        // Act
        options.ConfigureJwtBearerEvents();

        // Assert
        options.Events.OnChallenge.Should().NotBeNull("OnChallenge event should be configured");
    }

    [Fact]
    public void ConfigureJwtBearerEvents_ShouldConfigureOnForbidden()
    {
        // Arrange
        JwtBearerOptions options = new();

        // Act
        options.ConfigureJwtBearerEvents();

        // Assert
        options.Events.OnForbidden.Should().NotBeNull("OnForbidden event should be configured");
    }

    [Fact]
    public async Task ConfigureJwtBearerEvents_OnChallenge_ShouldReturn401WithProblemDetails()
    {
        // Arrange
        JwtBearerOptions options = new();
        options.ConfigureJwtBearerEvents();

        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        Mock<AuthenticationScheme> schemeMock = new("Bearer", "Bearer", typeof(JwtBearerHandler));

        JwtBearerChallengeContext challengeContext = new(
            httpContext,
            schemeMock.Object,
            options,
            new AuthenticationProperties()
        );

        // Act
        await options.Events.OnChallenge(challengeContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        httpContext.Response.ContentType.Should().Be("application/problem+json");
        challengeContext.Handled.Should().BeTrue("challenge should be marked as handled");

        // Verify response body contains ProblemDetails
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        responseBody.Should().NotBeEmpty("response should contain ProblemDetails JSON");
        responseBody.Should().Contain("title", "response should contain problem title");
    }

    [Fact]
    public async Task ConfigureJwtBearerEvents_OnForbidden_ShouldReturn403WithProblemDetails()
    {
        // Arrange
        JwtBearerOptions options = new();
        options.ConfigureJwtBearerEvents();

        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        Mock<AuthenticationScheme> schemeMock = new("Bearer", "Bearer", typeof(JwtBearerHandler));

        ForbiddenContext forbiddenContext = new(httpContext, schemeMock.Object, options);

        // Act
        await options.Events.OnForbidden(forbiddenContext);

        // Assert
        httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        httpContext.Response.ContentType.Should().Be("application/problem+json");

        // Verify response body contains ProblemDetails
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        responseBody.Should().NotBeEmpty("response should contain ProblemDetails JSON");
        responseBody.Should().Contain("title", "response should contain problem title");
    }

    [Fact]
    public async Task ConfigureJwtBearerEvents_OnChallenge_ShouldSerializeWithCamelCase()
    {
        // Arrange
        JwtBearerOptions options = new();
        options.ConfigureJwtBearerEvents();

        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        Mock<AuthenticationScheme> schemeMock = new("Bearer", "Bearer", typeof(JwtBearerHandler));

        JwtBearerChallengeContext challengeContext = new(
            httpContext,
            schemeMock.Object,
            options,
            new AuthenticationProperties()
        );

        // Act
        await options.Events.OnChallenge(challengeContext);

        // Assert - Verify camelCase JSON serialization
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("\"title\"", "JSON should use camelCase property names");
        responseBody.Should().Contain("\"status\"", "JSON should use camelCase property names");
    }

    [Fact]
    public async Task ConfigureJwtBearerEvents_OnForbidden_ShouldSerializeWithCamelCase()
    {
        // Arrange
        JwtBearerOptions options = new();
        options.ConfigureJwtBearerEvents();

        DefaultHttpContext httpContext = new() { Response = { Body = new MemoryStream() } };

        Mock<AuthenticationScheme> schemeMock = new("Bearer", "Bearer", typeof(JwtBearerHandler));

        ForbiddenContext forbiddenContext = new(httpContext, schemeMock.Object, options);

        // Act
        await options.Events.OnForbidden(forbiddenContext);

        // Assert - Verify camelCase JSON serialization
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(httpContext.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        responseBody.Should().Contain("\"title\"", "JSON should use camelCase property names");
        responseBody.Should().Contain("\"status\"", "JSON should use camelCase property names");
    }

    #endregion
}
