using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reqnroll;
using Reqnroll.BoDi;

namespace Agenix.ReqnrollPlugin;

/// Provides functionality to manage and retrieve binding instances of various types.
public class BindingInstanceProvider
{
    private readonly Dictionary<Type, object> _bindingInstances = new Dictionary<Type, object>();
    private IObjectContainer _objectContainer;

    // Set the object container reference
    public void SetObjectContainer(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    public void RegisterBinding<T>(T instance) where T : class
    {
        _bindingInstances[typeof(T)] = instance;
    }

    public T GetBinding<T>() where T : class
    {
        // First try our cache
        if (_bindingInstances.TryGetValue(typeof(T), out var instance))
        {
            return (T)instance;
        }
        
        // Then try to resolve from the container if available
        if (_objectContainer != null)
        {
            try
            {
                return _objectContainer.Resolve<T>();
            }
            catch
            {
                // Type is not registered in container
                return null;
            }
        }
        
        return null;
    }
    
    // Get all registered bindings from our dictionary
    public IReadOnlyDictionary<Type, object> GetAllCachedBindings()
    {
        return _bindingInstances;
    }
    
    // Find all types with the [Binding] attribute and resolve them from the container
    public IEnumerable<object> GetAllBindingInstances()
    {
        if (_objectContainer == null)
            return [];
            
        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        // Find all types with the [Binding] attribute
        var bindingTypes = assemblies
            .SelectMany(a => {
                try { 
                    return a.GetTypes(); 
                } 
                catch { 
                    return Type.EmptyTypes; 
                }
            })
            .Where(t => t.GetCustomAttribute<BindingAttribute>() != null);
            
        // Resolve each binding type from the container
        var result = new List<object>();
        foreach (var type in bindingTypes)
        {
            try
            {
                var instance = _objectContainer.Resolve(type);

                if (instance == null) continue;
                result.Add(instance);
                // Optionally cache the instance
                _bindingInstances[type] = instance;
            }
            catch
            {
                // Skip if can't resolve
            }
        }
        
        return result;
    }
    
    // Get all binding types from all loaded assemblies
    public IEnumerable<Type> GetAllBindingTypes()
    {
        // Get all loaded assemblies
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        // Find all types with the [Binding] attribute
        return assemblies
            .SelectMany(a => {
                try { 
                    return a.GetTypes(); 
                } 
                catch { 
                    return Type.EmptyTypes; 
                }
            })
            .Where(t => t.GetCustomAttribute<BindingAttribute>() != null);
    }
    
    // Get bindings from our cache by predicate
    public IEnumerable<object> GetBindingsWhere(Func<Type, object, bool> predicate)
    {
        return _bindingInstances
            .Where(kvp => predicate(kvp.Key, kvp.Value))
            .Select(kvp => kvp.Value);
    }
    
    // Clear our binding cache
    public void ClearBindings()
    {
        _bindingInstances.Clear();
    }
    
    // Count of cached bindings
    public int BindingCount => _bindingInstances.Count;
    
    // Directly access the ObjectContainer
    public IObjectContainer Container => _objectContainer;
}
