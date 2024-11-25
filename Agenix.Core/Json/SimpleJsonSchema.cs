using Agenix.Core.Common;
using Agenix.Core.Spi;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

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

    public void Initialize()
    {
        Schema = JSchema.Load(new JsonTextReader(Json.GetReader()));
    }
}