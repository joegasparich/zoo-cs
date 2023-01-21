﻿namespace Zoo.util; 

public static class EnumerableExtension {
    public static bool NullOrEmpty<T>( this IEnumerable<T>? collection ) {
        return collection == null || !collection.Any();
    }
    
    public static T? RandomElement<T>(this IEnumerable<T> collection) {
        if (collection.NullOrEmpty()) return default;
        
        return collection.ElementAt(Rand.Int(0, collection.Count()));
    }
}