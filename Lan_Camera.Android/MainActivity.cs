using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Custom_Navigation;

namespace Lan_Camera.Android;

[Activity(
    Label = "Lan Camera",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
    public override void OnBackPressed()
    {
        var navigatorService = App.Services?.GetRequiredService<INavigatorService>();
        
        if (navigatorService == null) return;
        if (navigatorService.IsExit())
        {        
            Finish();
        }
        else  
        {        
            navigatorService.OpenPrevious();
        }
    }
}