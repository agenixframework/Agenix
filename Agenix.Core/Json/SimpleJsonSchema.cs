#region Imports
using Agenix.Core.Common;
using Agenix.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

#endregion

namespace Agenix.Core.Json;

/// <summary>
///     Adapter between the resource reference from the bean configuration and the usable SimpleJsonSchema for validation.
/// </summary>
public class SimpleJsonSchema : InitializingPhase
{
    public SimpleJsonSchema(IResource resource)
    {
        Json = resource;
    }

    public SimpleJsonSchema()
    {
    }

    /// <summary>
    ///     The Resource of the json schema passed from the file
    /// </summary>
    private IResource Json { get; }

    /// <summary>
    ///     The parsed json schema ready for validation
    /// </summary>
    public JSchema Schema { get; set; }

    /// <summary>
    /// Initializes the SimpleJsonSchema by loading the JSON schema definition
    /// from the associated IResource instance into the Schema property.
    /// </summary>
    /// <remarks>
    /// This method uses a JSON text reader provided by the IResource's GetReader()
    /// implementation to parse and load the schema using the JSchema class from
    /// the Newtonsoft.Json.Schema library.
    /// </remarks>
    /// <exception cref="JsonException">
    /// Thrown if there is an error during the parsing or loading of the JSON schema.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the IResource instance used to retrieve the JSON reader is null or invalid.
    /// </exception>
    public void Initialize()
    {
        Schema = JSchema.Load(new JsonTextReader(Json.GetReader()));
    }
}