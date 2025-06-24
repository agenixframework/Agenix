#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Api.TypeResolution;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Spi;

/// <summary>
///     Provides functionality for resolving resource paths into types and properties using a predefined
///     or custom resource base path. This class is specifically designed to interpret resource paths
///     and map them to corresponding types or properties as needed.
/// </summary>
public class ResourcePathTypeResolver : ITypeResolver
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ResourcePathTypeResolver));

    /// <summary>
    ///     Read resource from assembly and load content as properties.
    ///     The properties found on the assembly will be cached.
    /// </summary>
    private readonly ConcurrentDictionary<string, Properties> _resourceProperties = new();

    /// <summary>
    ///     Cached specific type names as resolved from assembly.
    /// </summary>
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _typeCache = new();

    /// <summary>
    ///     Default constructor using an Extension resource base path.
    /// </summary>
    public ResourcePathTypeResolver() : this("Extension")
    {
    }

    /// <summary>
    ///     Default constructor initializes with given resource path.
    /// </summary>
    public ResourcePathTypeResolver(string resourceBasePath)
    {
        ResourceBasePath = resourceBasePath.EndsWith('/')
            ? resourceBasePath[..^1]
            : resourceBasePath;
    }

    /// <summary>
    ///     Base path for resources.
    /// </summary>
    public string ResourceBasePath { get; }

    /// <summary>
    ///     Resolves the value of a specific property from the given resource path.
    /// </summary>
    /// <param name="resourcePath">The path to the resource containing the properties.</param>
    /// <param name="property">The name of the property to resolve.</param>
    /// <returns>The value of the specified property as a string.</returns>
    public string ResolveProperty(string resourcePath, string property)
    {
        return ReadAsProperties(resourcePath).GetProperty(property);
    }

    /// <summary>
    ///     Resolves a type using the specified resource path and property, with optional initialization arguments.
    /// </summary>
    /// <typeparam name="T">The type to be resolved.</typeparam>
    /// <param name="resourcePath">The path of the resource containing the type definition.</param>
    /// <param name="property">The property name used to resolve the type.</param>
    /// <param name="initargs">Optional initialization arguments for the resolved type.</param>
    /// <returns>An instance of the resolved type <typeparamref name="T" />.</returns>
    public T Resolve<T>(string resourcePath, string property, params object[] initargs)
    {
        var cacheKey = ToCacheKey(resourcePath, property, "NO_KEY_PROPERTY");

        if (!_typeCache.TryGetValue(cacheKey, out var map))
        {
            map = new Dictionary<string, string> { { cacheKey, ResolveProperty(resourcePath, property) } };
            _typeCache[cacheKey] = map;
        }

        var type = new TypeResolver().Resolve(map[cacheKey]);
        return (T)ObjectUtils.InstantiateType(ObjectUtils.GetZeroArgConstructorInfo(type), initargs);
    }

    /// <summary>
    ///     Resolves all resources specified by the given parameters into a dictionary.
    /// </summary>
    /// <typeparam name="T">The type of instances to resolve from the resources.</typeparam>
    /// <param name="resourcePath">The path to the resource containing mappings.</param>
    /// <param name="property">The property indicating the type information for the resources.</param>
    /// <param name="keyProperty">The property used as a key for identifying resources in the dictionary.</param>
    /// <returns>A dictionary containing resource keys mapped to resolved instances of type <typeparamref name="T" />.</returns>
    public IDictionary<string, T> ResolveAll<T>(string resourcePath, string property, string keyProperty)
    {
        if (!_typeCache.TryGetValue(ToCacheKey(resourcePath, property, keyProperty), out var typeLookup))
        {
            typeLookup = GetPropertyMappingsFromResources(resourcePath, property, keyProperty);
            _typeCache[ToCacheKey(resourcePath, property, keyProperty)] = typeLookup;
        }

        var resources = new Dictionary<string, T>();

        foreach (var kvp in typeLookup)
        {
            var type = new TypeResolver().Resolve(typeLookup[kvp.Key]);
            resources[kvp.Key] = (T)ObjectUtils.InstantiateType(type);
        }

        return resources;
    }

    /// <summary>
    ///     Load default type information from given resource path property file and create new instance of given type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="initargs"></param>
    /// <returns></returns>
    public T Resolve<T>(string resourcePath, params object[] initargs)
    {
        return Resolve<T>(resourcePath, ITypeResolver.DEFAULT_TYPE_PROPERTY, initargs);
    }

    /// <summary>
    ///     Retrieves property mappings from resources by scanning and extracting relevant data.
    /// </summary>
    /// <param name="path">The path to the resource directory or file.</param>
    /// <param name="property">The specific property to extract from the resources.</param>
    /// <param name="keyProperty">The key property used to index the resulting dictionary.</param>
    /// <returns>A dictionary containing mappings of keys to properties extracted from the resources.</returns>
    private Dictionary<string, string> GetPropertyMappingsFromResources(string path, string property,
        string keyProperty)
    {
        var fullPath = GetFullResourcePath(path);
        Dictionary<string, string> typeLookup = new();

        try
        {
            var matchingProperties = LoadAllMatchingProperties(fullPath);

            foreach (var resource in matchingProperties)
            {
                var resourceName = resource.Key[(resource.Key.LastIndexOf('/') + 1)..];
                var resourceProps = resource.Value;
                var resourceType = resourceProps.GetProperty(property);

                if (property.Equals(ITypeResolver.TYPE_PROPERTY_WILDCARD))
                {
                    foreach (DictionaryEntry prop in resourceProps)
                    {
                        typeLookup.Add(resourceName + "." + prop.Key, prop.Value.ToString());
                    }
                }
                else
                {
                    typeLookup.Add(
                        keyProperty != null
                            ? ResolveProperty(fullPath + "/" + resourceName, keyProperty)
                            : resourceName, resourceType);
                }
            }
        }
        catch (IOException e)
        {
            Log.LogWarning(e, "Failed to resolve resources in '{}' => ", fullPath);
        }

        return typeLookup;
    }

    /// <summary>
    ///     Generates a cache key based on the specified path, property, and key property inputs.
    /// </summary>
    /// <param name="path">The resource path used in the key generation.</param>
    /// <param name="property">The property name used in the key generation.</param>
    /// <param name="keyProperty">The key property used in the key generation.</param>
    /// <returns>A string representing the generated cache key.</returns>
    private static string ToCacheKey(string path, string property, string keyProperty)
    {
        return path + "$$$" + property + "$$$" + keyProperty;
    }

    /// <summary>
    ///     Finds all manifest resources across loaded assemblies that match a specified path pattern.
    /// </summary>
    /// <param name="searchPattern">Pattern to search for in resource names (e.g., "Extension.mocks.foo")</param>
    /// <returns>Dictionary of matching resources grouped by assembly</returns>
    public static Dictionary<Assembly, List<string>> FindAllMatchingResources(string searchPattern)
    {
        var result = new Dictionary<Assembly, List<string>>();

        // Normalize the search pattern to handle different separator formats
        var normalizedPattern = searchPattern.Replace('/', '.').Replace('\\', '.');

        // Track already loaded assemblies by name to avoid duplicates
        var loadedAssemblyNames = new HashSet<string>(
            AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetName().Name)!
        );

        // First load assemblies from the current directory
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        foreach (var file in Directory.GetFiles(baseDirectory, "*.dll"))
        {
            try
            {
                var assemblyName = Path.GetFileNameWithoutExtension(file);

                // Skip if already loaded
                if (loadedAssemblyNames.Contains(assemblyName))
                {
                    continue;
                }

                // Try to load the assembly
                var assembly = Assembly.LoadFrom(file);

                // Add to domain if successfully loaded
                loadedAssemblyNames.Add(assemblyName);
            }
            catch (Exception ex)
            {
                // Just log and continue if we can't load an assembly
                Log.LogWarning("Failed to load assembly from {File}: {ExMessage}", file, ex.Message);
            }
        }

        // Now get all loaded assemblies including those we just loaded
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Now process all assemblies to find matching resources
        foreach (var assembly in allAssemblies)
        {
            try
            {
                // Skip dynamic assemblies that don't have a location
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                {
                    continue;
                }

                // Get all manifest resources from this assembly
                var resources = assembly.GetManifestResourceNames();

                // Find resources that match our pattern
                var matchingResources = resources
                    .Where(resource => resource.Contains(normalizedPattern))
                    .ToList();

                // If we found matching resources, add them to our result dictionary
                if (matchingResources.Count > 0)
                {
                    result[assembly] = matchingResources;
                }
            }
            catch (Exception ex)
            {
                // Some assemblies might throw exceptions when accessing their resources
                Log.LogWarning(
                    "Error accessing resources in assembly {AssemblyFullName}: {ExMessage}", assembly.FullName, ex
                        .Message);
            }
        }

        return result;
    }

    /// <summary>
    ///     Converts a manifest resource name to Agenix AssemblyResource URI format
    /// </summary>
    /// <param name="assembly">The assembly containing the resource</param>
    /// <param name="resourceName">The full resource name</param>
    /// <returns>Agenix compatible resource URI</returns>
    public static string ConvertToAgenixResourceUri(Assembly assembly, string resourceName)
    {
        var assemblyName = assembly.GetName().Name;
        var resourcePath = resourceName;

        // Extract the filename (everything after the last dot)
        var lastDotIndex = resourcePath.LastIndexOf('.');
        if (lastDotIndex < 0)
        // No dots found, use the whole path as namespace and leave filename empty
        {
            return $"assembly://{assemblyName}/{resourcePath}/";
        }

        var namespacePrefix = resourcePath[..lastDotIndex];
        var filename = resourcePath[(lastDotIndex + 1)..];

        return $"assembly://{assemblyName}/{namespacePrefix}/{filename}";
    }

    /// <summary>
    ///     Gets all properties from matching resources across all loaded assemblies
    /// </summary>
    /// <param name="searchPattern">Pattern to search for in resource names</param>
    /// <returns>Dictionary of property collections by resource URI</returns>
    public static Dictionary<string, Properties> LoadAllMatchingProperties(string searchPattern)
    {
        var result = new Dictionary<string, Properties>();
        var matchingResources = FindAllMatchingResources(searchPattern);

        foreach (var assemblyEntry in matchingResources)
        {
            var assembly = assemblyEntry.Key;
            var resources = assemblyEntry.Value;

            foreach (var resourceName in resources)
            {
                try
                {
                    // Convert to Agenix resource URI
                    var resourceUri = ConvertToAgenixResourceUri(assembly, resourceName);

                    // Create AssemblyResource and load properties
                    var resource = new AssemblyResource(resourceUri);

                    var props = new Properties();
                    using (var stream = resource.InputStream)
                    {
                        props.Load(stream);
                    }

                    result[resourceUri] = props;
                }
                catch (Exception ex)
                {
                    Log.LogWarning(
                        "Error loading resource {ResourceName} from {Name}: {ExMessage}", resourceName, assembly
                            .GetName().Name, ex.Message);
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Reads the specified resource and retrieves its content as a
    ///     <see cref="Properties" /> object, caching the result for subsequent calls.
    /// </summary>
    /// <param name="resourcePath">
    ///     A string representing the path to the resource file to be read.
    /// </param>
    /// <returns>
    ///     A <see cref="Properties" /> object containing the key-value pairs loaded
    ///     from the specified resource file.
    /// </returns>
    private Properties ReadAsProperties(string resourcePath)
    {
        if (_resourceProperties.TryGetValue(resourcePath, out var properties))
        {
            return properties;
        }

        var path = GetFullResourcePath(resourcePath);
        var matchingProperties = LoadAllMatchingProperties(path);

        // Create a new Properties instance to hold the merged properties
        properties = new Properties();

        // Iterate through all matching properties and merge them
        foreach (var entry in from prop in matchingProperties.Values
                              where prop != null
                              from DictionaryEntry entry in prop
                              select entry)
        {
            properties.SetProperty(entry.Key.ToString(), entry.Value?.ToString());
        }

        _resourceProperties[resourcePath] = properties;
        return properties;
    }

    /// <summary>
    ///     Combine base resource path and given resource path to the proper full resource path.
    /// </summary>
    private string GetFullResourcePath(string resourcePath)
    {
        if (string.IsNullOrEmpty(resourcePath))
        {
            return ResourceBasePath;
        }

        return !resourcePath.StartsWith(ResourceBasePath) ? $"{ResourceBasePath}/{resourcePath}" : resourcePath;
    }

    /// <summary>
    ///     Load all resources and create new instance of given type. The type information is read by
    ///     the given property in the resource file. The keys in the resulting map represent the resource file names.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public IDictionary<string, T> ResolveAll<T>()
    {
        return ResolveAll<T>("");
    }

    /// <summary>
    ///     Load all resources in a given resource path and create new instance of a given type. The given property reads the
    ///     type information
    ///     in the resource file. The keys in the resulting map represent the resource file names.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <returns></returns>
    public IDictionary<string, T> ResolveAll<T>(string resourcePath)
    {
        return ResolveAll<T>(resourcePath, ITypeResolver.DEFAULT_TYPE_PROPERTY);
    }

    /// <summary>
    ///     Load all resources in a given resource path and create new instance of a given type. The given property reads the
    ///     type information
    ///     in the resource file. The keys in the resulting map represent the resource file names.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    public IDictionary<string, T> ResolveAll<T>(string resourcePath, string property)
    {
        return ResolveAll<T>(resourcePath, property, null);
    }
}
