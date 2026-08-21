using System.Reflection;
using _116.Identity.Application.Auth.Exceptions;
using _116.Identity.Application.Auth.Exceptions.Handlers;
using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Exceptions;
using _116.Identity.Domain.StateMachines;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Cross-file contract tests for every concrete <see cref="IExceptionStrategy" /> in the solution.
/// The clauses asserted here come from <see cref="BaseExceptionStrategy{TException}" /> and apply to
/// every implementation, so each per-strategy file keeps only its own status, detail and branches.
/// </summary>
public class ExceptionStrategyContractTests
{
    /// <summary>
    /// The request path the theories assign before invoking a strategy, so a strategy that hardcodes
    /// an instance instead of reading the request fails.
    /// </summary>
    private const string TestRequestPath = "/api/v1/admin/users/123";

    /// <summary>
    /// The number of concrete strategies the scanned assemblies are expected to declare. Update this
    /// deliberately when a strategy is added; the failure is the notification that the new strategy
    /// needs a contract entry.
    /// </summary>
    private const int ExpectedStrategyCount = 25;

    /// <summary>
    /// The production assemblies scanned for strategy implementations, each anchored on a type rather
    /// than a name so a rename is a compile error instead of a silently smaller scan.
    /// </summary>
    private static readonly Assembly[] StrategyAssemblies =
    [
        typeof(IExceptionStrategy).Assembly,
        typeof(AccessDeniedExceptionHandler).Assembly,
    ];

    /// <summary>
    /// The response envelope each strategy is required to produce, keyed on the strategy type.
    /// A strategy missing from this table fails <see cref="ContractFor" /> rather than being skipped.
    /// </summary>
    private static readonly Dictionary<Type, StrategyContract> Contracts = new()
    {
        [typeof(AccessDeniedExceptionHandler)] = new StrategyContract(
            typeof(AccessDeniedException),
            () => new AccessDeniedException("Access denied"),
            StatusCodes.Status403Forbidden,
            nameof(AccessDeniedException),
            CarriesTraceExtensions: true
        ),
        [typeof(AccessTokenExpiryExceptionHandler)] = new StrategyContract(
            typeof(AccessTokenExpiryException),
            () => new AccessTokenExpiryException("Access token expired"),
            StatusCodes.Status401Unauthorized,
            nameof(AccessTokenExpiryException),
            CarriesTraceExtensions: true
        ),
        [typeof(AccountInactiveExceptionHandler)] = new StrategyContract(
            typeof(AccountInactiveException),
            () => new AccountInactiveException("Account is inactive"),
            StatusCodes.Status423Locked,
            nameof(AccountInactiveException),
            CarriesTraceExtensions: true
        ),
        [typeof(AccountNotVerifiedExceptionHandler)] = new StrategyContract(
            typeof(AccountNotVerifiedException),
            () => new AccountNotVerifiedException("Account is not verified"),
            StatusCodes.Status403Forbidden,
            nameof(AccountNotVerifiedException),
            CarriesTraceExtensions: true
        ),
        [typeof(AuthenticationExceptionHandler)] = new StrategyContract(
            typeof(AuthenticationException),
            () => new AuthenticationException("Invalid credentials"),
            StatusCodes.Status401Unauthorized,
            nameof(AuthenticationException),
            CarriesTraceExtensions: true
        ),
        [typeof(AuthorizationExceptionHandler)] = new StrategyContract(
            typeof(AuthorizationException),
            () => new AuthorizationException("Access denied"),
            StatusCodes.Status403Forbidden,
            nameof(AuthorizationException),
            CarriesTraceExtensions: true
        ),
        [typeof(BadGatewayExceptionHandler)] = new StrategyContract(
            typeof(BadGatewayException),
            () => new BadGatewayException("Upstream service unavailable"),
            StatusCodes.Status502BadGateway,
            nameof(BadGatewayException),
            CarriesTraceExtensions: true
        ),
        [typeof(BadRequestExceptionHandler)] = new StrategyContract(
            typeof(BadRequestException),
            () => new BadRequestException("Invalid request"),
            StatusCodes.Status400BadRequest,
            nameof(BadRequestException),
            CarriesTraceExtensions: true
        ),
        [typeof(ConflictExceptionHandler)] = new StrategyContract(
            typeof(ConflictException),
            () => new ConflictException("Resource already exists"),
            StatusCodes.Status409Conflict,
            nameof(ConflictException),
            CarriesTraceExtensions: true
        ),
        [typeof(DbUpdateExceptionStrategy)] = new StrategyContract(
            typeof(DbUpdateException),
            () =>
                new DbUpdateException(
                    "update failed",
                    new PostgresException("duplicate key", "ERROR", "ERROR", PostgresErrorCodes.UniqueViolation)
                ),
            StatusCodes.Status409Conflict,
            "ConflictException",
            CarriesTraceExtensions: true
        ),
        [typeof(DefaultExceptionHandler)] = new StrategyContract(
            typeof(Exception),
            () => new Exception("Unhandled failure"),
            StatusCodes.Status500InternalServerError,
            nameof(Exception),
            CarriesTraceExtensions: true
        ),
        [typeof(OperationCanceledExceptionHandler)] = new StrategyContract(
            typeof(OperationCanceledException),
            () => new OperationCanceledException("The request was cancelled"),
            499,
            nameof(OperationCanceledException),
            CarriesTraceExtensions: true
        ),
        [typeof(FormatExceptionStrategy)] = new StrategyContract(
            typeof(FormatException),
            () => new FormatException("Input string was not in a correct format."),
            StatusCodes.Status400BadRequest,
            nameof(InvalidFormatException),
            CarriesTraceExtensions: true
        ),
        [typeof(InternalServerExceptionHandler)] = new StrategyContract(
            typeof(InternalServerException),
            () => new InternalServerException("Internal error occurred"),
            StatusCodes.Status500InternalServerError,
            nameof(InternalServerException),
            CarriesTraceExtensions: true
        ),
        [typeof(MethodNotAllowedExceptionHandler)] = new StrategyContract(
            typeof(MethodNotAllowedException),
            () => new MethodNotAllowedException("Method not allowed"),
            StatusCodes.Status405MethodNotAllowed,
            nameof(MethodNotAllowedException),
            CarriesTraceExtensions: true
        ),
        [typeof(NotFoundExceptionHandler)] = new StrategyContract(
            typeof(NotFoundException),
            () => new NotFoundException("Resource not found"),
            StatusCodes.Status404NotFound,
            nameof(NotFoundException),
            CarriesTraceExtensions: true
        ),
        [typeof(OtpAttemptsLimitExceptionHandler)] = new StrategyContract(
            typeof(OtpAttemptsLimitException),
            () => new OtpAttemptsLimitException("Too many OTP attempts"),
            StatusCodes.Status429TooManyRequests,
            nameof(OtpAttemptsLimitException),
            CarriesTraceExtensions: true
        ),
        [typeof(OtpExpirationExceptionHandler)] = new StrategyContract(
            typeof(OtpExpirationException),
            () => new OtpExpirationException("The OTP has expired"),
            StatusCodes.Status410Gone,
            nameof(OtpExpirationException),
            CarriesTraceExtensions: true
        ),
        [typeof(RateLimitExceededExceptionHandler)] = new StrategyContract(
            typeof(RateLimitExceededException),
            () => new RateLimitExceededException(TimeSpan.FromSeconds(60)),
            StatusCodes.Status429TooManyRequests,
            nameof(RateLimitExceededException),
            CarriesTraceExtensions: true
        ),
        [typeof(DomainRuleExceptionStrategy)] = new StrategyContract(
            typeof(IdentityRuleException),
            () => new IdentityRuleException(IdentityRuleCodes.RoleNameRequired),
            StatusCodes.Status400BadRequest,
            nameof(BadRequestException),
            CarriesTraceExtensions: true
        ),
        [typeof(RefreshTokenExpiryExceptionHandler)] = new StrategyContract(
            typeof(RefreshTokenExpiryException),
            () => new RefreshTokenExpiryException("Refresh token expired"),
            StatusCodes.Status403Forbidden,
            nameof(RefreshTokenExpiryException),
            CarriesTraceExtensions: true
        ),
        [typeof(ResourceNotFoundExceptionHandler)] = new StrategyContract(
            typeof(ResourceNotFoundException),
            () => new ResourceNotFoundException("Endpoint not found"),
            StatusCodes.Status404NotFound,
            nameof(ResourceNotFoundException),
            CarriesTraceExtensions: true
        ),
        [typeof(ValidationExceptionHandler)] = new StrategyContract(
            typeof(ValidationException),
            () => new ValidationException(new ValidationFailure[] { new("Email", "Email is required") }),
            StatusCodes.Status400BadRequest,
            nameof(ValidationException),
            CarriesTraceExtensions: true
        ),
        [typeof(SocialTokenVerificationExceptionHandler)] = new StrategyContract(
            typeof(SocialTokenVerificationException),
            () => new SocialTokenVerificationException(),
            StatusCodes.Status401Unauthorized,
            nameof(SocialTokenVerificationException),
            CarriesTraceExtensions: true
        ),
        [typeof(UnsupportedProviderExceptionHandler)] = new StrategyContract(
            typeof(UnsupportedProviderException),
            () => new UnsupportedProviderException(EnumAuthProvider.Google),
            StatusCodes.Status400BadRequest,
            nameof(UnsupportedProviderException),
            CarriesTraceExtensions: true
        ),
    };

    /// <summary>
    /// Supplies every concrete strategy type declared by the scanned assemblies. The rows come from
    /// reflection over the type system rather than from a hand-written list, so a strategy added to
    /// either assembly becomes a theory row with no change to this file.
    /// </summary>
    /// <returns>One row per discovered strategy type, ordered by name for stable test output.</returns>
    public static TheoryData<Type> StrategyTypes() => new(DiscoverStrategyTypes());

    /// <summary>
    /// Supplies the discovered strategies whose contract says they route through the shared envelope
    /// helper, and which must therefore carry the trace extensions.
    /// </summary>
    /// <returns>One row per discovered strategy that carries the trace extensions.</returns>
    public static TheoryData<Type> TraceCarryingStrategyTypes() =>
        new(DiscoverStrategyTypes().Where(type => ContractFor(type).CarriesTraceExtensions));

    [Theory]
    [MemberData(nameof(StrategyTypes))]
    public void ExceptionType_ShouldBeTheDeclaredExceptionType(Type strategyType)
    {
        // Arrange
        StrategyContract contract = ContractFor(strategyType);
        IExceptionStrategy strategy = CreateStrategy(strategyType);

        // Act
        Type exceptionType = strategy.ExceptionType;

        // Assert
        exceptionType.Should().Be(contract.ExceptionType, strategyType.Name);
    }

    [Theory]
    [MemberData(nameof(StrategyTypes))]
    public void CreateProblemDetails_ShouldReportTheDeclaredTitleStatusAndInstance(Type strategyType)
    {
        // Arrange
        StrategyContract contract = ContractFor(strategyType);
        IExceptionStrategy strategy = CreateStrategy(strategyType);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        context.Request.Path = TestRequestPath;

        // Act
        ProblemDetails problemDetails = strategy.CreateProblemDetails(contract.CreateException(), context);

        // Assert
        problemDetails.Title.Should().Be(contract.ExpectedTitle, strategyType.Name);
        problemDetails.Status.Should().Be(contract.ExpectedStatus, strategyType.Name);
        problemDetails.Instance.Should().Be(TestRequestPath, strategyType.Name);
        problemDetails.Detail.Should().NotBeNullOrWhiteSpace(strategyType.Name);
    }

    [Theory]
    [MemberData(nameof(TraceCarryingStrategyTypes))]
    public void CreateProblemDetails_ShouldProduceTheStandardEnvelope(Type strategyType)
    {
        // Arrange
        StrategyContract contract = ContractFor(strategyType);
        IExceptionStrategy strategy = CreateStrategy(strategyType);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        context.TraceIdentifier = $"trace-for-{strategyType.Name}";

        // Act
        ProblemDetails problemDetails = strategy.CreateProblemDetails(contract.CreateException(), context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId", strategyType.Name);
        problemDetails.Extensions["traceId"].Should().Be(context.TraceIdentifier, strategyType.Name);
        problemDetails.Extensions.Should().ContainKey("timestamp", strategyType.Name);
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime), strategyType.Name);
    }

    [Fact]
    public void StrategyTypes_ShouldDiscoverEveryConcreteStrategy()
    {
        // Act
        List<Type> discovered = DiscoverStrategyTypes();

        // Assert
        discovered
            .Should()
            .HaveCount(
                ExpectedStrategyCount,
                "the Shared assembly declares 15 strategies and the Identity module adds 10"
            );
    }

    [Fact]
    public void StrategyTypes_ShouldAllCarryTheTraceExtensions()
    {
        // Act
        List<string> exempt = DiscoverStrategyTypes()
            .Where(type => !ContractFor(type).CarriesTraceExtensions)
            .Select(type => type.Name)
            .ToList();

        // Assert
        exempt
            .Should()
            .BeEmpty("every strategy — including the inline fallback — now emits the traceId and timestamp extensions");
    }

    /// <summary>
    /// Enumerates the concrete strategy types declared by the scanned assemblies. The scan reaches the
    /// type system only; it never reads private state.
    /// </summary>
    /// <returns>The discovered strategy types, ordered by name.</returns>
    private static List<Type> DiscoverStrategyTypes() =>
        StrategyAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } && typeof(IExceptionStrategy).IsAssignableFrom(type)
            )
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Looks up the contract a discovered strategy must honour, throwing when it has no entry so a
    /// newly added strategy fails loudly instead of dropping out of the theory.
    /// </summary>
    /// <param name="strategyType">The discovered strategy type.</param>
    /// <returns>The contract registered for the strategy.</returns>
    private static StrategyContract ContractFor(Type strategyType) =>
        Contracts.TryGetValue(strategyType, out StrategyContract? contract)
            ? contract
            : throw new InvalidOperationException(
                $"{strategyType.Name} has no entry in {nameof(Contracts)}. Add its sample exception, "
                    + "expected status and expected title so the contract theory covers it."
            );

    /// <summary>
    /// Activates a discovered strategy through its parameterless constructor. A strategy that gains a
    /// constructor dependency throws here rather than being skipped.
    /// </summary>
    /// <param name="strategyType">The discovered strategy type.</param>
    /// <returns>An instance of the strategy.</returns>
    private static IExceptionStrategy CreateStrategy(Type strategyType) =>
        (IExceptionStrategy)Activator.CreateInstance(strategyType)!;

    /// <summary>
    /// The ProblemDetails envelope a single strategy is required to produce.
    /// </summary>
    /// <param name="ExceptionType">The exception type the strategy declares.</param>
    /// <param name="CreateException">Builds a sample of that exception for the strategy to convert.</param>
    /// <param name="ExpectedStatus">The HTTP status code the ProblemDetails must carry.</param>
    /// <param name="ExpectedTitle">The title the ProblemDetails must carry.</param>
    /// <param name="CarriesTraceExtensions">Whether the strategy emits the traceId and timestamp extensions.</param>
    private sealed record StrategyContract(
        Type ExceptionType,
        Func<Exception> CreateException,
        int ExpectedStatus,
        string ExpectedTitle,
        bool CarriesTraceExtensions
    );
}
