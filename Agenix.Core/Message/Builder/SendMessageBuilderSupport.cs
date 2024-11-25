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
    /// <summary>
    ///     Sets the fork mode for this send message builder support.
    /// </summary>
    /// <param name="forkMode">Specifies whether the fork mode should be enabled or disabled.</param>
    /// <returns>The instance of the send message builder support with the updated fork mode.</returns>
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
    /// <param name="processor">The message processor to apply.</param>
    /// <returns>The instance of the send message builder support with the applied transformation.</returns>
    public TS Transform<B>(IMessageProcessor.IBuilder<IMessageProcessor, B> builder)
        where B : IMessageProcessor.IBuilder<IMessageProcessor, B>
    {
        return Transform(builder.Build());
    }
}