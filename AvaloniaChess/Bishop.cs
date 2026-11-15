using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess bishop and encapsulates its diagonal sliding movement and capture behavior.
/// </summary>
/// <param name="color">The team color of this bishop.</param>
public class Bishop(PieceColor color) : Piece(color)
{
    /// <summary>
    /// Diagonal directional offsets along which a bishop moves.
    /// </summary>
    private static readonly (int dRow, int dCol)[] Directions = [(-1, -1), (-1, 1), (1, -1), (1, 1)];

    /// <summary>
    /// Gets the image asset file name corresponding to this bishop piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "bishop_white.png" : "bishop_black.png";

    /// <summary>
    /// Calculates all valid diagonal moves and captures for this bishop based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this bishop.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this bishop.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();
        AddRayMoves(currentPosition, board, Directions, moves);
        return moves;
    }
}
