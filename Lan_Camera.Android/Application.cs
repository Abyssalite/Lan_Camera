using Android.App;
using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Custom_EventHub;
using Microsoft.Extensions.DependencyInjection;

namespace Lan_Camera.Android
{
    [Application]
    public class Application : AvaloniaAndroidApplication<App>
    {
        protected Application(nint javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
        {
            App.collection.AddCustomNavigation();
            App.collection.AddCustomEventHub();
            App.collection.AddAndroidCommonServices();
            App.Services = App.collection.BuildServiceProvider();
            
            return base.CustomizeAppBuilder(builder)
            .WithInterFont();
        }
    }
}
