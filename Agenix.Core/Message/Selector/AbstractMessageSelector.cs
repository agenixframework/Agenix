using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Validation.Matcher;

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
    protected readonly TestContext Context = context;

    protected readonly string MatchingValue = matchingValue;

    /// <summary>
    ///     Key and value to evaluate selection with
    /// </summary>
    protected readonly string SelectKey = selectKey;

    public abstract bool Accept(IMessage message);

    /// <summary>
    ///     Reads message payload as String either from a message object directly or from nested Agenix message representation.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public string GetPayloadAsString(IMessage message)
    {
        if (message.Payload is IMessage innerMessage) return innerMessage.GetPayload<string>();

        return message.Payload.ToString();
    }

    /// <summary>
    ///     Evaluates the given value to match this selector matching condition. Automatically supports validation matcher
    ///     expressions.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected bool Evaluate(string value)
    {
        if (ValidationMatcherUtils.IsValidationMatcherExpression(MatchingValue))
            try
            {
                ValidationMatcherUtils.ResolveValidationMatcher(SelectKey, value, MatchingValue, Context);
                return true;
            }
            catch (ValidationException)
            {
                return false;
            }

        return value.Equals(MatchingValue);
    }
}