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
using AndroidX.Media3.Common;
using AndroidX.Media3.ExoPlayer;
using AndroidX.Media3.UI;

using Lan_Camera.ViewModels;
using Lan_Camera.Service;
namespace Lan_Camera.Android.ViewModels;

public partial class PlayerViewModel : ViewModelBase, IHandleBackNavigation , IPlayerViewModel
{
    public Camera? SelectedCamera { get; }
    [ObservableProperty]
    private string? _view = "none";
    private readonly HttpClient _http = new();
    private IExoPlayer? _player;
    private PlayerView? _playerView;

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
 
    private string _hlsUrl = "";
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
        _hlsUrl = $"http://{address}:8888/cam/main_stream.m3u8";
        _baseUrl = $"http://{address}:5000";

        SwitchCamCommand = new AsyncRelayCommand(SwitchCamAsync);
        ReloadCommand = new AsyncRelayCommand(ReloadAsync);
    }
    
    public void Attach(PlayerView playerView)
    {
        Release();

        _playerView = playerView;
        _playerView.UseController = false;

        _player = new ExoPlayerBuilder(playerView.Context!).Build();
        _playerView.Player = _player;
    }

    public void Stop()
    {
        if (_player == null) return;
    
        _player.PlayWhenReady = false;
        _player.Stop();
        _player.ClearMediaItems();
    }

    public void Play()
    {
        Stop();
        if (_player == null) return;

        var mediaItem = MediaItem.FromUri(_hlsUrl);
        _player.SetMediaItem(mediaItem);
        _player.Prepare();
        _player.PlayWhenReady = true;
    }

    private async Task ReloadAsync()
    {
        try
        {
            var status = await _http.GetAsync($"{_baseUrl}/status");
            status.EnsureSuccessStatusCode();
            var body = await status.Content.ReadAsStringAsync(); 
            
            View = $"{JsonSerializer.Deserialize<Respond>(body)?.Current ?? "none"}";
            if (Enum.TryParse<Selection>(View, out var s))
                _viewSelect = (int)s;
            
            Play();
        }
        catch (Exception ex)
        {
            View = $"Reload failed: {ex.Message}";
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
                await Task.Delay(200);
            }
            while (JsonSerializer.Deserialize<Respond>(body)?.Ready != true);
            View = $"{(Selection)_viewSelect}";
            Play();
        }
        catch (Exception ex)
        {
            View = $"Switch failed: {ex.Message}";
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
                    await Task.Delay(200);
                }
                while (JsonSerializer.Deserialize<Respond>(body)?.Ready != true);
                View = $"{(Selection)_viewSelect}";
                Play();
            }
        }
        catch (Exception ex)
        {
            View = $"Toggle failed: {ex.Message}";
        }
    }

    public void Release()
    {
        try
        {
            if (_playerView != null)
                _playerView.Player = null;

            _player?.Release();
        }
        catch (Exception ex)
        {
            View = $"R{ex.Message}";
        }
        finally
        {
            _playerView = null;
            _player = null; 
        }
    }

    private async Task ClearAsync()
    {
        try
        {
            await _http.GetAsync($"{_baseUrl}/switch?cam=none");
            Stop();
            Release();
            _http.Dispose();
        }
        catch (Exception ex)
        {
            View = $"C{ex.Message}";
        }
        _store.SelectCamera(null);
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
