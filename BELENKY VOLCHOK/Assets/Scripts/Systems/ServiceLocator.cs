using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();
    
    public static void Assign<T>(T service) where T : class
    {
        var type = typeof(T);
        if (!services.ContainsKey(type))
            services[type] = service;
    }
    
    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        if (services.TryGetValue(type, out var service))
            return service as T;
        return null;
    }
    
    public static void Remove<T>() where T : class
    {
        var type = typeof(T);
        if (services.ContainsKey(type))
            services.Remove(type);
    }
    
    public static void ClearAll()
    {
        services.Clear();
    }
}