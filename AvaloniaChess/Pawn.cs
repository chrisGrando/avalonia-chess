using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Represents a chess pawn and encapsulates its movement, double-step forward, and diagonal capture logic.
/// </summary>
/// <param name="color">The team color of this pawn.</param>
public class Pawn(PieceColor color) : Piece(color)
{
    /// <summary>
    /// Gets the image asset file name corresponding to this pawn piece.
    /// </summary>
    public override string ImageFileName => Color == PieceColor.White ? "pawn_white.png" : "pawn_black.png";

    /// <summary>
    /// Calculates all valid moves and captures for this pawn piece based on the current board state.
    /// </summary>
    /// <param name="currentPosition">The current board position of this pawn.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A list of valid destination board positions for this pawn.</returns>
    public override List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board)
    {
        var moves = new List<Position>();
        int forwardDirection = Color == PieceColor.White ? -1 : 1;
        int startRow = Color == PieceColor.White ? 6 : 1;

        // Single step forward
        int oneStepRow = currentPosition.Row + forwardDirection;
        int col = currentPosition.Column;

        if (oneStepRow is >= 0 and < 8 && board[oneStepRow, col] == null)
        {
            moves.Add(new Position(oneStepRow, col));

            // Double step forward from starting rank
            int twoStepRow = currentPosition.Row + (2 * forwardDirection);
            if (!HasMoved && currentPosition.Row == startRow && twoStepRow is >= 0 and < 8 && board[twoStepRow, col] == null)
            {
                moves.Add(new Position(twoStepRow, col));
            }
        }

        // Diagonal captures
        int[] captureCols = [col - 1, col + 1];
        foreach (int capCol in captureCols)
        {
            if (oneStepRow is >= 0 and < 8 && capCol is >= 0 and < 8)
            {
                var targetPiece = board[oneStepRow, capCol];
                if (targetPiece != null && targetPiece.Color != Color)
                {
                    moves.Add(new Position(oneStepRow, capCol));
                }
            }
        }

        return moves;
    }
}
