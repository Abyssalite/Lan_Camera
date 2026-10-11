using CommunityToolkit.Mvvm.ComponentModel;
using Custom_EventHub;
using Custom_Navigation;
using Custom_Popup;

namespace Lan_Camera.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IViewHost _viewhost;
    public IViewHost ViewHost => _viewhost;
    private readonly IPopupHost _popuphost;
    public IPopupHost Popuphost => _popuphost;

    public MainViewModel(
        Store store,
        IViewHost viewHost,
        INavigatorService navigator,
        IEventHub events,
        IPopupHost popup
    ):base(store, navigator, events, popup)
    {
        _viewhost = viewHost;
        _popuphost = popup;
    }
}
