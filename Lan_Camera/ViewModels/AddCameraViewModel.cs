using Custom_Navigation;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using Custom_EventHub;
using Custom_Popup;

namespace Lan_Camera.ViewModels;

public partial class AddCameraViewModel : ViewModelBase, IHandleBackNavigation
{    
    public string? Address { get; set; }
    public string? Name { get; set; }
    public ICommand? AddCameraCommand { get; }

    public AddCameraViewModel(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        IPopupHost popup
    ):base(store, navigator, events, popup)    {
        AddCameraCommand = new AsyncRelayCommand(AddCamera);
    }

    private async Task ClearAsync()
    {
        Name = string.Empty;
        OnPropertyChanged(nameof(Name));
        Address = string.Empty;
        OnPropertyChanged(nameof(Address));
    }

    private async Task AddCamera()
    {
        string name = Helpers.InputOrDefault(Name, "");
        if (name == "")
            return;

        string address = Helpers.InputOrDefault(Address, "");
        if (!Helpers.IsValidIP(address))
            return;

        var camera = new Camera()
        {
            Name = name,
            Address = address
        };

        var result = await _store.StoreAddCamera(camera);
        if (!result) return;
        
        await _navigator.OpenPrevious();
    }

    async Task<bool> IHandleBackNavigation.HandleBackAsync()
    {
        await ClearAsync();
        return await Task.FromResult(false);
    }
}
