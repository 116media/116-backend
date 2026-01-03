using _116.Identity.Application.Session.Specifications;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Shared.Application.Specifications;

namespace _116.Identity.Application.Session.Builders;

/// <summary>
/// Builder for constructing dynamic session queries using specifications.
/// Implements the Builder pattern to eliminate conditional logic in query construction.
/// </summary>
public class SessionQueryBuilder : ISessionQueryBuilder
{
    private Specification<SessionEntity>? _specification;

    /// <inheritdoc />
    public ISessionQueryBuilder WithUserId(Guid userId)
    {
        var userSpec = new SessionByUserIdSpecification(userId: userId);
        CombineSpecification(spec: userSpec);
        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(value: status))
        {
            return this;
        }

        string normalizedStatus = status.ToLower();
        Specification<SessionEntity> statusSpec = normalizedStatus switch
        {
            _ when normalizedStatus.Equals(nameof(EnumSessionStatus.Active),
                    comparisonType: StringComparison.InvariantCultureIgnoreCase) =>
                new SessionIsActiveSpecification(),
            _ when normalizedStatus.Equals(nameof(EnumSessionStatus.Expired),
                    comparisonType: StringComparison.InvariantCultureIgnoreCase) =>
                new SessionIsExpiredSpecification(),
            _ => null!
        };

        CombineSpecification(spec: statusSpec);

        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(value: ipAddress))
        {
            return this;
        }

        var ipSpec = new SessionByIpAddressSpecification(ipAddress: ipAddress);
        CombineSpecification(spec: ipSpec);
        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithDeviceName(string? deviceName)
    {
        if (string.IsNullOrWhiteSpace(value: deviceName))
        {
            return this;
        }

        var deviceSpec = new SessionByDeviceNameSpecification(deviceName: deviceName);
        CombineSpecification(spec: deviceSpec);
        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithFromDate(DateTime? fromDate)
    {
        if (!fromDate.HasValue)
        {
            return this;
        }

        var dateSpec = new SessionCreatedAfterSpecification(fromDate: fromDate.Value);
        CombineSpecification(spec: dateSpec);
        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithToDate(DateTime? toDate)
    {
        if (!toDate.HasValue)
        {
            return this;
        }

        var dateSpec = new SessionCreatedBeforeSpecification(toDate: toDate.Value);
        CombineSpecification(spec: dateSpec);
        return this;
    }

    /// <inheritdoc />
    public ISessionQueryBuilder WithActiveStatus(bool? isActive)
    {
        if (!isActive.HasValue)
        {
            return this;
        }

        Specification<SessionEntity> activeSpec = isActive.Value
            ? new SessionIsActiveSpecification()
            : new SessionIsExpiredSpecification();

        CombineSpecification(spec: activeSpec);
        return this;
    }

    /// <inheritdoc />
    public Specification<SessionEntity>? Build()
    {
        return _specification;
    }

    private void CombineSpecification(Specification<SessionEntity> spec)
    {
        _specification = _specification is null
            ? spec
            : _specification.And(other: spec);
    }
}
