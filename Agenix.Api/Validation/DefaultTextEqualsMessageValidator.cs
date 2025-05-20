using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using log4net;

namespace Agenix.Api.Validation;

/// <summary>
///     Default message validator implementation performing text equals on given message payloads. Validator auto converts
///     message payloads into a String representation to perform text equals validation. Both received and control
///     messages should have textual message payloads. By default, the validator ignores leading and trailing whitespaces
///     and normalizes the line endings before the validation. Usually this validator implementation is used as a fallback
///     option when no other matching validator implementation could be found.
/// </summary>
public class DefaultTextEqualsMessageValidator : DefaultMessageValidator
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultTextEqualsMessageValidator));

    private bool _normalizeLineEndings = true;
    private bool _trim = true;

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        IValidationContext validationContext)
    {
        if (controlMessage?.Payload == null || string.IsNullOrEmpty(controlMessage.GetPayload<string>()))
        {
            _log.Debug("Skip message payload validation as no control message was defined");
            return;
        }

        _log.Debug("Start to verify message payload ...");

        var controlPayload = controlMessage.GetPayload<string>();
        var receivedPayload = receivedMessage.GetPayload<string>();

        if (_trim)
        {
            controlPayload = controlPayload.Trim();
            receivedPayload = receivedPayload.Trim();
        }

        if (_normalizeLineEndings)
        {
            controlPayload = NormalizeLineEndings(controlPayload);
            receivedPayload = NormalizeLineEndings(receivedPayload);
        }

        if (!receivedPayload.Equals(controlPayload))
            throw new ValidationException("Validation failed - message payload not equal " +
                                          GetFirstDiff(receivedPayload, controlPayload));
    }

    /// <summary>
    ///     Finds the first difference between the received and control strings, if any, and provides a formatted message
    ///     detailing the position and the differing characters.
    /// </summary>
    /// <param name="received">The received string to compare.</param>
    /// <param name="control">The control string to compare against.</param>
    /// <returns>A formatted message describing the first difference, or an empty string if there is no difference.</returns>
    public string GetFirstDiff(string received, string control)
    {
        int position;
        for (position = 0; position < received.Length && position < control.Length; position++)
            if (received[position] != control[position])
                break;

        if (position >= control.Length && position >= received.Length) return "";
        var controlEnd = Math.Min(position + 25, control.Length);
        var receivedEnd = Math.Min(position + 25, received.Length);

        return
            $"at position {position + 1} expected '{control.Substring(position, controlEnd - position)}', but was '{received.Substring(position, receivedEnd - position)}'";
    }

    /// <summary>
    ///     Enables the normalization of line endings in the validator, ensuring that different newline formats are normalized
    ///     before validation comparisons.
    /// </summary>
    /// <returns>The instance of DefaultTextEqualsMessageValidator with line ending normalization enabled.</returns>
    public DefaultTextEqualsMessageValidator EnableNormalizeLineEndings()
    {
        _normalizeLineEndings = true;
        return this;
    }

    /// <summary>
    ///     Enables trimming of whitespace from the beginning and end of the strings being validated,
    ///     ensuring that leading and trailing spaces do not affect the outcome of the validation.
    /// </summary>
    /// <returns>The instance of DefaultTextEqualsMessageValidator with trimming enabled.</returns>
    public DefaultTextEqualsMessageValidator EnableTrim()
    {
        _trim = true;
        return this;
    }

    /// <summary>
    ///     Normalize the text by replacing line endings with a Linux representation.
    /// </summary>
    public static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("&#13;", "");
    }

    /// <summary>
    /// Searches for an appropriate validation context within the provided list, prioritizing message validation contexts.
    /// If a message validation context is found, it is returned; if not, the base class implementation is invoked for a fallback.
    /// </summary>
    /// <param name="validationContexts">
    /// A list of validation contexts to search through for a suitable message validation context.
    /// </param>
    /// <returns>
    /// An instance of a message validation context if found within the list, or a fallback validation context provided by
    /// the base implementation.
    /// </returns>
    public override IValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        var messageValidationContext = validationContexts
            .FirstOrDefault(context => context is IMessageValidationContext);

        return messageValidationContext ?? base.FindValidationContext(validationContexts);
    }
}