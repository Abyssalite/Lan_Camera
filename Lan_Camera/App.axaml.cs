using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lan_Camera.ViewModels;
using Lan_Camera.Views;
using Microsoft.Extensions.DependencyInjection;
using Custom_Navigation;
using Custom_EventHub;
using System.Threading.Tasks;
using LibVLCSharp.Shared;

namespace Lan_Camera;

public partial class App : Application
{
    public static ServiceProvider? Services { get; internal set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _ = Task.Run(() => Core.Initialize());

        // Register all the services needed for the application to run
        IServiceCollection collection = new ServiceCollection();
        collection.AddCommonServices();
        collection.AddCustomNavigation();
        collection.AddCustomEventHub();

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        Services = collection.BuildServiceProvider();
        var vm = Services.GetRequiredService<MainViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime)
        {
            singleViewFactoryApplicationLifetime.MainViewFactory = () => new MainView { DataContext = vm };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}