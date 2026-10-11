using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Lan_Camera.Views;

public partial class AddCameraView : UserControl
{
    private CancellationTokenSource? _resizeToken;
    private bool _layoutInitialized = false;

    public AddCameraView()
    {
        InitializeComponent();

        this.LayoutUpdated += (_, __) =>
        {
            _resizeToken?.Cancel();
            _resizeToken = new CancellationTokenSource();

            var token = _resizeToken.Token;
            Task.Delay(10, token).ContinueWith(t =>
            {
                if (t.IsCanceled) return;
                Dispatcher.UIThread.InvokeAsync(AdjustStackOrientation);
            });
        };
    }

    private void AdjustStackOrientation()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var width = topLevel.Bounds.Width;

        if (width < 800)
        {
            CameraAddBorder.Margin = new Thickness(20, 50, 20, 50);
            CameraAddTextStack.Margin = new Thickness(0, 0, 0, 20);
            CameraAddStack.Margin = new Thickness(-30, 103, 30, 102);

            CameraAddStack.RowDefinitions = new RowDefinitions("*,Auto");
            CameraAddStack.ColumnDefinitions = new ColumnDefinitions("*");

            Grid.SetRow(CameraAddButton, 1);
            Grid.SetColumn(CameraAddButton, 0);
        }

        else if (width > 800)
        {
            CameraAddBorder.Margin = new Thickness(100, 0, 100, 0);
            CameraAddTextStack.Margin = new Thickness(0, 0, 50, 0);
            CameraAddStack.Margin = new Thickness(40, 103, 90, 102);

            CameraAddStack.RowDefinitions = new RowDefinitions("*");
            CameraAddStack.ColumnDefinitions = new ColumnDefinitions("*,Auto");
            
            Grid.SetRow(CameraAddButton, 0);
            Grid.SetColumn(CameraAddButton, 1);
        }

        if (!_layoutInitialized)
        {
            CameraAddBorder.IsVisible = true;
            _layoutInitialized = true;
        }
    }
}