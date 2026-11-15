using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess queen and encapsulates its omnidirectional sliding movement and capture behavior.
/// </summary>
/// <param name="color">The team color of this queen.</param>
public class Queen(PieceColor color) : Piece(color)
{
    /// <summary>
    /// All eight orthogonal and diagonal directional offsets along which a queen moves.
    /// </summary>
    private static readonly (int dRow, int dCol)[] Directions = [
        (-1, 0), (1, 0), (0, -1), (0, 1),
        (-1, -1), (-1, 1), (1, -1), (1, 1)
    ];

    /// <summary>
    /// Gets the image asset file name corresponding to this queen piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "queen_white.png" : "queen_black.png";

    /// <summary>
    /// Calculates all valid omnidirectional moves and captures for this queen based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this queen.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this queen.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();
        AddRayMoves(currentPosition, board, Directions, moves);
        return moves;
    }
}
