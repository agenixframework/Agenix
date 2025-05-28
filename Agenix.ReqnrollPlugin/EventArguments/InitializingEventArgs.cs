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

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Represents event data for the initializing event in the Agenix Reqnroll plugin.
/// </summary>
public class InitializingEventArgs(Core.Agenix agenix)
{
    /// <summary>
    ///     Represents the core class of the Agenix system, acting as the central entry point for managing and interacting with
    ///     testing-related functionalities.
    /// </summary>
    /// <remarks>
    ///     The `Agenix` class is responsible for coordinating test execution, suite initialization, reporting mechanisms,
    ///     and listener integrations within the Agenix framework. It serves as a foundational component, encapsulating
    ///     configuration management, context provisioning, and test action execution.
    /// </remarks>
    /// <remarks>
    ///     This class supports adding and managing test-related listeners, reporters, and suites, ensuring a flexible and
    ///     extensible workflow for test execution processes. It also provides utility methods for versioning, resource
    ///     management, and lifecycle operations.
    /// </remarks>
    public Core.Agenix Agenix { get; set; } = agenix;
}
