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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Core.Util;

namespace Agenix.Core.Repository;

/// <summary>
///     Provides a base implementation for a repository, including functionality for
///     initialization, managing locations, and handling repository resources.
/// </summary>
public abstract class BaseRepository(string name) : INamed, InitializingPhase
{
    /// <summary>
    ///     Gets the name of the repository.
    /// </summary>
    /// <remarks>
    ///     The name is assigned during the construction of the repository
    ///     and represents its unique identifier within the context of the derived implementation.
    ///     This property is read-only and reflects the value initialized at the time of creating the repository instance.
    /// </remarks>
    public string Name { get; private set; } = name;

    /// <summary>
    ///     Gets or sets the list of location paths associated with the repository.
    /// </summary>
    /// <remarks>
    ///     This property holds a collection of location paths that are utilized by the repository
    ///     during its initialization phase to load and manage resources.
    ///     The locations in this list represent file paths or resource identifiers to be processed
    ///     by the repository.
    /// </remarks>
    public List<string> Locations { get; set; } = [];

    /// <summary>
    ///     Sets the name of the repository. This method updates the internal name field
    ///     to the given value, allowing the repository to have an updated or initially
    ///     defined name that identifies it within the system.
    /// </summary>
    /// <param name="name">
    ///     The new name to assign to the repository. This string represents the unique identifier
    ///     or descriptor for the repository instance.
    /// </param>
    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Initializes the repository by iterating through the defined locations and adding existing
    ///     resources to the repository. This method handles the initialization logic, including retrieving
    ///     file resources, verifying their existence, and invoking the repository addition process for each.
    /// </summary>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when an I/O error occurs during the initialization process, encapsulating
    ///     the original IOException as the inner exception.
    /// </exception>
    public virtual void Initialize()
    {
        try
        {
            foreach (var found in Locations.Select(FileUtils.GetFileResource).Where(found => found.Exists))
            {
                AddRepository(found);
            }
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to initialize repository", e);
        }
    }

    /// <summary>
    ///     Adds a repository to the current context or system by utilizing the provided resource.
    ///     This method must be implemented by a derived class to define the specific functionality
    ///     of how a repository is added.
    /// </summary>
    /// <param name="resource">
    ///     The resource to be added as a repository. The resource should implement the IResource
    ///     interface and provide details such as the resource's URI, description, existence, and file details.
    /// </param>
    protected abstract void AddRepository(IResource resource);
}
