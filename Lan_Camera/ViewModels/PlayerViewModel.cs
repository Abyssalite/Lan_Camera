using System;
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
    public string? View { set; get; }
    public string? WebRtcUrl { set; get; }

    private enum Selection
    {
        front,
        back,
        middle
    }
    private int _viewSelect = 0;

    private bool? _toggleCamera  = true;
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

    public ICommand? SwitchCamCommand { get; }
    
    private readonly HttpClient _http = new();
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
        WebRtcUrl = $"http://{address}:8889/cam/";
        _baseUrl = $"http://{address}:5000";

        SwitchCamCommand = new AsyncRelayCommand(SwitchCamAsync);
        Reload();
    }

    private void Reload()
    {
        // Force WebView to reload
        var url = WebRtcUrl;
        WebRtcUrl = null;
        WebRtcUrl = url;
        Console.WriteLine(WebRtcUrl);
    }

    private async Task SwitchCamAsync()
    {
        try
        {
            _viewSelect = (_viewSelect + 1) % 3;
            var response = await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");
            View = $"{(Selection)_viewSelect}";

            await Task.Delay(1000); // give server time to restart pipeline
            Reload();
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
            if (isCameraOn)
            {
                // Turn off
                await _http.GetAsync($"{_baseUrl}/switch?cam=off");
                WebRtcUrl = null;;
            }
            else
            {
                await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");
                View = $"{(Selection)_viewSelect}";

                await Task.Delay(1000);
                Reload();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Toggle failed: {ex.Message}");
        }
    }

    private async Task ClearAsync()
    {
        _http.Dispose();
        _store.SelectCamera(null);
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
