# Test Patterns & Conventions

All patterns derived from reading Identity and Core module tests.

## Frameworks & Versions

- xUnit v3 (xunit.v3 1.1.0)
- AwesomeAssertions 9.0.0 (NOT FluentAssertions — import `using AwesomeAssertions;`)
- Moq 4.20.72
- FluentValidation.TestHelper (part of FluentValidation 12.0.0)
- Bogus 35.6.3
- Microsoft.EntityFrameworkCore.InMemory 9.0.4

## Namespace Pattern

```
_116.Unit.Tests.Modules.Content.Domain.Entities
_116.Unit.Tests.Modules.Content.Application.Shared.Errors
_116.Unit.Tests.Modules.Content.Application.Lookup.Specifications
_116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType
_116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory
_116.Unit.Tests.Modules.Content.Infrastructure
```

## Test Method Naming

Pattern: `[Method]_[Condition]_[ExpectedBehavior]`

Examples:
- `Create_WithValidParameters_ShouldCreateContentType`
- `Activate_WhenAlreadyActive_ShouldReturnFalse`
- `Handle_WhenNameAlreadyExists_ShouldThrowConflictException`
- `Validate_WithNullName_ShouldHaveError`
- `Validate_WithValidCommand_ShouldNotHaveErrors`

## Test Structure (AAA)

```csharp
[Fact]
public async Task Handle_WhenNameAlreadyExists_ShouldThrowConflictException()
{
    // Arrange
    string name = "Article";
    var command = new CreateContentTypeCommand(Name: name);
    _lookupRepositoryMock.SetupContentTypeExistsByName(name, true);

    // Act
    Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<ConflictException>();
}
```

## Region Organization

```csharp
#region Create Tests
// ...
#endregion

#region Activate Tests
// ...
#endregion

#region Success Cases
// ...
#endregion

#region Failure Cases
// ...
#endregion

#region Cancellation Token Tests
// ...
#endregion

#region Helper Methods
private void SetupSuccess(...) { ... }
#endregion
```

## Handler Test Class Structure

```csharp
public class CreateContentTypeHandlerTests
{
    private readonly Mock<ILookupRepository> _lookupRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly CreateContentTypeHandler _handler;

    public CreateContentTypeHandlerTests()
    {
        _lookupRepositoryMock = MockLookupRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        _mapper = new Mapper(config);

        _handler = new CreateContentTypeHandler(
            _lookupRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapper
        );
    }
    // ...
}
```

## Validator Test Class Structure

```csharp
public class CreateContentTypeValidatorTests
{
    private readonly CreateContentTypeValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new CreateContentTypeCommand(Name: "Article");
        var result = await _validator.TestValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new CreateContentTypeCommand(Name: string.Empty);
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Content type name is required.");
    }
}
```

## Entity Test Class Structure

```csharp
public class ContentTypeEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateContentType()
    {
        Guid id = Guid.NewGuid();
        string name = TestConstants.Content.ContentType.ValidName;

        ContentTypeEntity entity = ContentTypeEntity.Create(id, name);

        entity.Id.Should().Be(id);
        entity.Name.Should().Be(name);
        entity.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowBadRequestException(string? invalidName)
    {
        Action act = () => ContentTypeEntity.Create(Guid.NewGuid(), invalidName!);
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
```

## Mock UnitOfWork Pattern

```csharp
public static class MockContentUnitOfWork
{
    public static Mock<IContentUnitOfWork> Create()
    {
        var mock = new Mock<IContentUnitOfWork>();
        mock.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return mock;
    }

    public static void VerifyCommitCalled(this Mock<IContentUnitOfWork> mock)
        => mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);

    public static void VerifyCommitNotCalled(this Mock<IContentUnitOfWork> mock)
        => mock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
}
```

## Mock Repository Pattern

```csharp
public static class MockLookupRepository
{
    public static Mock<ILookupRepository> Create()
    {
        var mock = new Mock<ILookupRepository>();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ILookupRepository> SetupContentTypeExistsByName(
        this Mock<ILookupRepository> mock, string name, bool exists)
    {
        mock.Setup(x => x.ContentTypeExistsByNameAsync(name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetContentTypeByIdOrThrow(
        this Mock<ILookupRepository> mock, ContentTypeEntity entity)
    {
        mock.Setup(x => x.GetContentTypeByIdOrThrowAsync(entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        return mock;
    }

    public static Mock<ILookupRepository> SetupGetContentTypeByIdOrThrowNotFound(
        this Mock<ILookupRepository> mock, Guid id)
    {
        mock.Setup(x => x.GetContentTypeByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("ContentType", "id", keyValue: id));
        return mock;
    }

    // ... more setups

    private static void SetupDefaults(Mock<ILookupRepository> mock)
    {
        mock.Setup(x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddPricingTierAsync(It.IsAny<PricingTierEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddPromotionLevelAsync(It.IsAny<PromotionLevelEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.AddTagAsync(It.IsAny<TagEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
```

## BaseContentHandlerTest

```csharp
// tests/Unit/Common/BaseContentHandlerTest.cs
public abstract class BaseContentHandlerTest
{
    protected readonly IMapper Mapper;

    protected BaseContentHandlerTest()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        Mapper = new Mapper(config);
    }
}
```

## Builder Pattern

```csharp
// Internal — not public. Used only by factories.
internal class ContentTypeBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _name = TestConstants.Content.ContentType.ValidName;
    private bool _isActive = true;

    public ContentTypeBuilder WithId(Guid id) { _id = id; return this; }
    public ContentTypeBuilder WithName(string name) { _name = name; return this; }
    public ContentTypeBuilder AsInactive() { _isActive = false; return this; }

    public ContentTypeEntity Build()
    {
        var entity = ContentTypeEntity.Create(_id, _name);
        if (!_isActive) entity.Deactivate();
        return entity;
    }
}
```

## Factory Pattern

```csharp
// Public — used in test classes directly.
public static class ContentTypeFactory
{
    public static ContentTypeEntity Create() => new ContentTypeBuilder().Build();
    public static ContentTypeEntity Create(string name) => new ContentTypeBuilder().WithName(name).Build();
    public static ContentTypeEntity CreateWithId(Guid id) => new ContentTypeBuilder().WithId(id).Build();
    public static ContentTypeEntity CreateInactive() => new ContentTypeBuilder().AsInactive().Build();
    public static List<ContentTypeEntity> CreateMany(int count) =>
        Enumerable.Range(0, count).Select(_ => Create()).ToList();
}
```

## Verify Patterns

```csharp
// Verify Add was called
_lookupRepositoryMock.Verify(
    x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()),
    Times.Once);

// Verify Commit was called
_unitOfWorkMock.VerifyCommitCalled();

// Verify specific arg
_lookupRepositoryMock.Verify(
    x => x.ContentTypeExistsByNameAsync(command.Name, It.IsAny<CancellationToken>()),
    Times.Once);

// Verify NOT called
_lookupRepositoryMock.Verify(
    x => x.AddContentTypeAsync(It.IsAny<ContentTypeEntity>(), It.IsAny<CancellationToken>()),
    Times.Never);
```

## Cancellation Token Tests Pattern

```csharp
[Fact]
public async Task Handle_WithCancellationToken_ShouldPassToRepository()
{
    // Arrange
    using CancellationTokenSource cts = new();
    string name = "Article";
    var command = new CreateContentTypeCommand(Name: name);
    _lookupRepositoryMock.SetupContentTypeExistsByName(name, false);

    // Act
    await _handler.Handle(command, cts.Token);

    // Assert
    _lookupRepositoryMock.Verify(
        x => x.ContentTypeExistsByNameAsync(name, cts.Token), Times.Once);
}
```
