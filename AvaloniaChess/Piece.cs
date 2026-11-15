using System;
using System.Collections.Generic;

namespace AvaloniaChess;

/// <summary>
/// Abstract base class representing a chess piece, defining its team color, movement tracking, and move calculation contracts.
/// </summary>
/// <param name="color">The team color of this piece.</param>
public abstract class Piece(PieceColor color)
{
    /// <summary>
    /// Gets the team color of the piece.
    /// </summary>
    public PieceColor Color { get; } = color;

    /// <summary>
    /// Gets or sets a value indicating whether this piece has moved from its initial starting position.
    /// </summary>
    public bool HasMoved { get; set; }

    /// <summary>
    /// Gets the image asset file name corresponding to this piece (e.g., "pawn_white.png").
    /// </summary>
    public abstract string ImageFileName { get; }

    /// <summary>
    /// Calculates all possible valid move destinations for this piece from the specified current board position.
    /// </summary>
    /// <param name="currentPosition">The current board position of this piece.</param>
    /// <param name="board">The current 8x8 chessboard state containing active pieces.</param>
    /// <returns>A collection of valid destination board positions.</returns>
    public abstract List<Position> GetPossibleMoves(Position currentPosition, Piece?[,] board);

    /// <summary>
    /// Helper method that projects continuous sliding ray moves along the specified directional vectors until blocked.
    /// </summary>
    /// <param name="currentPosition">The starting board position.</param>
    /// <param name="board">The current 8x8 chessboard state.</param>
    /// <param name="directions">The collection of directional row and column offsets.</param>
    /// <param name="moves">The destination list to which valid positions will be added.</param>
    protected void AddRayMoves(Position currentPosition, Piece?[,] board, ReadOnlySpan<(int dRow, int dCol)> directions, List<Position> moves)
    {
        foreach (var (dRow, dCol) in directions)
        {
            int r = currentPosition.Row + dRow;
            int c = currentPosition.Column + dCol;

            while (r is >= 0 and < 8 && c is >= 0 and < 8)
            {
                var targetPiece = board[r, c];
                if (targetPiece == null)
                {
                    moves.Add(new Position(r, c));
                }
                else
                {
                    if (targetPiece.Color != Color)
                    {
                        moves.Add(new Position(r, c));
                    }
                    break;
                }

                r += dRow;
                c += dCol;
            }
        }
    }
}
