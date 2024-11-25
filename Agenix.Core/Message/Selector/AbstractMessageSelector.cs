using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Matcher;

namespace Agenix.Core.Message.Selector;

/// <summary>
///     Abstract base class for message selectors.
/// </summary>
/// <remarks>
///     This abstract class provides the basic functionality to select messages based on key-value pairs
///     and supports validation matcher expressions.
/// </remarks>
public abstract class AbstractMessageSelector(string selectKey, string matchingValue, TestContext context)
    : IMessageSelector
{
    /// <summary>
    ///     Test Context
    /// </summary>
    protected readonly TestContext _context = context;

    protected readonly string _matchingValue = matchingValue;

    /// <summary>
    ///     Key and value to evaluate selection with
    /// </summary>
    protected readonly string _selectKey = selectKey;

    public abstract bool Accept(IMessage message);

    /// <summary>
    ///     Reads message payload as String either from message object directly or from nested Citrus message representation.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public string GetPayloadAsString(IMessage message)
    {
        if (message.Payload is IMessage innerMessage) return innerMessage.GetPayload<string>();

        return message.Payload.ToString();
    }

    /// <summary>
    ///     Evaluates given value to match this selectors matching condition. Automatically supports validation matcher
    ///     expressions.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected bool Evaluate(string value)
    {
        if (ValidationMatcherUtils.IsValidationMatcherExpression(_matchingValue))
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher(_selectKey, value, _matchingValue, _context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }

        return value.Equals(_matchingValue);
    }
}