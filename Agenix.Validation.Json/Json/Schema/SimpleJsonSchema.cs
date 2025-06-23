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

#region Imports

using Agenix.Api.Common;
using Agenix.Api.Exceptions;
using Agenix.Api.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

#endregion

namespace Agenix.Validation.Json.Json.Schema;

/// <summary>
///     Adapter between the resource reference from the bean configuration and the usable SimpleJsonSchema for validation.
/// </summary>
public class SimpleJsonSchema : InitializingPhase
{
    /// <summary>
    ///     Represents an adapter for handling a JSON schema and its associated resource references,
    ///     enabling usage in validation workflows.
    /// </summary>
    /// <remarks>
    ///     This class integrates with custom resource references and loaded JSON schemas,
    ///     providing a framework to use them within validation functionalities.
    ///     It is designed to be initialized with valid resources and schemas for proper operation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when required resource or schema objects are null during initialization.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the schema initialization process encounters errors.
    /// </exception>
    public SimpleJsonSchema(IResource json)
    {
        Json = json;
    }

    public SimpleJsonSchema()
    {
    }

    /// <summary>
    ///     The Resource of the JSON schema passed from the file
    /// </summary>
    private IResource Json { get; } = null!;

    /// <summary>
    ///     The parsed JSON schema ready for validation
    /// </summary>
    public JSchema? Schema { get; set; }

    /// <summary>
    ///     Initializes the SimpleJsonSchema instance by loading and parsing
    ///     the JSON schema definition into the Schema property.
    /// </summary>
    /// <remarks>
    ///     This method retrieves the JSON schema from an IResource instance using
    ///     its GetReader() method and processes it using the JSchema.Load method
    ///     provided by the Newtonsoft.Json.Schema library.
    /// </remarks>
    /// <exception cref="IOException">
    ///     Thrown when there is an error in retrieving the JSON schema resource or during schema parsing.
    /// </exception>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when there is an unexpected issue during the loading of the JSON schema.
    /// </exception>
    public void Initialize()
    {
        try
        {
            Schema = JSchema.Load(new JsonTextReader(Json.GetReader()));
        }
        catch (IOException e)
        {
            throw new AgenixSystemException("Failed to load Json schema", e);
        }
    }
}
