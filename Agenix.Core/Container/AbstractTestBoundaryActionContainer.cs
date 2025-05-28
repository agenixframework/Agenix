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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Represents an abstract container for boundary action tests, inheriting from <see cref="AbstractActionContainer" />.
/// </summary>
public abstract class AbstractTestBoundaryActionContainer : AbstractActionContainer
{
    private static readonly ILogger Log = LogManager.GetLogger("AbstractTestBoundaryActionContainer");
    private Dictionary<string, string> _env = new();
    private string _namePattern;
    private string _packageNamePattern;
    private Dictionary<string, string> _systemProperties = new();
    private List<string> _testGroups = [];

    /// <summary>
    ///     Determines whether the test should execute based on various criteria such as
    ///     test name, package name, included groups, environment properties, and system properties.
    /// </summary>
    /// <param name="testName">The name of the test to be executed.</param>
    /// <param name="packageName">The package name where the test is located.</param>
    /// <param name="includedGroups">An array of groups that the test belongs to.</param>
    /// <returns>Returns <c>true</c> if the test should execute; otherwise, <c>false</c>.</returns>
    public bool ShouldExecute(string testName, string packageName, string[] includedGroups)
    {
        const string baseErrorMessage =
            "Skip before test container because of {0} restrictions - do not execute container '{1}'";

        if (!string.IsNullOrEmpty(_packageNamePattern))
            if (!Regex.IsMatch(packageName, _packageNamePattern))
            {
                Log.LogWarning(baseErrorMessage, "test package", Name);
                return false;
            }

        if (!string.IsNullOrEmpty(_namePattern))
            if (!Regex.IsMatch(testName, SanitizePattern(_namePattern)))
            {
                Log.LogWarning(baseErrorMessage, "test name", Name);
                return false;
            }

        if (!CheckTestGroups(includedGroups))
        {
            Log.LogWarning(baseErrorMessage, "test groups", Name);
            return false;
        }

        if (_env.Any(envEntry => !ConfigurationManager.AppSettings[envEntry.Key]!.Contains(envEntry.Key) ||
                                 (!string.IsNullOrEmpty(envEntry.Value) && !ConfigurationManager
                                     .AppSettings[envEntry.Key].Equals(envEntry.Value))))
        {
            Log.LogWarning(baseErrorMessage, "env properties", Name);
            return false;
        }

        if (!_systemProperties
                .Where(systemProperty => Environment.GetEnvironmentVariable(systemProperty.Key) != null)
                .Any(systemProperty => !string.IsNullOrEmpty(systemProperty.Value) && !Environment
                    .GetEnvironmentVariable(systemProperty.Key)!.Equals(systemProperty.Value))) return true;
        Log.LogWarning(baseErrorMessage, "system properties", Name);
        return false;
    }

    /// <summary>
    ///     Modifies the given pattern by appending or prepending appropriate characters
    ///     to convert it into a valid regular expression pattern.
    /// </summary>
    /// <param name="pattern">The pattern to be sanitized.</param>
    /// <returns>Returns the sanitized version of the input pattern as a string.</returns>
    private string SanitizePattern(string pattern)
    {
        if (pattern.StartsWith('*')) return "." + pattern;

        if (pattern.EndsWith('*') && pattern[^2] != '.') return pattern[..^1] + ".*";

        return pattern;
    }

    /// <summary>
    ///     Determines whether the specified groups are included within the test groups.
    /// </summary>
    /// <param name="includedGroups">An array of groups that the test belongs to.</param>
    /// <returns>Returns <c>true</c> if any of the included groups match the test groups; otherwise, <c>false</c>.</returns>
    private bool CheckTestGroups(string[] includedGroups)
    {
        if (_testGroups.Count == 0) return true;

        return includedGroups != null && includedGroups.Any(includedGroup => _testGroups.Contains(includedGroup));
    }

    /// <summary>
    ///     Retrieves the list of test groups associated with the current test execution context.
    /// </summary>
    /// <returns>Returns a list of strings representing the test groups.</returns>
    public List<string> GetTestGroups()
    {
        return _testGroups;
    }

    /// <summary>
    ///     Sets the test groups associated with the current test execution context.
    /// </summary>
    /// <param name="groups">A list of strings representing the test groups to be set.</param>
    public void SetTestGroups(List<string> groups)
    {
        _testGroups = groups;
    }

    /// <summary>
    ///     Retrieves the name pattern used for test execution criteria.
    /// </summary>
    /// <returns>Returns the name pattern as a string.</returns>
    public string GetNamePattern()
    {
        return _namePattern;
    }

    /// <summary>
    ///     Sets the name pattern for the test container. This pattern can be used for identifying relevant tests
    ///     based on the name criteria during the execution.
    /// </summary>
    /// <param name="pattern">The name pattern to be set.</param>
    public void SetNamePattern(string pattern)
    {
        _namePattern = pattern;
    }

    /// <summary>
    ///     Retrieves the package name pattern used for test execution criteria.
    /// </summary>
    /// <returns>Returns the package name pattern as a string.</returns>
    public string GetPackageNamePattern()
    {
        return _packageNamePattern;
    }

    /// <summary>
    ///     Sets the pattern used to match package names during test execution.
    /// </summary>
    /// <param name="packageNamePattern">The pattern to be used for matching package names.</param>
    public void SetPackageNamePattern(string packageNamePattern)
    {
        _packageNamePattern = packageNamePattern;
    }

    /// <summary>
    ///     Retrieves the current environment properties for the test action container.
    /// </summary>
    /// <returns>A dictionary containing the environment properties.</returns>
    public Dictionary<string, string> GetEnv()
    {
        return _env;
    }

    /// <summary>
    ///     Sets the environment variables for the test boundary action container.
    /// </summary>
    /// <param name="env">A dictionary containing the environment variables to be set.</param>
    public void SetEnv(Dictionary<string, string> env)
    {
        _env = env;
    }

    /// <summary>
    ///     Retrieves the system properties that are set for the test container.
    /// </summary>
    /// <returns>
    ///     A dictionary containing the system properties where the key is the property name
    ///     and the value is the property value.
    /// </returns>
    public Dictionary<string, string> GetSystemProperties()
    {
        return _systemProperties;
    }

    /// <summary>
    ///     Sets the system properties for the test container.
    /// </summary>
    /// <param name="systemProperties">A dictionary containing key-value pairs of system properties to be set.</param>
    public void SetSystemProperties(Dictionary<string, string> systemProperties)
    {
        _systemProperties = systemProperties;
    }
}
