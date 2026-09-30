namespace ROMulus.Core.Models;

/// <summary>
/// Confidence level of the ROM identification pipeline.
/// Mirrors the Python <c>match_confidence</c> string enum.
/// </summary>
public enum MatchConfidence
{
    /// <summary>No match found at any layer.</summary>
    Unmatched,
    /// <summary>Matched by fuzzy filename comparison (L1).</summary>
    Fuzzy,
    /// <summary>Matched by internal ROM header extraction (L2).</summary>
    Header,
    /// <summary>Matched by SHA-1 / CRC32 against a No-Intro DAT (L3).</summary>
    DatVerified,
}
