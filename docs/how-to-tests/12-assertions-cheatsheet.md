# Assertions Cheat Sheet

All assertions use **AwesomeAssertions** (a FluentAssertions-compatible fork). The API is identical to FluentAssertions.

---

## Basic Value Assertions

```csharp
// Equality
result.Should().Be(expected);
result.Should().NotBe(unexpected);

// Boolean
result.Should().BeTrue();
result.Should().BeFalse();

// Null
result.Should().BeNull();
result.Should().NotBeNull();

// String
result.Should().Be("exact string");
result.Should().Contain("substring");
result.Should().StartWith("prefix");
result.Should().BeEmpty();
result.Should().NotBeEmpty();

// Numeric
result.Should().Be(42);
result.Should().BeGreaterThan(0);
result.Should().BeGreaterThanOrEqualTo(1);
result.Should().BeLessThan(100);
result.Should().BePositive();

// Guid
result.Should().Be(expectedGuid);
result.Should().NotBeEmpty();    // Not Guid.Empty

// DateTimeOffset / DateTime
result.Should().NotBeNull();
result.Should().BeOnOrAfter(before);
result.Should().Be(expectedDateTime);
```

---

## Collection Assertions

```csharp
list.Should().HaveCount(3);
list.Should().BeEmpty();
list.Should().NotBeEmpty();
list.Should().Contain(item);
list.Should().NotContain(item);
list.Should().ContainSingle();                        // Exactly 1 item
list.Should().HaveCountGreaterThan(0);
list.Should().AllSatisfy(x => x.IsActive.Should().BeTrue());
```

---

## Object / DTO Assertions

```csharp
result.Should().NotBeNull();
result.Should().BeEquivalentTo(expectedDto);          // Deep equality, all properties
result.Id.Should().Be(entity.Id);                    // Individual property check
result.Name.Should().Be(entity.Name);
```

---

## Exception Assertions — Synchronous

```csharp
// Simple throw
Action act = () => entity.Submit();
act.Should().Throw<ConflictException>();

// Throw with message check
act.Should().Throw<BadRequestException>()
    .WithMessage("*partial message*");   // Wildcards supported

// No throw
Action act = () => entity.Activate();
act.Should().NotThrow();
```

---

## Exception Assertions — Asynchronous (ALWAYS use for handlers and factories)

```csharp
// Async throw
Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);
await act.Should().ThrowAsync<NotFoundException>();

// Async throw with message check
await act.Should().ThrowAsync<BadRequestException>()
    .WithMessage("*expected message part*");

// Async no throw
await act.Should().NotThrowAsync();
```

**Rule:** Never use `Action act = () => ...` for async code. Always use `Func<Task> act = async () => ...`.

---

## ValidationResult Assertions (Validator Tests)

```csharp
// Happy path
result.IsValid.Should().BeTrue();
result.Errors.Should().BeEmpty();

// Error path — check property name only
result.IsValid.Should().BeFalse();
result.Errors.Should().Contain(e => e.PropertyName == nameof(Command.FieldName));

// Error path — check property name AND message
result.Errors.Should().Contain(e =>
    e.PropertyName == nameof(Command.FieldName) &&
    e.ErrorMessage == "Expected error message");

// Error count
result.Errors.Should().HaveCount(1);
```

---

## Moq — Setting Up Returns

```csharp
// Return a value
mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync(entity);

// Return null
mock.Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
    .ReturnsAsync((EntityType?)null);

// Return Task.CompletedTask (void async)
mock.Setup(x => x.UpdateAsync(It.IsAny<EntityType>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

// Throw
mock.Setup(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()))
    .ThrowsAsync(new NotFoundException("Not found"));
```

---

## Moq — Argument Matching

```csharp
// Match any value of type T
It.IsAny<Guid>()
It.IsAny<CancellationToken>()
It.IsAny<string>()

// Match a specific value
It.Is<Guid>(id => id == specificId)
It.Is<string>(s => s.StartsWith("prefix"))
It.Is<EntityType>(e => e.Status == OrderStatus.Draft)

// Match exact value (only use when value is deterministic)
specificId     // Direct value matching
```

---

## Moq — Verifying Calls

```csharp
// Called exactly once
mock.Verify(x => x.UpdateAsync(It.IsAny<EntityType>(), It.IsAny<CancellationToken>()), Times.Once);

// Never called
mock.Verify(x => x.UpdateAsync(It.IsAny<EntityType>(), It.IsAny<CancellationToken>()), Times.Never);

// Called exactly N times
mock.Verify(x => x.GetByIdOrThrowAsync(id, It.IsAny<CancellationToken>()), Times.Exactly(2));

// Called at least once
mock.Verify(x => x.AddAsync(It.IsAny<EntityType>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

// Called with specific argument
mock.Verify(x => x.AddAsync(
    It.Is<EntityType>(e => e.Id == expectedId),
    It.IsAny<CancellationToken>()),
    Times.Once);
```

---

## Times Reference

| Value | Meaning |
|-------|---------|
| `Times.Once` | Called exactly 1 time |
| `Times.Never` | Called 0 times |
| `Times.Exactly(n)` | Called exactly n times |
| `Times.AtLeastOnce` | Called 1 or more times |
| `Times.AtMost(n)` | Called at most n times |

---

## Mock Wrapper Methods vs Direct Verify

Always prefer the mock helper's `Verify*` wrapper over writing `.Verify(...)` directly:

```csharp
// Preferred — readable, uses the mock helper
_unitOfWorkMock.VerifyCommitCalled();
_articleRepositoryMock.VerifyUpdateCalled();
_orderRepositoryMock.VerifyAddItemTierCalled();

// Only use direct Verify when the mock helper doesn't cover the case
_fileRepositoryMock.Verify(
    x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
    Times.Never
);
```

---

## Common Pitfalls

| Pitfall | Fix |
|---------|-----|
| Using `Action` for async code | Use `Func<Task>` |
| Hardcoded string property names in validator assertions | Use `nameof(Command.Field)` |
| `It.IsAny<T>()` for a value you care about | Use `It.Is<T>(x => x == expected)` |
| Not calling `await` on `act.Should().ThrowAsync<T>()` | Always `await` the assertion |
| Using `Times.Once()` with parentheses (it's a property) | Use `Times.Once` (no parentheses) |
| Forgetting to assert both `IsValid` and `Errors` | Always assert both on failure path |
