using System;
using System.Threading.Tasks;
using Avalonia;
using Custom_EventHub;
using Lan_Camera.Service;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
namespace Lan_Camera.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        App.collection.AddCustomNavigation();
        App.collection.AddCustomEventHub();
        App.collection.AddDesktopCommonServices();
        App.Services = App.collection.BuildServiceProvider();

        _ = Task.Run(() => Core.Initialize());

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);  
    } 

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
