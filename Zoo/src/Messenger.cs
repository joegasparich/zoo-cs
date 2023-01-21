namespace Zoo;

public enum EventType {
    ElevationUpdated,
    PlaceSolid,
    AreaCreated,
    AreaUpdated,
    AreaRemoved,
    AnimalPlaced,
    AnimalRemoved,
}

public static class Messenger {
    // State
    private static readonly Dictionary<EventType, Dictionary<string, Action<object>>> listeners = new ();
    
    public static string On(EventType eventType, Action<object> callback) {
        var handle = Guid.NewGuid().ToString();
            
        if (!listeners.ContainsKey(eventType))
            listeners.Add(eventType, new Dictionary<string, Action<object>>());
        
        listeners[eventType].Add(handle, callback);
        
        return handle;
    }
    
    public static void Off(EventType eventType, string handle) {
        if (handle == null)
            return;
        if (!listeners.ContainsKey(eventType))
            return;
        if (!listeners[eventType].ContainsKey(handle))
            return;
        
        listeners[eventType].Remove(handle);
    }

    public static void Fire(EventType eventType) {
        Fire(eventType, null);
    }
    
    public static void Fire(EventType eventType, object data) {
        if (!listeners.ContainsKey(eventType))
            return;
        
        foreach (var callback in listeners[eventType].Values) {
            callback(data);
        }
    }
}