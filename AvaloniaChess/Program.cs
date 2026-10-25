using System;
using Avalonia;

namespace AvaloniaChess;

/// <summary>
/// Application entry point and runtime bootstrapper.
/// </summary>
internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    /// <summary>
    /// Main entry point for the application execution.
    /// </summary>
    /// <param name="args">Command line arguments passed to the application.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    /// <summary>
    /// Configures and builds the Avalonia application instance.
    /// </summary>
    /// <returns>A configured <see cref="AppBuilder"/> instance ready for execution or visual designer hosting.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
