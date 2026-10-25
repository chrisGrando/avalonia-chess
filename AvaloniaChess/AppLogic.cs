using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace AvaloniaChess;

/// <summary>
/// Controls the core game logic and UI synchronization for the 2-player chess game.
/// </summary>
/// <param name="mainWindow">Optional reference to the main window hosting the chess board.</param>
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
    /// Loads the required chess piece image assets from disk or embedded resources and positions
    /// each piece in its initial starting location on the board grid.
    /// </summary>
    /// <param name="window">
    /// Optional window containing the chess board. If not provided, falls back to the instance's
    /// main window or active desktop lifetime main window.
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

        ReadOnlySpan<string?> initialBoard = [
            // Row 8 (Black main pieces)
            "rook_black.png", "knight_black.png", "bishop_black.png", "queen_black.png", "king_black.png", "bishop_black.png", "knight_black.png", "rook_black.png",
            // Row 7 (Black pawns)
            "pawn_black.png", "pawn_black.png", "pawn_black.png", "pawn_black.png", "pawn_black.png", "pawn_black.png", "pawn_black.png", "pawn_black.png",
            // Row 6
            null, null, null, null, null, null, null, null,
            // Row 5
            null, null, null, null, null, null, null, null,
            // Row 4
            null, null, null, null, null, null, null, null,
            // Row 3
            null, null, null, null, null, null, null, null,
            // Row 2 (White pawns)
            "pawn_white.png", "pawn_white.png", "pawn_white.png", "pawn_white.png", "pawn_white.png", "pawn_white.png", "pawn_white.png", "pawn_white.png",
            // Row 1 (White main pieces)
            "rook_white.png", "knight_white.png", "bishop_white.png", "queen_white.png", "king_white.png", "bishop_white.png", "knight_white.png", "rook_white.png"
        ];

        var panels = boardGrid.Children.OfType<Panel>().ToList();
        if (panels.Count < 64)
        {
            throw new InvalidOperationException($"Expected at least 64 panels on board grid, found {panels.Count}.");
        }

        for (int i = 0; i < 64; i++)
        {
            var panel = panels[i];
            panel.Children.Clear();

            string? assetName = initialBoard[i];
            if (!string.IsNullOrEmpty(assetName))
            {
                var bitmap = LoadBitmapAsset(assetName);
                var image = new Image
                {
                    Source = bitmap,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };

                panel.Children.Add(image);
            }
        }
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
