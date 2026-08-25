using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using Bogus;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="PackageEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; PackageFactory only names chains three or more tests share.
/// </summary>
public class PackageBuilder
{
    private readonly Faker _faker = TestFaker.Create();

    private Guid _id;
    private string _name;
    private string _description = "Default package description";
    private bool _isActive = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="PackageBuilder"/> class with random default values.
    /// </summary>
    public PackageBuilder()
    {
        _id = Guid.NewGuid();
        string word = _faker.Lorem.Word();
        _name = word[..Math.Min(TestConstants.Package.NameMaxLength, word.Length)];
    }

    /// <summary>
    /// Sets the package name.
    /// </summary>
    public PackageBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Marks the package as inactive.
    /// </summary>
    public PackageBuilder AsInactive()
    {
        _isActive = false;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="PackageEntity"/> instance.
    /// </summary>
    public PackageEntity Build()
    {
        var entity = PackageEntity.Create(_id, _name, _description, TestErrorsFactory.CreatePackageErrors());

        if (!_isActive)
        {
            entity.Deactivate();
        }

        return entity;
    }
}
