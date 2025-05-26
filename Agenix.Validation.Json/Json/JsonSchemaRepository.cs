using Agenix.Api.IO;
using Agenix.Api.Log;
using Agenix.Core.Repository;
using Agenix.Validation.Json.Json.Schema;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Json.Json
{
    /// <summary>
    /// Schema repository holding a set of JSON schema resources known in the test scope.
    /// </summary>
    public class JsonSchemaRepository : BaseRepository
    {
        
        private const string DefaultName = "jsonSchemaRepository";

        public JsonSchemaRepository() : base(DefaultName)
        {
            Schemas = [];
        }
        
        protected override void AddRepository(IResource resource)
        {
            if (resource.File.Extension.EndsWith(".json"))
            {
                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug($"Loading json schema resource '{resource.Description}'");
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
        
        public List<SimpleJsonSchema> Schemas { get; set; }

        /// <summary>
        /// Provides logging functionality for the <see cref="JsonSchemaRepository"/> class.
        /// </summary>
        public static ILogger Log { get; set; } = LogManager.GetLogger(typeof(JsonSchemaRepository));

        /// <summary>
        /// Adds a new JSON schema to the repository for validation purposes.
        /// </summary>
        /// <param name="simpleJsonSchema">The JSON schema to add represented as a <see cref="SimpleJsonSchema"/> object.</param>
        public void AddSchema(SimpleJsonSchema simpleJsonSchema)
        {
            Schemas.Add(simpleJsonSchema);
        }
    }
}