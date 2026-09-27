using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.ValueObjects;

/// <summary>
/// A non-negative USD amount. Every price, snapshot and total in the Commerce and Catalogue
/// slices is one of these, so "a price cannot be negative" is stated once.
/// </summary>
public record Money
{
    /// <summary>
    /// Initializes a new <see cref="Money" />, rejecting a negative amount.
    /// </summary>
    /// <param name="amount">The USD amount.</param>
    /// <exception cref="ContentRuleException">Thrown when the amount is negative.</exception>
    public Money(decimal amount)
    {
        if (amount < 0m)
        {
            throw new ContentRuleException(ContentRuleCodes.NegativeMoneyAmount, amount.ToString("0.##"));
        }

        Amount = amount;
    }

    /// <summary>
    /// The validated USD amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// The zero amount, used as the seed of a sum and as a free item's price.
    /// </summary>
    public static Money Zero { get; } = new(amount: 0m);

    /// <summary>
    /// Parses a candidate that may be negative or absent, returning null instead of throwing.
    /// </summary>
    /// <param name="amount">The candidate amount.</param>
    /// <returns>The parsed amount, or null when the candidate is absent or negative.</returns>
    public static Money? TryFrom(decimal? amount)
    {
        return amount is not decimal value || value < 0m ? null : new Money(value);
    }

    /// <summary>
    /// Adds the amounts in a sequence.
    /// </summary>
    /// <param name="amounts">The amounts to add.</param>
    /// <returns>Their total, or <see cref="Zero" /> when the sequence is empty.</returns>
    public static Money Sum(IEnumerable<Money> amounts)
    {
        return amounts.Aggregate(Zero, (total, next) => total + next);
    }

    /// <summary>
    /// Returns the amount formatted invariantly, without a currency symbol.
    /// </summary>
    /// <returns>The amount as a string.</returns>
    public override string ToString()
    {
        return Amount.ToString("0.##");
    }

    /// <summary>
    /// Adds two amounts.
    /// </summary>
    /// <param name="left">The first amount.</param>
    /// <param name="right">The second amount.</param>
    public static Money operator +(Money left, Money right)
    {
        return new Money(amount: left.Amount + right.Amount);
    }

    /// <summary>
    /// Multiplies an amount by a quantity.
    /// </summary>
    /// <param name="amount">The unit amount.</param>
    /// <param name="quantity">The quantity, which must not be negative.</param>
    public static Money operator *(Money amount, int quantity)
    {
        return new Money(amount: amount.Amount * quantity);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Money" /> to a <see cref="decimal" />.
    /// </summary>
    /// <param name="money">The amount.</param>
    public static implicit operator decimal(Money money)
    {
        return money.Amount;
    }

    /// <summary>
    /// Implicitly converts a <see cref="decimal" /> to a <see cref="Money" />.
    /// </summary>
    /// <param name="amount">The amount to convert.</param>
    /// <exception cref="ContentRuleException">Thrown when the amount is negative.</exception>
    public static implicit operator Money(decimal amount)
    {
        return new Money(amount: amount);
    }
}
