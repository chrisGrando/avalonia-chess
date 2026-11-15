using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess rook and encapsulates its orthogonal sliding movement and capture behavior.
/// </summary>
/// <param name="color">The team color of this rook.</param>
public class Rook(PieceColor color) : Piece(color)
{
    /// <summary>
    /// Orthogonal directional offsets (north, south, west, east) along which a rook moves.
    /// </summary>
    private static readonly (int dRow, int dCol)[] Directions = [(-1, 0), (1, 0), (0, -1), (0, 1)];

    /// <summary>
    /// Gets the image asset file name corresponding to this rook piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "rook_white.png" : "rook_black.png";

    /// <summary>
    /// Calculates all valid orthogonal moves and captures for this rook based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this rook.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this rook.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();
        AddRayMoves(currentPosition, board, Directions, moves);
        return moves;
    }
}
