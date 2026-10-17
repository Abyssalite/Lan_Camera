using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Custom_EventHub;
using Custom_Navigation;
using Custom_Popup;
using LibVLCSharp.Shared;

using Lan_Camera.ViewModels;
using Lan_Camera.Service;
namespace Lan_Camera.Desktop.ViewModels;

public partial class PlayerViewModel : ViewModelBase, IHandleBackNavigation , IPlayerViewModel
{
    public Camera? SelectedCamera { get; }
    [ObservableProperty]
    private string? _view = "none";
    private readonly HttpClient _http = new();
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    public MediaPlayer? MediaPlayer => _mediaPlayer;

    private enum Selection
    {
        front,
        back,
        center
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

    public ICommand? SwitchCamCommand { get; }
    public ICommand? ReloadCommand { get; }

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
        ReloadCommand = new AsyncRelayCommand(ReloadAsync);

        InitPlayer();
    }
    
    private void InitPlayer()
    {
        _libVLC = new LibVLC(
            "--no-audio",
            "--quiet",
            "--network-caching=1000",
            "--avcodec-hw=any"
        );
        _mediaPlayer = new MediaPlayer(_libVLC);
    }

    private void Stop()
    {
        if (_mediaPlayer == null) return;
        _mediaPlayer.Stop();
        OnPropertyChanged(nameof(MediaPlayer));
    }

    private void Play()
    {
        Stop();
        if (_mediaPlayer == null || _libVLC == null) return;

        using var media = new Media(_libVLC, _rtspUrl, FromType.FromLocation);
        media.AddOption(":rtsp-tcp");
        media.AddOption(":network-caching=1000");
        _mediaPlayer.Play(media);
    }

    private async Task ReloadAsync()
    {
        try
        {
            var status = await _http.GetAsync($"{_baseUrl}/status");
            status.EnsureSuccessStatusCode();
            var body = await status.Content.ReadAsStringAsync(); 
            Console.WriteLine(body);
            
            View = $"{JsonSerializer.Deserialize<Respond>(body)?.Current ?? "none"}";
            if (Enum.TryParse<Selection>(View, out var s))
                _viewSelect = (int)s;
            
            Play();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Reload failed: {ex.Message}");
        }
    }

    private async Task SwitchCamAsync()
    {
        try
        {
            _viewSelect = (_viewSelect + 1) % Enum.GetNames(typeof(Selection)).Length;
            var response = await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");

            string body = "";
            do
            {
                var status = await _http.GetAsync($"{_baseUrl}/status");
                status.EnsureSuccessStatusCode();
                body = await status.Content.ReadAsStringAsync(); 
                Console.WriteLine(body);
                await Task.Delay(200);
            }
            while (JsonSerializer.Deserialize<Respond>(body)?.Ready != true);
            View = $"{(Selection)_viewSelect}";
            Play();
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
                Stop();
                await _http.GetAsync($"{_baseUrl}/switch?cam=none");
                View = "none";
            }
            else
            {
                await _http.GetAsync($"{_baseUrl}/switch?cam={(Selection)_viewSelect}");

                string body = "";
                do
                {
                    var status = await _http.GetAsync($"{_baseUrl}/status");
                    status.EnsureSuccessStatusCode();
                    body = await status.Content.ReadAsStringAsync(); 
                    Console.WriteLine(body);
                    await Task.Delay(200);
                }
                while (JsonSerializer.Deserialize<Respond>(body)?.Ready != true);
                View = $"{(Selection)_viewSelect}";
                Play();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Toggle failed: {ex.Message}");
        }
    }

    private async Task ClearAsync()
    {
        try
        {
            await _http.GetAsync($"{_baseUrl}/switch?cam=none");
            Stop();
            _mediaPlayer = null;
            _libVLC?.Dispose();
            _http.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Clear failed: {ex.Message}");
        }
        _store.SelectCamera(null);
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
