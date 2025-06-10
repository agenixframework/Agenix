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
using Agenix.Api.Log;
using Agenix.Core.Repository;
using Agenix.Validation.Json.Json.Schema;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Json;

/// <summary>
///     Schema repository holding a set of JSON schema resources known in the test scope.
/// </summary>
public class JsonSchemaRepository : BaseRepository
{
    private const string DefaultName = "jsonSchemaRepository";

    public JsonSchemaRepository() : base(DefaultName)
    {
        Schemas = [];
    }

    public List<SimpleJsonSchema> Schemas { get; set; }

    /// <summary>
    ///     Provides logging functionality for the <see cref="JsonSchemaRepository" /> class.
    /// </summary>
    public static ILogger Log { get; set; } = LogManager.GetLogger(typeof(JsonSchemaRepository));

    protected override void AddRepository(IResource resource)
    {
        if (resource.File.Extension.EndsWith(".json"))
        {
            if (Log.IsEnabled(LogLevel.Debug))
            {
                Log.LogDebug("Loading json schema resource '{ResourceDescription}'", resource.Description);
            }

            var simpleJsonSchema = new SimpleJsonSchema(resource);
            simpleJsonSchema.Initialize();
            Schemas.Add(simpleJsonSchema);
        }
        else
        {
            Log.LogWarning($"Skipped resource other than json schema for repository '{resource.Description}'");
        }
    }

    /// <summary>
    ///     Adds a new JSON schema to the repository for validation purposes.
    /// </summary>
    /// <param name="simpleJsonSchema">The JSON schema to add represented as a <see cref="SimpleJsonSchema" /> object.</param>
    public void AddSchema(SimpleJsonSchema simpleJsonSchema)
    {
        Schemas.Add(simpleJsonSchema);
    }
}
