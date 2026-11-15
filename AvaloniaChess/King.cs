using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess king and encapsulates its single-square movement, captures, and castling rules.
/// </summary>
/// <param name="color">The team color of this king.</param>
public class King(PieceColor color) : Piece(color)
{
    /// <summary>
    /// Relative row and column offsets representing the eight squares adjacent to the king.
    /// </summary>
    private static readonly (int dRow, int dCol)[] AdjacentOffsets = [
        (-1, -1), (-1, 0), (-1, 1),
        (0, -1),           (0, 1),
        (1, -1),  (1, 0),  (1, 1)
    ];

    /// <summary>
    /// Gets the image asset file name corresponding to this king piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "king_white.png" : "king_black.png";

    /// <summary>
    /// Calculates all valid moves, captures, and castling options for this king based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this king.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this king.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();

        foreach (var (dRow, dCol) in AdjacentOffsets)
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

        // Castling options when the king has not moved
        if (!HasMoved)
        {
            int row = currentPosition.Row;

            // Kingside castling (King moves to column 6, Rook is at column 7)
            if (board[row, 5] == null && board[row, 6] == null &&
                board[row, 7] is Rook kingsideRook && !kingsideRook.HasMoved && kingsideRook.Color == Color)
            {
                moves.Add(new Position(row, 6));
            }

            // Queenside castling (King moves to column 2, Rook is at column 0)
            if (board[row, 1] == null && board[row, 2] == null && board[row, 3] == null &&
                board[row, 0] is Rook queensideRook && !queensideRook.HasMoved && queensideRook.Color == Color)
            {
                moves.Add(new Position(row, 2));
            }
        }

        return moves;
    }
}
