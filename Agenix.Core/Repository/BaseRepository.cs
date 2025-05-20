using System.Collections.Generic;
using System.IO;
using System.Linq;
using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Agenix.Core.Util;

namespace Agenix.Core.Repository;

/// <summary>
/// Provides a base implementation for a repository, including functionality for
/// initialization, managing locations, and handling repository resources.
/// </summary>
public abstract class BaseRepository(string name) : INamed, InitializingPhase
{
    private string _name = name;

    /// <summary>
    /// Initializes the repository by iterating through the defined locations and adding existing
    /// resources to the repository. This method handles the initialization logic, including retrieving
    /// file resources, verifying their existence, and invoking the repository addition process for each.
    /// </summary>
    /// <exception cref="AgenixSystemException">
    /// Thrown when an I/O error occurs during the initialization process, encapsulating
    /// the original IOException as the inner exception.
    /// </exception>
    public void Initialize()
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
    /// Adds a repository to the current context or system by utilizing the provided resource.
    /// This method must be implemented by a derived class to define the specific functionality
    /// of how a repository is added.
    /// </summary>
    /// <param name="resource">
    /// The resource to be added as a repository. The resource should implement the IResource
    /// interface and provide details such as the resource's URI, description, existence, and file details.
    /// </param>
    protected abstract void AddRepository(IResource resource);

    /// <summary>
    /// Sets the name of the repository. This method updates the internal name field
    /// to the given value, allowing the repository to have an updated or initially
    /// defined name that identifies it within the system.
    /// </summary>
    /// <param name="name">
    /// The new name to assign to the repository. This string represents the unique identifier
    /// or descriptor for the repository instance.
    /// </param>
    public void SetName(string name)
    {
        _name = name;
    }

    /// <summary>
    /// Gets the name of the repository.
    /// </summary>
    /// <remarks>
    /// The name is assigned during the construction of the repository
    /// and represents its unique identifier within the context of the derived implementation.
    /// This property is read-only and reflects the value initialized at the time of creating the repository instance.
    /// </remarks>
    public string Name => _name;

    /// <summary>
    /// Gets or sets the list of location paths associated with the repository.
    /// </summary>
    /// <remarks>
    /// This property holds a collection of location paths that are utilized by the repository
    /// during its initialization phase to load and manage resources.
    /// The locations in this list represent file paths or resource identifiers to be processed
    /// by the repository.
    /// </remarks>
    public List<string> Locations { get; set; }
}