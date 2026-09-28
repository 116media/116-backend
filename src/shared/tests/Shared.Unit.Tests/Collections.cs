using Xunit;

namespace _116.Shared.Unit.Tests;

/// <summary>
/// The test collections this suite joins. xunit resolves a collection definition only inside the assembly
/// that uses it, so every suite declares its own; the fixtures themselves live in the shared boot library.
/// </summary>
[CollectionDefinition("EnvironmentVariable", DisableParallelization = true)]
public sealed class EnvironmentVariableCollection;
