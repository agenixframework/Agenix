#region Imports

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
            lock (AssemblyResolverLock)
            {
                AppDomain.CurrentDomain.AssemblyResolve += LoadedAssemblyResolver;
                _assemblyResolverRegistered = true;
            }
    }

    private static Assembly LoadedAssemblyResolver(object sender, ResolveEventArgs args)
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        return loadedAssemblies.FirstOrDefault(assembly => assembly.FullName == args.Name);
    }
}
