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

using Agenix.Api.IO;
using Agenix.Api.Util;

namespace Agenix.Api.Config;

/// <summary>
///     Implementation of <see cref="IVariableSource" /> that
///     resolves variable name against Java-style property file.
/// </summary>
/// <seealso cref="Api.Util.Properties" />
[Serializable]
public class PropertyFileVariableSource : IVariableSource
{
    private readonly object _objectMonitor = new();
    private bool _ignoreMissingResources;
    private IResource[] _locations;
    protected Properties Properties;

    /// <summary>
    ///     Gets or sets the locations of the property files
    ///     to read properties from.
    /// </summary>
    /// <value>
    ///     The locations of the property files
    ///     to read properties from.
    /// </value>
    public IResource[] Locations
    {
        get => _locations;
        set => _locations = value;
    }

    /// <summary>
    ///     Convenience property. Gets or sets a single location
    ///     to read properties from.
    /// </summary>
    /// <value>
    ///     A location to read properties from.
    /// </value>
    public IResource Location
    {
        set => _locations = [value];
    }

    /// <summary>
    ///     Sets a value indicating whether to ignore resource locations that do not exist.  This will call
    ///     the <see cref="IResource" /> Exists property.
    /// </summary>
    /// <value>
    ///     <c>true</c> if one should ignore missing resources; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreMissingResources
    {
        set => _ignoreMissingResources = value;
    }

    /// <summary>
    ///     Before requesting a variable resolution, a client should
    ///     ask whether the source can resolve a particular variable name.
    /// </summary>
    /// <param name="name">the name of the variable to resolve</param>
    /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
    public bool CanResolveVariable(string name)
    {
        lock (_objectMonitor)
        {
            if (Properties != null) return Properties.Contains(name);
            Properties = new Properties();
            InitProperties();
            return Properties.Contains(name);
        }
    }

    /// <summary>
    ///     Resolves variable value for the specified variable name.
    /// </summary>
    /// <param name="name">
    ///     The name of the variable to resolve.
    /// </param>
    /// <returns>
    ///     The variable value if able to resolve, <c>null</c> otherwise.
    /// </returns>
    public string ResolveVariable(string name)
    {
        lock (_objectMonitor)
        {
            if (Properties != null) return Properties.GetProperty(name);
            Properties = new Properties();
            InitProperties();
            return Properties.GetProperty(name);
        }
    }

    /// <summary>
    ///     Initializes properties based on the specified
    ///     property file locations.
    /// </summary>
    protected virtual void InitProperties()
    {
        foreach (var location in _locations)
        {
            var exists = location.Exists;
            if (!exists && _ignoreMissingResources) continue;

            using var input = location.InputStream;
            lock (_objectMonitor)
            {
                Properties.Load(input);
            }
        }
    }
}
