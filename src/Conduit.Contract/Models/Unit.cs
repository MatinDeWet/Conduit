namespace Conduit.Contract.Models;

/// <summary>
/// Represents a unit type, commonly used to signify the absence of a meaningful value.
/// </summary>
public record Unit
{
    /// <summary>
    /// Private constructor to prevent external instantiation.
    /// </summary>
    private Unit() { }

    /// <summary>
    /// Gets the singleton instance of the <see cref="Unit"/> type.
    /// </summary>
    public static Unit Value { get; } = new();
}
