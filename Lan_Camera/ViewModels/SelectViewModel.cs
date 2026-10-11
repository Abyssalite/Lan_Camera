
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Custom_Navigation;
using Custom_EventHub;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Custom_Popup;

namespace Lan_Camera.ViewModels;

public partial class SelectViewModel : ViewModelBase
{    
    public ICommand AddCameraCommand { get; }

    public ObservableCollection<Camera>? CamerasList { get; }
    private Camera? _selectedCamera;
    public Camera? SelectedCamera
    {
        get => _selectedCamera;
        set
        {
            if (value == null || _selectedCamera == value) return;

            _selectedCamera = value;

            _ = OpenCameraAsync(_selectedCamera);
            _selectedCamera = null;
            OnPropertyChanged(nameof(SelectedCamera));
        }
    }
    
    public SelectViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        IPopupHost popup
    ):base(store, navigator, events, popup)
    {
        CamerasList = _store.CamerasList;
        AddCameraCommand = new AsyncRelayCommand(addCameraAsync);
    }

    async Task addCameraAsync()
    {
        var vm = App.Services?.GetRequiredService<AddCameraViewModel>();
        _selectedCamera = null;
        OnPropertyChanged(nameof(SelectedCamera));

        await _navigator.NavigateMain(vm);     
    }

    private async Task OpenCameraAsync(Camera camera)
    {
        _store.SelectCamera(camera);

        var vm = App.Services?.GetRequiredService<PlayerViewModel>();
        await _navigator.NavigateMain(vm);     
    }
}
