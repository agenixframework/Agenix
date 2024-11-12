using System;
using System.Linq;
using System.Reflection;

namespace Agenix.Core.Util;

/// <summary>
///     Provides utility methods for working with reflection, including
///     iterating over classes, fields, and methods and finding or invoking
///     them.
/// </summary>
public static class ReflectionHelper
{
    /// Callback delegate for operations on class types.
    /// <param name="clazz">The Type object representing the class to operate on.</param>
    public delegate void ClassCallback(Type clazz);

    /// Callback delegate for operations on field types.
    /// <param name="field">The FieldInfo object representing the field to operate on.</param>
    public delegate void FieldCallback(FieldInfo field);

    /// Callback delegate for operations on method types.
    /// <param name="method">The MethodInfo object representing the method to operate on.</param>
    public delegate void MethodCallback(MethodInfo method);

    /// Executes a callback for each nested class in the given class type.
    /// <param name="clazz">The Type object representing the class on which to search for nested classes.</param>
    /// <param name="cc">The callback to invoke for each nested class found.</param>
    public static void DoWithClasses(Type clazz, ClassCallback cc)
    {
        var classes = clazz.GetNestedTypes();
        foreach (var aClazz in classes) cc.Invoke(aClazz);
    }

    /// Executes a callback for each field in the given class type.
    /// <param name="clazz">The Type object representing the class on which to search for fields.</param>
    /// <param name="fc">The callback to invoke for each field found.</param>
    public static void DoWithFields(Type clazz, FieldCallback fc)
    {
        var fields = clazz.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields) fc.Invoke(field);
    }

    /// Executes a callback for each method in the given class type.
    /// <param name="clazz">The Type object representing the class on which to search for methods.</param>
    /// <param name="mc">The callback to invoke for each method found.</param>
    public static void DoWithMethods(Type clazz, MethodCallback mc)
    {
        var methods = clazz.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            if (method.IsSpecialName) continue;
            mc.Invoke(method);
        }
    }

    /// Finds a field on a given class type that matches the specified name.
    /// <param name="clazz">The Type object representing the class on which to search for the field.</param>
    /// <param name="name">The name of the field to search for.</param>
    /// <return>The FieldInfo object representing the found field, or null if no matching field is found.</return>
    public static FieldInfo FindField(Type clazz, string name)
    {
        var fields = clazz.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return fields.FirstOrDefault(f => f.Name == name);
    }

    /// Finds a method on a given class type that matches the specified name and parameter types.
    /// <param name="clazz">The Type object representing the class on which to search for the method.</param>
    /// <param name="name">The name of the method to search for.</param>
    /// <param name="paramTypes">An array of Type objects representing the parameter types of the method signature.</param>
    /// <return>The MethodInfo object representing the found method, or null if no matching method is found.</return>
    public static MethodInfo FindMethod(Type clazz, string name, params Type[] paramTypes)
    {
        var methods = clazz.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        return methods.FirstOrDefault(m =>
            m.Name == name && m.GetParameters().Select(p => p.ParameterType).SequenceEqual(paramTypes));
    }

    /// Invokes a specified method on a given object instance with the provided arguments.
    /// <param name="method">The MethodInfo object representing the method to invoke.</param>
    /// <param name="target">The object instance on which to invoke the method.</param>
    /// <param name="args">An array of objects to pass as arguments to the method.</param>
    /// <return>The result of the invoked method.</return>
    public static object InvokeMethod(MethodInfo method, object target, params object[] args)
    {
        try
        {
            return method.Invoke(target, args);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to invoke method", e);
        }
    }

    /// Sets the value of a specified field for a given object instance.
    /// <param name="f">The FieldInfo object representing the field to be set.</param>
    /// <param name="instance">The object instance for which the field value is to be changed.</param>
    /// <param name="value">The new value to assign to the field.</param>
    public static void SetField(FieldInfo f, object instance, object value)
    {
        if (!f.IsPublic) f.SetValue(instance, value);
    }

    /// Retrieves the value of a specified field from a given object instance.
    /// <param name="f">The FieldInfo object representing the field to be retrieved.</param>
    /// <param name="instance">The object instance from which the field value is to be retrieved.</param>
    /// <return>The value of the field if it is not public, otherwise null.</return>
    /// /
    public static object GetField(FieldInfo f, object instance)
    {
        return !f.IsPublic ? f.GetValue(instance) : null;
    }
}