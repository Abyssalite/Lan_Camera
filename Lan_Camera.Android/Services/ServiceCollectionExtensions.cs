using Lan_Camera.Android.ViewModels;
using Lan_Camera.Service;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddAndroidCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<Store>();

        collection.AddTransient<IPlayerViewModel, PlayerViewModel>();
    }
}