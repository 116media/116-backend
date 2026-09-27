using _116.Shared.Domain.Exceptions;

namespace _116.Storage.Domain.Exceptions;

/// <summary>
/// A <see cref="DomainRuleException" /> raised by the Storage domain, carrying a
/// <see cref="StateMachines.StorageRuleCodes" /> code. The module's strategy translates it.
/// </summary>
public class StorageRuleException(string code, params string[] args) : DomainRuleException(code, args);
