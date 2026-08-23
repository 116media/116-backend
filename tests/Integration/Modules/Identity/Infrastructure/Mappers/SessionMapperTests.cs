using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;
using MapsterMapper;
using IdentityMappingRegistration = _116.Identity.Application.Shared.Mappers.MappingRegistration;

namespace _116.Integration.Tests.Modules.Identity.Mappers;

/// <summary>
/// Integration tests for <see cref="SessionMapper" />.
/// Verifies entity-to-DTO mapping with real PostgreSQL entities.
/// </summary>
[Collection("Database")]
public class SessionMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    private readonly IMapper _mapper = new Mapper(IdentityMappingRegistration.CreateConfiguration());

    [Fact]
    public async Task ToSessionDto_ShouldMapAllFields()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        var session = SessionFactory.Create(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        SessionEntity loaded = await readContext.Sessions.FirstAsync(s => s.Id == session.Id);

        SessionDto dto = loaded.ToSessionDto(_mapper);

        dto.Id.Should().Be(loaded.Id);
        dto.IpAddress.Should().Be(loaded.IpAddress);
        dto.UserAgent.Should().Be(loaded.UserAgent);
        dto.Browser.Should().Be(loaded.Browser);
        dto.Device.Should().Be(loaded.Device);
        dto.Platform.Should().Be(loaded.Platform);
        dto.Client.Should().Be(loaded.Client);
        dto.ExpiresAt.Should().Be(loaded.ExpiresAt);
        dto.IsActive.Should().BeTrue();
        dto.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task ToSessionDto_WithCurrentSessionId_ShouldSetIsCurrent()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        var session = SessionFactory.Create(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        SessionEntity loaded = await readContext.Sessions.FirstAsync(s => s.Id == session.Id);

        SessionDto dto = loaded.ToSessionDto(_mapper, loaded.Id);

        dto.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task ToSessionDto_ExpiredSession_ShouldMapIsActiveFalse()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        var session = SessionFactory.CreateExpired(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        SessionEntity loaded = await readContext.Sessions.FirstAsync(s => s.Id == session.Id);

        SessionDto dto = loaded.ToSessionDto(_mapper);

        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToSessionExportDtos_ShouldMapCollection()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.Create();
        seedContext.Users.Add(user);
        var session1 = SessionFactory.Create(user.Id);
        var session2 = SessionFactory.Create(user.Id);
        seedContext.Sessions.AddRange(session1, session2);
        await seedContext.SaveChangesAsync();

        await using var readContext = CreateDbContext<IdentityDbContext>();
        List<SessionEntity> loaded = await readContext.Sessions.Where(s => s.UserId == user.Id).ToListAsync();

        List<SessionExportDto> dtos = loaded.ToSessionExportDtos(_mapper);

        dtos.Should().HaveCount(2);
        dtos.Select(d => d.Id).Should().BeEquivalentTo([session1.Id, session2.Id]);
    }
}
