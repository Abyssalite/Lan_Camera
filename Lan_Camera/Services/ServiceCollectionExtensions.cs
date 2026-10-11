using Lan_Camera.ViewModels;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<Store>();

        collection.AddTransient<MainViewModel>();
        collection.AddTransient<PlayerViewModel>();
        collection.AddTransient<SelectViewModel>();
        collection.AddTransient<AddCameraViewModel>();
    }
}