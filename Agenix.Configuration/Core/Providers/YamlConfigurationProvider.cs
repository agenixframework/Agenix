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

namespace Agenix.Configuration.Core.Providers;

/// <summary>
/// YAML configuration provider that reads configuration from YAML files.
/// </summary>
public class YamlConfigurationProvider : FileConfigurationProvider
{
    /// <summary>
    /// Initializes a new instance of the YamlConfigurationProvider class.
    /// </summary>
    /// <param name="source">The YAML configuration source</param>
    public YamlConfigurationProvider(YamlConfigurationSource source) : base(source) { }

    /// <summary>
    /// Loads the YAML data from the file.
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    public override void Load(Stream stream)
    {
        var parser = new DeserializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
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
            return result;

        if (yamlObject is Dictionary<object, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var key = kvp.Key?.ToString();
                if (key == null) continue;

                var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}:{key}";

                if (kvp.Value is Dictionary<object, object> || kvp.Value is List<object>)
                {
                    var nested = FlattenYamlObject(kvp.Value, fullKey);
                    foreach (var nestedKvp in nested)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else
                {
                    result[fullKey] = kvp.Value?.ToString();
                }
            }
        }
        else if (yamlObject is List<object> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var fullKey = string.IsNullOrEmpty(prefix) ? i.ToString() : $"{prefix}:{i}";

                if (list[i] is Dictionary<object, object> || list[i] is List<object>)
                {
                    var nested = FlattenYamlObject(list[i], fullKey);
                    foreach (var nestedKvp in nested)
                    {
                        result[nestedKvp.Key] = nestedKvp.Value;
                    }
                }
                else
                {
                    result[fullKey] = list[i]?.ToString();
                }
            }
        }
        else
        {
            result[prefix] = yamlObject.ToString();
        }

        return result;
    }
}
