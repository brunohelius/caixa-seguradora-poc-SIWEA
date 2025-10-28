namespace CaixaSeguradora.Core.ValueObjects;

/// <summary>
/// Immutable value object representing claim composite primary key
/// Validation Rules:
/// - Tipseg >= 0
/// - Orgsin ∈ [1, 99]
/// - Rmosin ∈ [0, 99]
/// - Numsin ∈ [1, 999999]
/// </summary>
public class ClaimKey : IEquatable<ClaimKey>
{
    /// <summary>
    /// Insurance type
    /// </summary>
    public int Tipseg { get; }

    /// <summary>
    /// Origin code (2 digits, 01-99)
    /// </summary>
    public int Orgsin { get; }

    /// <summary>
    /// Branch code (2 digits, 00-99)
    /// </summary>
    public int Rmosin { get; }

    /// <summary>
    /// Claim number (1-6 digits)
    /// </summary>
    public int Numsin { get; }

    /// <summary>
    /// Constructor with validation
    /// </summary>
    public ClaimKey(int tipseg, int orgsin, int rmosin, int numsin)
    {
        if (tipseg < 0)
            throw new ArgumentException("Tipseg must be >= 0", nameof(tipseg));

        if (orgsin < 1 || orgsin > 99)
            throw new ArgumentException("Orgsin must be between 1 and 99", nameof(orgsin));

        if (rmosin < 0 || rmosin > 99)
            throw new ArgumentException("Rmosin must be between 0 and 99", nameof(rmosin));

        if (numsin < 1 || numsin > 999999)
            throw new ArgumentException("Numsin must be between 1 and 999999", nameof(numsin));

        Tipseg = tipseg;
        Orgsin = orgsin;
        Rmosin = rmosin;
        Numsin = numsin;
    }

    /// <summary>
    /// Returns formatted "ORGSIN/RMOSIN/NUMSIN" (e.g., "01/05/123456")
    /// </summary>
    public override string ToString()
    {
        return $"{Orgsin:D2}/{Rmosin:D2}/{Numsin}";
    }

    /// <summary>
    /// Value-based equality (all 4 fields match)
    /// </summary>
    public bool Equals(ClaimKey? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Tipseg == other.Tipseg
            && Orgsin == other.Orgsin
            && Rmosin == other.Rmosin
            && Numsin == other.Numsin;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ClaimKey);
    }

    /// <summary>
    /// Hash based on all 4 fields
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Tipseg, Orgsin, Rmosin, Numsin);
    }

    public static bool operator ==(ClaimKey? left, ClaimKey? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ClaimKey? left, ClaimKey? right)
    {
        return !Equals(left, right);
    }
}
