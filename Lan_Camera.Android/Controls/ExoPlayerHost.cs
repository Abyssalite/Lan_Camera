using System;
using Android.Views;
using AndroidX.Media3.UI;
using Avalonia.Android;
using Avalonia.Controls;
using Avalonia.Platform;
using Lan_Camera.Android.ViewModels;

namespace Lan_Camera.Android.Controls;

public class ExoPlayerHost : NativeControlHost
{
    private PlayerView? _playerView;

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        var context = (parent as AndroidViewControlHandle)?.View.Context
                      ?? throw new InvalidOperationException("No Android Context");

        _playerView = new PlayerView(context)
        {
            LayoutParameters = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent)
        };

        // Attach ExoPlayer when DataContext is ready
        if (DataContext is PlayerViewModel vm)
            vm.Attach(_playerView);

        DataContextChanged += (_, _) =>
        {
            if (DataContext is PlayerViewModel vm && _playerView != null)
                vm.Attach(_playerView);
        };

        return new AndroidViewControlHandle(_playerView);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        _playerView = null;
        base.DestroyNativeControlCore(control);
    }
}