namespace Agenix.Api.Validation.Context;

/// <summary>
///     Interface for schema validation context.
/// </summary>
public interface ISchemaValidationContext
{
    /// <summary>
    ///     Gets a value indicating whether schema validation is enabled.
    /// </summary>
    bool IsSchemaValidationEnabled { get; }

    /// <summary>
    ///     Gets the schema repository.
    /// </summary>
    string SchemaRepository { get; }

    /// <summary>
    ///     Gets the schema.
    /// </summary>
    string Schema { get; }


    /// <summary>
    ///     Fluent builder interface.
    /// </summary>
    /// <typeparam name="TB">The type of the builder.</typeparam>
    public interface IBuilder<TB>
    {
        /// <summary>
        ///     Sets schema validation enabled or disabled for this message.
        /// </summary>
        /// <param name="enabled">True to enable schema validation; otherwise, false.</param>
        /// <returns>The builder.</returns>
        TB SchemaValidation(bool enabled);

        /// <summary>
        ///     Sets an explicit schema instance name to use for schema validation.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The builder.</returns>
        TB Schema(string schemaName);

        /// <summary>
        ///     Sets an explicit XSD schema repository instance to use for validation.
        /// </summary>
        /// <param name="schemaRepository">The schema repository.</param>
        /// <returns>The builder.</returns>
        TB SchemaRepository(string schemaRepository);
    }
}