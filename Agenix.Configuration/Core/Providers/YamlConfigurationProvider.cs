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

using Microsoft.Extensions.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Agenix.Configuration.Core.Providers;

/// <summary>
///     YAML configuration provider that reads configuration from YAML files.
/// </summary>
public class YamlConfigurationProvider : FileConfigurationProvider
{
    /// <summary>
    ///     Initializes a new instance of the YamlConfigurationProvider class.
    /// </summary>
    /// <param name="source">The YAML configuration source</param>
    public YamlConfigurationProvider(YamlConfigurationSource source) : base(source) { }

    /// <summary>
    ///     Loads the YAML data from the file.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    public override void Load(Stream stream)
    {
        var parser = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        using var reader = new StreamReader(stream);
        var yamlContent = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            return;
        }

        try
        {
            var yamlObject = parser.Deserialize(yamlContent);
            Data = FlattenYamlObject(yamlObject) ?? new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            throw new FormatException($"Could not parse the YAML file. Error: {ex.Message}", ex);
        }
    }

    private static Dictionary<string, string?> FlattenYamlObject(object? yamlObject, string prefix = "")
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        if (yamlObject == null)
        {
            return result;
        }

        return yamlObject switch
        {
            Dictionary<object, object> dictionary => FlattenDictionary(dictionary, prefix),
            List<object> list => FlattenList(list, prefix),
            _ => FlattenScalarValue(yamlObject, prefix)
        };
    }

    private static Dictionary<string, string?> FlattenDictionary(Dictionary<object, object> dictionary, string prefix)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in dictionary)
        {
            var key = kvp.Key?.ToString();
            if (key == null)
            {
                continue;
            }

            var fullKey = BuildFullKey(prefix, key);
            MergeNestedResults(result, kvp.Value, fullKey);
        }

        return result;
    }

    private static Dictionary<string, string?> FlattenList(List<object> list, string prefix)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < list.Count; i++)
        {
            var fullKey = BuildFullKey(prefix, i.ToString());
            MergeNestedResults(result, list[i], fullKey);
        }

        return result;
    }

    private static Dictionary<string, string?> FlattenScalarValue(object yamlObject, string prefix)
    {
        var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        result[prefix] = yamlObject.ToString();
        return result;
    }

    private static string BuildFullKey(string prefix, string key)
    {
        return string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";
    }

    private static void MergeNestedResults(Dictionary<string, string?> result, object? value, string fullKey)
    {
        if (IsComplexType(value))
        {
            var nested = FlattenYamlObject(value, fullKey);
            foreach (var nestedKvp in nested)
            {
                result[nestedKvp.Key] = nestedKvp.Value;
            }
        }
        else
        {
            result[fullKey] = value?.ToString();
        }
    }

    private static bool IsComplexType(object? value)
    {
        return value is Dictionary<object, object> or List<object>;
    }
}
