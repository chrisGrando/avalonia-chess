using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace AvaloniaChess;

/// <summary>
/// Represents the main Avalonia application, responsible for initializing application resources
/// and setting up the desktop lifetime.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Loads the application's XAML definition and associated styles and resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Finalizes framework initialization by instantiating the main window for classic desktop lifetimes.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
