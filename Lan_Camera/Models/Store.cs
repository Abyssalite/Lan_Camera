
using Custom_EventHub;

public class Store
{
    private readonly IEventHub _events;

    public Store(IEventHub events)
    {
        _events = events;
    }

}