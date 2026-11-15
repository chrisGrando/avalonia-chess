namespace AvaloniaChess;

/// <summary>
/// Represents an immutable 2D board position with zero-based row and column coordinates.
/// </summary>
/// <param name="Row">The zero-based row index (0 to 7).</param>
/// <param name="Column">The zero-based column index (0 to 7).</param>
public readonly record struct Position(int Row, int Column)
{
    /// <summary>
    /// Gets a value indicating whether the coordinates are within valid 8x8 chessboard boundaries.
    /// </summary>
    public bool IsValid => Row is >= 0 and < 8 && Column is >= 0 and < 8;

    /// <summary>
    /// Converts this 2D position into a zero-based 1D linear index (0 to 63).
    /// </summary>
    /// <returns>A linear index value representing this square on the board.</returns>
    public int ToIndex() => (Row * 8) + Column;

    /// <summary>
    /// Constructs a <see cref="Position"/> instance from a zero-based 1D linear index (0 to 63).
    /// </summary>
    /// <param name="index">The zero-based linear board index.</param>
    /// <returns>The corresponding <see cref="Position"/> struct.</returns>
    public static Position FromIndex(int index) => new(index / 8, index % 8);
}
