using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Custom_EventHub;
using Custom_Navigation;
using Custom_Popup;

namespace Lan_Camera.ViewModels;

public partial class PlayerViewModel : ViewModelBase, IHandleBackNavigation
{
    public Camera? SelectedCamera { get; }
    [ObservableProperty]
    private string? _view = "none";

    private enum Selection
    {
        front,
        back,
        middle
    }
    private int _viewSelect = 0;

    private bool? _toggleCamera  = false;
    public bool? ToggleCamera 
    { 
        get => _toggleCamera;
        set
        {
            if (value == null || value == _toggleCamera) return;

            _toggleCamera = value;
            _ = ToggleCamAsync(_toggleCamera ?? false);
            OnPropertyChanged(nameof(ToggleCamera));
        }
    }
    private bool? _togglePlay  = false;
    public bool? TogglePlay
    { 
        get => _togglePlay;
        set
        {
            if (value == null || value == _togglePlay) return;

            _togglePlay = value;
            if (_togglePlay ?? false) StartPlay();
            else StopPlay();
            OnPropertyChanged(nameof(TogglePlay));
        }
    }

    public ICommand? SwitchCamCommand { get; }
    
    private readonly HttpClient _http = new();
    private Process? _ffplay;
    private string _rtspUrl = "";
    private string _baseUrl = "";
    public PlayerViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        IPopupHost popup
    ):base(store, navigator, events, popup)
    {        
        SelectedCamera = _store.SelectedCamera;
        if (SelectedCamera == null) return;

        var address = SelectedCamera.Address.Trim();
        _rtspUrl = $"rtsp://{address}:8554/cam";
        _baseUrl = $"http://{address}:5000";

        SwitchCamCommand = new AsyncRelayCommand(SwitchCamAsync);
    }

private void StartPlay()
    {
        StopPlay();

        if (string.IsNullOrWhiteSpace(_rtspUrl))
        {
            return;
        }

        try
        {
            _ffplay = Process.Start(new ProcessStartInfo
            {
                FileName = "ffplay",
                Arguments =
                    $"-rtsp_transport tcp " +
                    $"-fflags nobuffer " +
                    $"-flags low_delay " +
                    $"-sync ext " +
                    $"-window_title \"LAN Camera\" " +
                    $"\"{_rtspUrl}\"",
                UseShellExecute = false,
                CreateNoWindow = false
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ffplay failed: {ex.Message}");
        }
    }

    private void StopPlay()
    {
        if (_ffplay != null)
        {
            try
            {
                if (!_ffplay.HasExited)
                    _ffplay.Kill(entireProcessTree: true);
            }
            catch { /* ignore */ }
            finally
            {
                _ffplay.Dispose();
                _ffplay = null;
            }
        }
    }

    private async Task SwitchCamAsync()
    {
        try
        {
            _viewSelect = (_viewSelect + 1) % 3;
            var response = await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");
            View = $"{(Selection)_viewSelect}";

            await Task.Delay(2000); // give server time to restart pipeline
            StartPlay();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Switch failed: {ex.Message}");
        }
    }

    private async Task ToggleCamAsync(bool isCameraOn)
    {
        try
        {
            if (!isCameraOn)
            {               
                TogglePlay = false;
                // Turn off
                await _http.GetAsync($"{_baseUrl}/switch?cam=none");
                View = "none";
            }
            else
            {
                await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");
                View = $"{(Selection)_viewSelect}";

                await Task.Delay(2000);
                StartPlay();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Toggle failed: {ex.Message}");
        }
    }

    private async Task ClearAsync()
    {
        StopPlay();
        _http.Dispose();
        _store.SelectCamera(null);
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
