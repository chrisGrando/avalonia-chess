using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace AvaloniaChess;

/// <summary>
/// Controls the core game logic, board state, mouse interactions, and UI synchronization for the 2-player chess game.
/// </summary>
/// <param name="mainWindow">Optional reference to the main window hosting the chess board UI.</param>
public class AppLogic(MainWindow? mainWindow = null)
{
    /// <summary>
    /// Backing reference to the main application window hosting the chess board UI.
    /// </summary>
    private readonly MainWindow? _mainWindow = mainWindow;

    /// <summary>
    /// In-memory cache for loaded chess piece bitmap assets to prevent redundant I/O and memory allocations.
    /// </summary>
    private static readonly Dictionary<string, Bitmap> _bitmapCache = [];

    /// <summary>
    /// Brush used to highlight valid move destination squares in green.
    /// </summary>
    private static readonly IBrush _highlightBrush = new SolidColorBrush(Color.FromRgb(118, 205, 118));

    /// <summary>
    /// Internal 8x8 chessboard representation storing active pieces or null for empty squares.
    /// </summary>
    private Piece?[,] _board = new Piece?[8, 8];

    /// <summary>
    /// Indicates which team's turn it currently is (White starts first).
    /// </summary>
    private PieceColor _currentTurn = PieceColor.White;

    /// <summary>
    /// Currently selected piece board position, or null if no piece is currently selected.
    /// </summary>
    private Position? _selectedPosition;

    /// <summary>
    /// List of valid destination positions for the currently selected piece.
    /// </summary>
    private List<Position> _highlightedMoves = [];

    /// <summary>
    /// References to all 64 panel elements representing chessboard squares in linear index order.
    /// </summary>
    private readonly List<Panel> _panels = [];

    /// <summary>
    /// Stores the original background brushes of each square to restore them after highlights.
    /// </summary>
    private readonly IBrush?[] _originalTileBrushes = new IBrush?[64];

    /// <summary>
    /// Reference to the left-side text block indicating White's turn.
    /// </summary>
    private TextBlock? _whiteTurnTextBlock;

    /// <summary>
    /// Reference to the right-side text block indicating Black's turn.
    /// </summary>
    private TextBlock? _blackTurnTextBlock;

    /// <summary>
    /// Reference to the left-side "PLAY AGAIN" button displayed below the winning text when White wins.
    /// </summary>
    private Button? _whitePlayAgainButton;

    /// <summary>
    /// Reference to the right-side "PLAY AGAIN" button displayed below the winning text when Black wins.
    /// </summary>
    private Button? _blackPlayAgainButton;

    /// <summary>
    /// Indicates whether the match has completed (e.g., via king capture).
    /// </summary>
    private bool _isGameOver;

    /// <summary>
    /// Gets the color of the player whose turn it currently is.
    /// </summary>
    public PieceColor CurrentTurn => _currentTurn;

    /// <summary>
    /// Gets a value indicating whether the current game has completed.
    /// </summary>
    public bool IsGameOver => _isGameOver;

    /// <summary>
    /// Loads the required chess piece image assets, initializes the board state, attaches mouse click handlers,
    /// and positions each piece in its starting location on the grid.
    /// </summary>
    /// <param name="window">
    /// Optional window containing the chess board. If not provided, falls back to the instance's
    /// main window or the active desktop lifetime main window.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the target window or the 8x8 chess board grid cannot be resolved, or if the board has fewer than 64 panels.
    /// </exception>
    public void LoadChessPiecesPictures(MainWindow? window = null)
    {
        var targetWindow = window ?? _mainWindow;
        if (targetWindow == null && Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            targetWindow = desktop.MainWindow as MainWindow;
        }

        if (targetWindow == null)
        {
            throw new InvalidOperationException("MainWindow is not available.");
        }

        var boardGrid = targetWindow.FindControl<UniformGrid>("ChessBoardGrid")
            ?? targetWindow.GetVisualDescendants()
                           .OfType<UniformGrid>()
                           .FirstOrDefault(g => g.Rows == 8 && g.Columns == 8);

        if (boardGrid == null)
        {
            throw new InvalidOperationException("Chess board grid was not found in MainWindow.");
        }

        _whiteTurnTextBlock = targetWindow.FindControl<TextBlock>("WhiteTurnText")
            ?? targetWindow.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Name == "WhiteTurnText");

        _blackTurnTextBlock = targetWindow.FindControl<TextBlock>("BlackTurnText")
            ?? targetWindow.GetVisualDescendants().OfType<TextBlock>().FirstOrDefault(t => t.Name == "BlackTurnText");

        _whitePlayAgainButton = targetWindow.FindControl<Button>("WhitePlayAgainButton")
            ?? targetWindow.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "WhitePlayAgainButton");

        _blackPlayAgainButton = targetWindow.FindControl<Button>("BlackPlayAgainButton")
            ?? targetWindow.GetVisualDescendants().OfType<Button>().FirstOrDefault(b => b.Name == "BlackPlayAgainButton");

        if (_whitePlayAgainButton != null)
        {
            _whitePlayAgainButton.Click -= OnPlayAgainClicked;
            _whitePlayAgainButton.Click += OnPlayAgainClicked;
            _whitePlayAgainButton.IsVisible = false;
        }

        if (_blackPlayAgainButton != null)
        {
            _blackPlayAgainButton.Click -= OnPlayAgainClicked;
            _blackPlayAgainButton.Click += OnPlayAgainClicked;
            _blackPlayAgainButton.IsVisible = false;
        }

        _panels.Clear();
        _panels.AddRange(boardGrid.Children.OfType<Panel>());

        if (_panels.Count < 64)
        {
            throw new InvalidOperationException($"Expected at least 64 panels on board grid, found {_panels.Count}.");
        }

        _board = new Piece?[8, 8];
        InitializeStartingBoard();

        _currentTurn = PieceColor.White;
        _selectedPosition = null;
        _highlightedMoves.Clear();
        _isGameOver = false;

        for (int i = 0; i < 64; i++)
        {
            var panel = _panels[i];
            _originalTileBrushes[i] = panel.Background;

            panel.Tag = Position.FromIndex(i);
            panel.PointerPressed -= OnSquarePointerPressed;
            panel.PointerPressed += OnSquarePointerPressed;

            panel.Children.Clear();
            var pos = Position.FromIndex(i);
            var piece = _board[pos.Row, pos.Column];
            if (piece != null)
            {
                panel.Children.Add(CreatePieceImage(piece.ImageFileName));
            }
        }

        UpdateTurnIndicators();
    }

    /// <summary>
    /// Restarts the chess game, resetting board state, active player turn, and UI indicators.
    /// </summary>
    public void RestartGame()
    {
        LoadChessPiecesPictures();
    }

    /// <summary>
    /// Initializes the standard starting layout of pieces on the 8x8 chessboard.
    /// </summary>
    private void InitializeStartingBoard()
    {
        // Rank 8: Black major pieces
        _board[0, 0] = new Rook(PieceColor.Black);
        _board[0, 1] = new Knight(PieceColor.Black);
        _board[0, 2] = new Bishop(PieceColor.Black);
        _board[0, 3] = new Queen(PieceColor.Black);
        _board[0, 4] = new King(PieceColor.Black);
        _board[0, 5] = new Bishop(PieceColor.Black);
        _board[0, 6] = new Knight(PieceColor.Black);
        _board[0, 7] = new Rook(PieceColor.Black);

        // Rank 7: Black pawns
        for (int col = 0; col < 8; col++)
        {
            _board[1, col] = new Pawn(PieceColor.Black);
        }

        // Rank 2: White pawns
        for (int col = 0; col < 8; col++)
        {
            _board[6, col] = new Pawn(PieceColor.White);
        }

        // Rank 1: White major pieces
        _board[7, 0] = new Rook(PieceColor.White);
        _board[7, 1] = new Knight(PieceColor.White);
        _board[7, 2] = new Bishop(PieceColor.White);
        _board[7, 3] = new Queen(PieceColor.White);
        _board[7, 4] = new King(PieceColor.White);
        _board[7, 5] = new Bishop(PieceColor.White);
        _board[7, 6] = new Knight(PieceColor.White);
        _board[7, 7] = new Rook(PieceColor.White);
    }

    /// <summary>
    /// Event handler invoked when a user clicks on any board square panel.
    /// </summary>
    /// <param name="sender">The square panel that triggered the event.</param>
    /// <param name="e">Pointer event arguments.</param>
    private void OnSquarePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Panel panel || panel.Tag is not Position clickedPos)
        {
            return;
        }

        HandleSquareClick(clickedPos);
    }

    /// <summary>
    /// Event handler invoked when a player clicks the "PLAY AGAIN" button after a match concludes.
    /// </summary>
    /// <param name="sender">The button that triggered the event.</param>
    /// <param name="e">Routed event arguments.</param>
    private void OnPlayAgainClicked(object? sender, RoutedEventArgs e)
    {
        RestartGame();
    }

    /// <summary>
    /// Handles user interaction when clicking a square, managing selection, move execution, and turn switching.
    /// </summary>
    /// <param name="clickedPos">The board position clicked by the user.</param>
    public void HandleSquareClick(Position clickedPos)
    {
        if (_isGameOver)
        {
            return;
        }

        // Case 1: Clicking on a valid highlighted move destination
        if (_selectedPosition != null && _highlightedMoves.Contains(clickedPos))
        {
            var movingPiece = _board[_selectedPosition.Value.Row, _selectedPosition.Value.Column];
            if (movingPiece != null)
            {
                ExecuteMove(_selectedPosition.Value, clickedPos, movingPiece);
            }
            return;
        }

        var clickedPiece = _board[clickedPos.Row, clickedPos.Column];

        // Case 2: Clicking on a piece belonging to the active player's team
        if (clickedPiece != null && clickedPiece.Color == _currentTurn)
        {
            if (_selectedPosition == clickedPos)
            {
                // Clicking the already selected piece deselects it
                ClearHighlights();
                _selectedPosition = null;
                _highlightedMoves.Clear();
            }
            else
            {
                // Select newly clicked friendly piece and show possible moves
                ClearHighlights();
                _selectedPosition = clickedPos;
                _highlightedMoves = clickedPiece.GetPossibleMoves(clickedPos, _board);
                HighlightPossibleMoves(_highlightedMoves);
            }
            return;
        }

        // Case 3: Clicking on an invalid square or enemy piece not in possible moves clears selection
        ClearHighlights();
        _selectedPosition = null;
        _highlightedMoves.Clear();
    }

    /// <summary>
    /// Executes a move from source to destination position, updating board state, handling captures,
    /// promotions, castling, and switching turns.
    /// </summary>
    /// <param name="from">The starting square position.</param>
    /// <param name="to">The destination square position.</param>
    /// <param name="movingPiece">The piece being moved.</param>
    private void ExecuteMove(Position from, Position to, Piece movingPiece)
    {
        ClearHighlights();

        // Handle castling rook movement
        if (movingPiece is King && Math.Abs(to.Column - from.Column) == 2)
        {
            if (to.Column == 6)
            {
                // Kingside castling: move rook from column 7 to 5
                var kingsideRook = _board[to.Row, 7];
                _board[to.Row, 7] = null;
                _board[to.Row, 5] = kingsideRook;
                if (kingsideRook != null)
                {
                    kingsideRook.HasMoved = true;
                }

                _panels[new Position(to.Row, 7).ToIndex()].Children.Clear();
                var rookPanel = _panels[new Position(to.Row, 5).ToIndex()];
                rookPanel.Children.Clear();
                if (kingsideRook != null)
                {
                    rookPanel.Children.Add(CreatePieceImage(kingsideRook.ImageFileName));
                }
            }
            else if (to.Column == 2)
            {
                // Queenside castling: move rook from column 0 to 3
                var queensideRook = _board[to.Row, 0];
                _board[to.Row, 0] = null;
                _board[to.Row, 3] = queensideRook;
                if (queensideRook != null)
                {
                    queensideRook.HasMoved = true;
                }

                _panels[new Position(to.Row, 0).ToIndex()].Children.Clear();
                var rookPanel = _panels[new Position(to.Row, 3).ToIndex()];
                rookPanel.Children.Clear();
                if (queensideRook != null)
                {
                    rookPanel.Children.Add(CreatePieceImage(queensideRook.ImageFileName));
                }
            }
        }

        var capturedPiece = _board[to.Row, to.Column];
        _board[from.Row, from.Column] = null;

        // Handle pawn promotion to Queen upon reaching the opponent's back rank
        if (movingPiece is Pawn && (to.Row == 0 || to.Row == 7))
        {
            movingPiece = new Queen(movingPiece.Color) { HasMoved = true };
        }
        else
        {
            movingPiece.HasMoved = true;
        }

        _board[to.Row, to.Column] = movingPiece;

        // Update UI panels for moving piece and captured piece
        _panels[from.ToIndex()].Children.Clear();
        var destinationPanel = _panels[to.ToIndex()];
        destinationPanel.Children.Clear();
        destinationPanel.Children.Add(CreatePieceImage(movingPiece.ImageFileName));

        _selectedPosition = null;
        _highlightedMoves.Clear();

        // Check if an enemy king was captured to conclude the game
        if (capturedPiece is King)
        {
            _isGameOver = true;
            bool whiteWon = movingPiece.Color == PieceColor.White;

            if (_whiteTurnTextBlock != null)
            {
                _whiteTurnTextBlock.Text = whiteWon ? "White Wins!" : "";
                _whiteTurnTextBlock.IsVisible = whiteWon;
            }

            if (_whitePlayAgainButton != null)
            {
                _whitePlayAgainButton.IsVisible = whiteWon;
            }

            if (_blackTurnTextBlock != null)
            {
                _blackTurnTextBlock.Text = !whiteWon ? "Black Wins!" : "";
                _blackTurnTextBlock.IsVisible = !whiteWon;
            }

            if (_blackPlayAgainButton != null)
            {
                _blackPlayAgainButton.IsVisible = !whiteWon;
            }

            return;
        }

        // Toggle turn between White and Black
        _currentTurn = _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
        UpdateTurnIndicators();
    }

    /// <summary>
    /// Highlights the specified valid move positions in green tiles on the board.
    /// </summary>
    /// <param name="moves">The collection of valid board positions to highlight.</param>
    private void HighlightPossibleMoves(IEnumerable<Position> moves)
    {
        foreach (var move in moves)
        {
            int index = move.ToIndex();
            if (index >= 0 && index < _panels.Count)
            {
                _panels[index].Background = _highlightBrush;
            }
        }
    }

    /// <summary>
    /// Restores all chessboard square panels back to their original alternating background brushes.
    /// </summary>
    private void ClearHighlights()
    {
        for (int i = 0; i < _panels.Count && i < _originalTileBrushes.Length; i++)
        {
            if (_originalTileBrushes[i] != null)
            {
                _panels[i].Background = _originalTileBrushes[i];
            }
        }
    }

    /// <summary>
    /// Updates the visibility and text of the turn indicator labels and action buttons on both sides of the chessboard.
    /// </summary>
    private void UpdateTurnIndicators()
    {
        if (_whiteTurnTextBlock != null)
        {
            _whiteTurnTextBlock.Text = "White's Turn";
            _whiteTurnTextBlock.IsVisible = (_currentTurn == PieceColor.White);
        }

        if (_whitePlayAgainButton != null)
        {
            _whitePlayAgainButton.IsVisible = false;
        }

        if (_blackTurnTextBlock != null)
        {
            _blackTurnTextBlock.Text = "Black's Turn";
            _blackTurnTextBlock.IsVisible = (_currentTurn == PieceColor.Black);
        }

        if (_blackPlayAgainButton != null)
        {
            _blackPlayAgainButton.IsVisible = false;
        }
    }

    /// <summary>
    /// Creates a properly configured <see cref="Image"/> control displaying a chess piece bitmap.
    /// </summary>
    /// <param name="fileName">The filename of the image asset to load.</param>
    /// <returns>A configured <see cref="Image"/> instance.</returns>
    private static Image CreatePieceImage(string fileName)
    {
        var bitmap = LoadBitmapAsset(fileName);
        return new Image
        {
            Source = bitmap,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            IsHitTestVisible = false
        };
    }

    /// <summary>
    /// Loads a piece bitmap asset by filename, resolving it from embedded Avalonia resources, the application base directory,
    /// or local project assets with in-memory caching and path traversal validation.
    /// </summary>
    /// <param name="fileName">The filename of the image asset to load (e.g., "pawn_white.png").</param>
    /// <returns>A loaded <see cref="Bitmap"/> instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the asset image cannot be found in any known location.</exception>
    private static Bitmap LoadBitmapAsset(string fileName)
    {
        string safeFileName = Path.GetFileName(fileName);

        if (_bitmapCache.TryGetValue(safeFileName, out var cachedBitmap))
        {
            return cachedBitmap;
        }

        var resourceUri = new Uri($"avares://AvaloniaChess/assets/{safeFileName}");
        if (AssetLoader.Exists(resourceUri))
        {
            using var stream = AssetLoader.Open(resourceUri);
            var bitmap = new Bitmap(stream);
            _bitmapCache[safeFileName] = bitmap;
            return bitmap;
        }

        var localPath = Path.Combine(AppContext.BaseDirectory, "assets", safeFileName);
        if (File.Exists(localPath))
        {
            var bitmap = new Bitmap(localPath);
            _bitmapCache[safeFileName] = bitmap;
            return bitmap;
        }

        var currentDirProjectAssetPath = Path.Combine(Directory.GetCurrentDirectory(), "AvaloniaChess", "assets", safeFileName);
        if (File.Exists(currentDirProjectAssetPath))
        {
            var bitmap = new Bitmap(currentDirProjectAssetPath);
            _bitmapCache[safeFileName] = bitmap;
            return bitmap;
        }

        throw new FileNotFoundException($"Chess piece asset could not be found: {safeFileName}");
    }
}
