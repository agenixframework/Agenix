using Agenix.Api.Message;
using Agenix.Core.Actions;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Supports the construction of SendMessageAction instances by providing additional methods
///     for configuring specific settings such as fork mode and message processors.
/// </summary>
/// <typeparam name="T">Type of SendMessageAction.</typeparam>
/// <typeparam name="TB">Type of SendMessageActionBuilder for building SendMessageAction.</typeparam>
/// <typeparam name="TS">Type of the derived SendMessageBuilderSupport.</typeparam>
public class SendMessageBuilderSupport<T, TB, TS>(TB dlg) : MessageBuilderSupport<T, TB, TS>(dlg)
    where T : SendMessageAction
    where TB : SendMessageAction.SendMessageActionBuilder<T, TS, TB>
    where TS : SendMessageBuilderSupport<T, TB, TS>
{
    protected string _schema;
    protected string _schemaRepository;
    protected bool _schemaValidation;

    /// Retrieves the schema associated with the send message action.
    /// @return the schema as a string
    /// /
    public string GetSchema => _schema;

    /// <summary>
    ///     Retrieves the schema repository associated with the send message action.
    /// </summary>
    /// <returns>A string representing the schema repository.</returns>
    public string GetSchemaRepository => _schemaRepository;

    /// <summary>
    ///     Sets the fork mode for this send message builder support.
    /// </summary>
    /// <param name="forkMode">Specifies whether the fork mode should be enabled or disabled.</param>
    /// <returns>The instance of the send message builder supports with the updated fork mode.</returns>
    public TS Fork(bool forkMode)
    {
        _delegate.Fork(forkMode);
        return _self;
    }

    /// <summary>
    ///     Adds message processor on the message to be sent.
    /// </summary>
    /// <param name="processor"></param>
    /// <returns>The modified send message action builder</returns>
    public TS Transform(IMessageProcessor processor)
    {
        return Process(processor);
    }

    /// <summary>
    ///     Applies the specified message processor to transform the send message action.
    /// </summary>
    /// <param name="builder">The message processor to apply.</param>
    /// <returns>The instance of the sent message builder supports with the applied transformation.</returns>
    public TS Transform<B>(IMessageProcessor.IBuilder<IMessageProcessor, B> builder)
        where B : IMessageProcessor.IBuilder<IMessageProcessor, B>
    {
        return Transform(builder.Build());
    }

    /// <summary>
    ///     Enables or disables schema validation for the message.
    /// </summary>
    /// <param name="enabled">Specifies whether schema validation should be enabled or disabled.</param>
    /// <returns>The instance of the sent message builder supports with the updated schema validation setting.</returns>
    public TS SchemaValidation(bool enabled)
    {
        _schemaValidation = enabled;
        return _self;
    }

    /// <summary>
    ///     Retrieves the current state of the schema validation flag.
    /// </summary>
    /// <returns>A boolean value indicating whether schema validation is enabled.</returns>
    public bool IsSchemaValidation()
    {
        return _schemaValidation;
    }

    /// <summary>
    ///     Sets the schema instance name to be used for schema validation.
    /// </summary>
    /// <param name="schemaName">The name of the schema instance to use for validation.</param>
    /// <returns>The instance of the send message builder support with the specified schema applied.</returns>
    public TS Schema(string schemaName)
    {
        _schema = schemaName;
        return _self;
    }

    /// <summary>
    ///     Sets the schema repository to be used for validation during message processing.
    /// </summary>
    /// <param name="schemaRepository">The identifier or instance of the schema repository to use.</param>
    /// <returns>The current instance of the send message builder support with the specified schema repository.</returns>
    public TS SchemaRepository(string schemaRepository)
    {
        _schemaRepository = schemaRepository;
        return _self;
    }
}