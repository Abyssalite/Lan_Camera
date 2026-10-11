using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Lan_Camera.Views;

public partial class SelectView : UserControl
{
    private CancellationTokenSource? _resizeToken;
    private bool _layoutInitialized = false;

    public SelectView()
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
            CameraSelectStack.Margin = new Thickness(20, 50, 20, 50);
            CameraSelectBorder.Margin = new Thickness(20, 50, 20, 50);

            CameraSelectListBox.Margin = new Thickness(0, 0, 0, 8);
            CameraSelectListBox.Classes.Remove("horizontal");
            CameraSelectListBox.Classes.Add("vertical");

            CameraSelectStack.RowDefinitions = new RowDefinitions("*,Auto");
            CameraSelectStack.ColumnDefinitions = new ColumnDefinitions("*");

            Grid.SetRow(AddCameraCommandButton, 1);
            Grid.SetColumn(AddCameraCommandButton, 0);
        }

        else if (width > 800)
        {
            CameraSelectStack.Margin = new Thickness(20, 100, 20, 100);
            CameraSelectBorder.Margin = new Thickness(100, 0, 100, 0);
            
            CameraSelectListBox.Margin = new Thickness(0, 0, 10, 0);
            CameraSelectListBox.Classes.Remove("vertical");
            CameraSelectListBox.Classes.Add("horizontal");

            CameraSelectStack.RowDefinitions = new RowDefinitions("*");
            CameraSelectStack.ColumnDefinitions = new ColumnDefinitions("*,Auto");

            Grid.SetRow(AddCameraCommandButton, 0);
            Grid.SetColumn(AddCameraCommandButton, 1);
        }

        if (!_layoutInitialized)
        {
            CameraSelectBorder.IsVisible = true;
            _layoutInitialized = true;
        }
    }
}