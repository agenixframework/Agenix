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

#region Imports

using System.Reflection;

#endregion

namespace Agenix.Api.Util;

/// <summary>
///     Utility class containing miscellaneous system-level functionality.
/// </summary>
public sealed class SystemUtils
{
    private static bool _assemblyResolverRegistered;
    private static readonly object AssemblyResolverLock;

    static SystemUtils()
    {
        MonoRuntime = Type.GetType("Mono.Runtime") != null;
        AssemblyResolverLock = new object();
    }


    /// <summary>
    ///     Returns true if running on Mono
    /// </summary>
    /// <remarks>Tests for the presence of the type Mono.Runtime</remarks>
    public static bool MonoRuntime { get; }

    /// <summary>
    ///     Gets the thread id for the current thread. Use thread name is available,
    ///     otherwise use CurrentThread.GetHashCode() for .NET 1.0/1.1 and
    ///     CurrentThread.ManagedThreadId otherwise.
    /// </summary>
    /// <value>The thread id.</value>
    public static string ThreadId
    {
        get
        {
            var name = Thread.CurrentThread.Name;
            return StringUtils.HasText(name) ? name : Thread.CurrentThread.ManagedThreadId.ToString();
        }
    }

    /// <summary>
    ///     Registers assembly resolver that iterates over the
    ///     assemblies loaded into the current <see cref="AppDomain" />
    ///     in order to find an assembly that cannot be resolved.
    /// </summary>
    /// <remarks>
    ///     This method has to be called if you need to serialize dynamically
    ///     generated types in transient assemblies, such as Spring AOP proxies,
    ///     because standard .NET serialization engine always tries to load
    ///     assembly from the disk.
    /// </remarks>
    public static void RegisterLoadedAssemblyResolver()
    {
        if (!_assemblyResolverRegistered)
        {
            lock (AssemblyResolverLock)
            {
                AppDomain.CurrentDomain.AssemblyResolve += LoadedAssemblyResolver;
                _assemblyResolverRegistered = true;
            }
        }
    }

    private static Assembly LoadedAssemblyResolver(object sender, ResolveEventArgs args)
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        return loadedAssemblies.FirstOrDefault(assembly => assembly.FullName == args.Name);
    }
}
