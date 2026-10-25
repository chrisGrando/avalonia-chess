using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaChess;

/// <summary>
/// Interaction logic for the main application window displaying the chess board.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Instance of the game application logic controller.
    /// </summary>
    private AppLogic? _appLogic;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class and registers lifecycle events.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// Event handler invoked after the window UI has completed loading to initialize game logic and load pieces.
    /// </summary>
    /// <param name="sender">The source of the loaded event.</param>
    /// <param name="e">The routed event arguments.</param>
    private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        _appLogic = new AppLogic(this);
        _appLogic.LoadChessPiecesPictures();
    }
}
