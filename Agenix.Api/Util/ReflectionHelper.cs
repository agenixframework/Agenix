#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Reflection;

namespace Agenix.Api.Util;

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
        foreach (var aClazz in classes)
        {
            cc.Invoke(aClazz);
        }
    }

    /// Executes a callback for each field in the given class type.
    /// <param name="clazz">The Type object representing the class on which to search for fields.</param>
    /// <param name="fc">The callback to invoke for each field found.</param>
    public static void DoWithFields(Type clazz, FieldCallback fc)
    {
        var fields = clazz.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            fc.Invoke(field);
        }
    }

    /// Executes a callback for each method in the given class type.
    /// <param name="clazz">The Type object representing the class on which to search for methods.</param>
    /// <param name="mc">The callback to invoke for each method found.</param>
    public static void DoWithMethods(Type clazz, MethodCallback mc)
    {
        var methods = clazz.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        foreach (var method in methods)
        {
            if (method.IsSpecialName)
            {
                continue;
            }

            mc.Invoke(method);
        }
    }

    /// Finds a field on a given class type that matches the specified name.
    /// <param name="clazz">The Type object representing the class on which to search for the field.</param>
    /// <param name="name">The name of the field to search for.</param>
    /// <return>The FieldInfo object representing the found field, or null if no matching field is found.</return>
    public static FieldInfo FindField(Type clazz, string name)
    {
        // Traverse the type hierarchy
        while (clazz != null)
        {
            var fields = clazz.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            var field = fields.FirstOrDefault(f =>
                string.Equals(f.Name, $"<{name}>k__BackingField", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
            if (field != null)
            {
                return field;
            }

            clazz = clazz.BaseType; // Move to the base type
        }

        return null; // Field not found in the hierarchy
    }

    /// Finds a method on a given class type that matches the specified name and parameter types.
    /// <param name="clazz">The Type object representing the class on which to search for the method.</param>
    /// <param name="name">The name of the method to search for.</param>
    /// <param name="paramTypes">An array of Type objects representing the parameter types of the method signature.</param>
    /// <return>The MethodInfo object representing the found method, or null if no matching method is found.</return>
    public static MethodInfo FindMethod(Type clazz, string name, params Type[] paramTypes)
    {
        while (clazz != null)
        {
            var methods = clazz.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                           BindingFlags.Static);

            // Possible method name variations (original, camelCase, and snake_case)
            var possibleNames = new[]
            {
                name, // Original name
                "Set" + char.ToUpper(name[0]) + name.Substring(1), // SetCamelCase
                "set_" + char.ToLower(name[0]) + name.Substring(1) // set_snake_case
            };

            // Iterating through all possible names
            foreach (var possibleName in possibleNames)
                foreach (var method in methods.Where(m =>
                             string.Equals(m.Name, possibleName, StringComparison.OrdinalIgnoreCase)))
                // Handle generic methods
                {
                    if (method.IsGenericMethodDefinition)
                    {
                        var genericArguments = method.GetGenericArguments();
                        if (genericArguments.Length == paramTypes.Length)
                        {
                            try
                            {
                                // Construct the generic method to match the parameter types
                                var constructedMethod = method.MakeGenericMethod(paramTypes);

                                if (AreParametersCompatible(constructedMethod.GetParameters(), paramTypes))
                                {
                                    return constructedMethod;
                                }
                            }
                            catch
                            {
                                // If unable to construct the method, continue searching
                            }
                        }
                    }
                    else
                    {
                        // Handle non-generic methods
                        if (AreParametersCompatible(method.GetParameters(), paramTypes))
                        {
                            return method;
                        }
                    }
                }

            // Move up the type hierarchy
            clazz = clazz.BaseType;
        }

        return null; // Method not found in the hierarchy
    }

    /// <summary>
    ///     Determines if the provided parameter types are compatible with the method's parameters.
    /// </summary>
    private static bool AreParametersCompatible(ParameterInfo[] parameters, Type[] paramTypes)
    {
        if (parameters.Length == 0 && paramTypes.Length == 0)
        {
            return true;
        }

        // Handle `params` parameter matching
        if (parameters.Length > 0 && parameters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Any())
        {
            // Split into normal parameters and params parameter
            var fixedParams = parameters.Take(parameters.Length - 1).ToArray();
            var paramsParam = parameters.Last();

            if (paramTypes.Length < fixedParams.Length)
            {
                return false;
            }

            // Check fixed parameters
            for (var i = 0; i < fixedParams.Length; i++)
            {
                if (!fixedParams[i].ParameterType.IsAssignableFrom(paramTypes[i]))
                {
                    return false;
                }
            }

            // Check params parameter
            var paramsType = paramsParam.ParameterType.GetElementType();
            for (var i = fixedParams.Length; i < paramTypes.Length; i++)
            {
                if (!paramsType!.IsAssignableFrom(paramTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Standard parameter matching
        if (parameters.Length != paramTypes.Length)
        {
            return false;
        }

        for (var i = 0; i < parameters.Length; i++)
        {
            if (!parameters[i].ParameterType.IsAssignableFrom(paramTypes[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AreParametersCompatible2(ParameterInfo[] parameters, Type[] paramTypes,
        MethodInfo method = null)
    {
        if (parameters.Length == 0 && paramTypes.Length == 0)
        {
            return true;
        }

        if (method != null && method.IsGenericMethodDefinition)
        {
            // Match generic parameters with their constraints
            var genericArguments = method.GetGenericArguments();
            for (var i = 0; i < genericArguments.Length; i++)
            {
                var constraints = genericArguments[i].GetGenericParameterConstraints();

                // Ensure the provided type matches the generic constraints
                if (constraints.Length > 0 &&
                    !constraints.All(constraint => constraint.IsAssignableFrom(paramTypes[i])))
                {
                    return false;
                }
            }
        }

        // Handle normal parameter validation as before
        return CheckParameterTypes(parameters, paramTypes);
    }

    private static bool CheckParameterTypes(ParameterInfo[] parameters, Type[] paramTypes)
    {
        // Handle `params` parameter matching
        if (parameters.Length > 0 &&
            parameters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Any())
        {
            var fixedParams = parameters.Take(parameters.Length - 1).ToArray();
            var paramsParam = parameters.Last();

            if (paramTypes.Length < fixedParams.Length)
            {
                return false;
            }

            // Check fixed parameters
            if (fixedParams.Where((t, i) => !t.ParameterType.IsAssignableFrom(paramTypes[i])).Any())
            {
                return false;
            }

            // Check params parameter
            var paramsType = paramsParam.ParameterType.GetElementType();
            for (var i = fixedParams.Length; i < paramTypes.Length; i++)
            {
                if (!paramsType!.IsAssignableFrom(paramTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Standard parameter matching
        if (parameters.Length != paramTypes.Length)
        {
            return false;
        }

        return !parameters.Where((t, i) => !t.ParameterType.IsAssignableFrom(paramTypes[i])).Any();
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

    /// <summary>
    ///     Sets a field value using reflection, handling accessibility modifiers.
    /// </summary>
    /// <param name="field">The field to set</param>
    /// <param name="instance">The object instance on which to set the field</param>
    /// <param name="value">The value to set</param>
    public static void SetField(FieldInfo field, object instance, object value)
    {
        try
        {
            if (!field.IsPublic)
            {
                field = field.DeclaringType.GetField(field.Name,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            }

            field?.SetValue(instance, value);
        }
        catch (Exception ex)
        {
            throw new NotSupportedException(
                $"Cannot set value of type: {value?.GetType()} into field: {field}", ex);
        }
    }


    /// <summary>
    ///     Gets a field value using reflection, handling accessibility modifiers.
    /// </summary>
    /// <param name="field">The field to get</param>
    /// <param name="instance">The object instance from which to get the field</param>
    /// <returns>The field value or null if access fails</returns>
    public static object GetField(FieldInfo field, object instance)
    {
        try
        {
            if (!field.IsPublic || !field.DeclaringType.IsPublic || field.IsInitOnly)
            {
                // Get a more accessible version of the field
                var flags = BindingFlags.Public | BindingFlags.NonPublic;
                flags |= field.IsStatic ? BindingFlags.Static : BindingFlags.Instance;

                field = field.DeclaringType.GetField(field.Name, flags);
            }
        }
        catch (Exception)
        {
            // Ignore any reflection exceptions when trying to make field accessible
        }

        try
        {
            return field?.GetValue(instance);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    ///     Checks, if the specified type is a nullable
    /// </summary>
    public static bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }
}
