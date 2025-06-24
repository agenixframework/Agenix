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

using System.Reflection;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Api;

/// <summary>
///     Provides version management for the Agenix application.
/// </summary>
public static class AgenixVersion
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixVersion));

    static AgenixVersion()
    {
        try
        {
            // Using reflection to get version information
            var assembly = Assembly.GetExecutingAssembly();

            // Retrieve the AssemblyVersion
            var versionAttribute = assembly.GetName().Version;
            Version = versionAttribute?.ToString() ?? "Version not found";

            // Optionally, retrieve the AssemblyInformationalVersion if you wish
            var infoVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var infoVersion = infoVersionAttribute?.InformationalVersion ?? "Info version not set";

            if (string.IsNullOrWhiteSpace(infoVersion) || infoVersion == Version)
            {
                Log.LogWarning("Agenix version has not been updated from the default yet");
            }
        }
        catch (Exception e)
        {
            Log.LogWarning(e, "Unable to read Agenix version information");
            Version = "";
        }
    }

    public static string Version { get; }
}
