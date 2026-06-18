# Writing Validator Tests

Validator tests verify that a FluentValidation validator accepts valid inputs and rejects invalid ones with the correct property name and error message.

---

## Class Setup

No base class, no mocks. Instantiate the validator directly.

```csharp
public class AdminSubmitArticleValidatorTests
{
    private readonly AdminSubmitArticleValidator _validator = new();
}
```

---

## Happy Path Test

```csharp
[Fact]
public async Task Validate_WithValidData_ShouldNotHaveErrors()
{
    // Arrange — construct a fully valid command
    var command = new AdminSubmitArticleCommand(Id: Guid.NewGuid().ToString());

    // Act — always async for FluentValidation
    ValidationResult result = await _validator.ValidateAsync(command);

    // Assert
    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
}
```

---

## Error Path Tests

### Empty / whitespace field

```csharp
[Fact]
public async Task Validate_WithEmptyId_ShouldHaveError()
{
    var command = new AdminSubmitArticleCommand(Id: "");

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == nameof(AdminSubmitArticleCommand.Id) &&
        e.ErrorMessage == "...");    // Match the actual error message in the validator
}
```

### Invalid GUID

```csharp
[Fact]
public async Task Validate_WithInvalidGuidId_ShouldHaveError()
{
    var command = new AdminSubmitArticleCommand(Id: "not-a-guid");

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == nameof(AdminSubmitArticleCommand.Id));
}
```

### Multiple GUID fields

```csharp
[Fact]
public async Task Validate_WithValidData_ShouldNotHaveErrors()
{
    var command = new AdminAddItemTierCommand(
        OrderId: Guid.NewGuid().ToString(),
        OrderItemId: Guid.NewGuid().ToString(),
        PricingTierId: Guid.NewGuid().ToString()
    );

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeTrue();
    result.Errors.Should().BeEmpty();
}

[Fact]
public async Task Validate_WithInvalidGuidOrderId_ShouldHaveError()
{
    var command = new AdminAddItemTierCommand(
        OrderId: "not-a-guid",
        OrderItemId: Guid.NewGuid().ToString(),
        PricingTierId: Guid.NewGuid().ToString()
    );

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == nameof(AdminAddItemTierCommand.OrderId));
}

[Fact]
public async Task Validate_WithInvalidGuidOrderItemId_ShouldHaveError()
{
    var command = new AdminAddItemTierCommand(
        OrderId: Guid.NewGuid().ToString(),
        OrderItemId: "not-a-guid",
        PricingTierId: Guid.NewGuid().ToString()
    );

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == nameof(AdminAddItemTierCommand.OrderItemId));
}
```

### Exceeding max length (use TestConstants)

```csharp
[Fact]
public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
{
    var command = new AdminCreateRoleCommand(
        Name: new string('x', TestConstants.Role.NameMaxLength + 1),
        Description: TestConstants.Role.ValidDescription
    );

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeFalse();
    result.Errors.Should().Contain(e =>
        e.PropertyName == nameof(AdminCreateRoleCommand.Name));
}
```

### Null optional fields

```csharp
[Fact]
public async Task Validate_WithNullName_ShouldBeValid()
{
    // Optional field — null is acceptable
    var command = new AdminUpdateRoleCommand(
        Id: Guid.NewGuid().ToString(),
        Name: null,
        Description: null
    );

    ValidationResult result = await _validator.ValidateAsync(command);

    result.IsValid.Should().BeTrue();
}
```

---

## Key Rules

1. **Always `await _validator.ValidateAsync(...)`** — never call `Validate()` synchronously
2. **Use `nameof(Command.Property)`** for property name assertions — never hardcode strings
3. **Assert both `IsValid` and `Errors`** on the failure case
4. **Use `TestConstants.*MaxLength + 1`** for exceeding max length tests
5. **Write one test per validation rule** — do not combine multiple failures in one test

---

## Assertion Reference

```csharp
// Happy path
result.IsValid.Should().BeTrue();
result.Errors.Should().BeEmpty();

// Error path — just check property name
result.IsValid.Should().BeFalse();
result.Errors.Should().Contain(e => e.PropertyName == nameof(Cmd.Field));

// Error path — check property name AND error message
result.Errors.Should().Contain(e =>
    e.PropertyName == nameof(Cmd.Field) &&
    e.ErrorMessage == "Expected error message text");

// Multiple errors at once (when testing one command with multiple violations)
result.Errors.Should().HaveCount(2);
```

---

## Real Test Files to Reference

| File | Key Pattern |
|------|-------------|
| `tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/SubmitArticle/AdminSubmitArticleValidatorTests.cs` | Single GUID field, empty + invalid GUID |
| `tests/Unit/Modules/Content/Application/Commerce/UseCases/Admin/Commands/AddItemTier/AdminAddItemTierValidatorTests.cs` | Three GUID fields, each tested individually |
