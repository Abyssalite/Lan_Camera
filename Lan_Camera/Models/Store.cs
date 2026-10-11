
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Custom_EventHub;
using Lan_Camera.Events;

public class Store
{
    private readonly IEventHub _events;
    public required ObservableCollection<Camera> CamerasList { get; set; } = new();
    public Camera? SelectedCamera;

    public Store(IEventHub events)
    {
        _events = events;
    }

    public void SelectCamera(Camera? camera)
    {
        if (SelectedCamera == camera) return;

        SelectedCamera = camera;
        _events.Publish(new SelectedCameraChangedEvent(SelectedCamera));
    }
    public async Task<bool> StoreAddCamera(Camera camera)
    {
        var CameraName = CamerasList.FirstOrDefault(d => d.Name == camera.Name);
        if (CameraName != null) return false;

        CamerasList.Add(camera);

        await Helpers.SaveAsync(this);
        return true;
    }

    public async Task<bool> StoreUpdateCameraIp(Camera camera, string ip)
    {
        var CameraName = CamerasList.FirstOrDefault(d => d.Name == camera.Name);
        if (CameraName == null) return false;
        if (!Helpers.IsValidIP(ip)) return false;

        camera.Address = ip;

        await Helpers.SaveAsync(this);
        return true;
    }
}