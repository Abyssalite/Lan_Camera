using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Custom_EventHub;
using Custom_Navigation;
using Custom_Popup;

namespace Lan_Camera.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected readonly Store _store;
    protected readonly INavigatorService _navigator;
    protected readonly IEventHub _events;
    protected readonly IPopupHost _popup;

    protected readonly List<IDisposable> _subscriptions = new();
    public ICommand? BackCommand { get; }
    
    protected ViewModelBase(
        Store store,
        INavigatorService navigator,
        IEventHub events,
        IPopupHost popup
    ){
        _store = store;
        _navigator = navigator;
        _events = events;
        _popup = popup;
        BackCommand = new AsyncRelayCommand(BackAsync);
    }

    protected virtual async Task BackAsync()
    {
        await _navigator.OpenPrevious();
        await Task.CompletedTask;
    }
}
