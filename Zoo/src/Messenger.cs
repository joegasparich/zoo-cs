namespace Zoo;

public enum EventType {
    ElevationUpdated,
    PlaceSolid
}

public static class Messenger {
    private static readonly Dictionary<EventType, List<Action<object>>> listeners = new ();
    
    public static string On(EventType eventType, Action<object> callback) {
        var handle = Guid.NewGuid().ToString();
            
        if (!listeners.ContainsKey(eventType)) {
            listeners.Add(eventType, new List<Action<object>>());
        }
        
        listeners[eventType].Add(callback);
        
        return handle;
    }
    
    public static void Off(EventType eventType, string handle) {
        if (!listeners.ContainsKey(eventType)) {
            return;
        }
        
        listeners[eventType].RemoveAll(callback => callback.Method.Name == handle);
    }

    public static void Fire(EventType eventType) {
        Fire(eventType, null);
    }
    
    public static void Fire(EventType eventType, object data) {
        if (!listeners.ContainsKey(eventType)) {
            return;
        }
        
        foreach (var callback in listeners[eventType]) {
            callback(data);
        }
    }
}