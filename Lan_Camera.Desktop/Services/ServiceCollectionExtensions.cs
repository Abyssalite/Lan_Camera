using Lan_Camera.Desktop.ViewModels;
using Lan_Camera.Service;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddDesktopCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<Store>();

        collection.AddTransient<IPlayerViewModel, PlayerViewModel>();
    }
}