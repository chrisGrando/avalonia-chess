using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess knight and encapsulates its L-shaped jumping movement and capture behavior.
/// </summary>
/// <param name="color">The team color of this knight.</param>
public class Knight(PieceColor color) : Piece(color)
{
    /// <summary>
    /// Relative row and column offsets defining all eight possible knight leaps.
    /// </summary>
    private static readonly (int dRow, int dCol)[] Offsets = [
        (-2, -1), (-2, 1),
        (-1, -2), (-1, 2),
        (1, -2),  (1, 2),
        (2, -1),  (2, 1)
    ];

    /// <summary>
    /// Gets the image asset file name corresponding to this knight piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "knight_white.png" : "knight_black.png";

    /// <summary>
    /// Calculates all valid jumping moves and captures for this knight based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this knight.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this knight.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();

        foreach (var (dRow, dCol) in Offsets)
        {
            int r = currentPosition.Row + dRow;
            int c = currentPosition.Column + dCol;

            if (r is >= 0 and < 8 && c is >= 0 and < 8)
            {
                var targetPiece = board[r, c];
                if (targetPiece == null || targetPiece.Color != Color)
                {
                    moves.Add(new Position(r, c));
                }
            }
        }

        return moves;
    }
}
