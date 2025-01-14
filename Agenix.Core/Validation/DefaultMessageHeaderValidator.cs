using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Agenix.Core.Endpoint.Resolver;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;
using log4net;

namespace Agenix.Core.Validation;

/// <summary>
///     Basic header message validator provides message header validation. Subclasses only have to add specific logic for
///     message payload validation. This validator is based on a control message.
/// </summary>
public class DefaultMessageHeaderValidator : AbstractMessageValidator<HeaderValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(DefaultMessageHeaderValidator));

    /// <summary>
    ///     A dictionary containing the set of default header validators located via resource path lookup.
    /// </summary>
    private readonly IDictionary<string, IHeaderValidator> _defaultValidators = IHeaderValidator.Lookup();

    /// <summary>
    ///     List of special header validators.
    /// </summary>
    private List<IHeaderValidator> _validators = [];

    /// <summary>
    ///     Determines if the given message type is supported by this message validator.
    /// </summary>
    /// <param name="messageType">The type of the message to be validated.</param>
    /// <param name="message">The message instance to be validated.</param>
    /// <returns>True if the message type is supported, otherwise false.</returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return true;
    }

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        HeaderValidationContext validationContext)
    {
        IDictionary<string, object> controlHeaders = controlMessage.GetHeaders();
        IDictionary<string, object> receivedHeaders = receivedMessage.GetHeaders();

        if (controlHeaders == null || !controlHeaders.Any()) return;

        _log.Debug("Start message header validation ...");

        foreach (var (key, controlValue) in controlHeaders)
        {
            if (MessageHeaderUtils.IsSpringInternalHeader(key) ||
                key.StartsWith(MessageHeaders.MessagePrefix) ||
                key.Equals(IEndpointUriResolver.EndpointUriHeaderName) ||
                key.Equals(IEndpointUriResolver.RequestPathHeaderName) ||
                key.Equals(IEndpointUriResolver.QueryParamHeaderName))
                continue;

            var headerName = GetHeaderName(key, receivedHeaders, context, validationContext);

            if (!receivedHeaders.TryGetValue(headerName, out var value))
                throw new ValidationException($"Validation failed: Header element '{headerName}' is missing");

            var validator = validationContext.Validators
                                .FirstOrDefault(v => v.Supports(headerName, controlValue?.GetType()))
                            ?? validationContext.ValidatorNames
                                .Select(beanName =>
                                {
                                    try
                                    {
                                        return context.GetReferenceResolver().Resolve<IHeaderValidator>(beanName);
                                    }
                                    catch (CoreSystemException)
                                    {
                                        _log.Warn($"Failed to resolve header validator for name: {beanName}");
                                        return null;
                                    }
                                })
                                .FirstOrDefault(v => v != null && v.Supports(headerName, controlValue?.GetType()))
                            ?? GetHeaderValidators(context)
                                .FirstOrDefault(v => v.Supports(headerName, controlValue?.GetType()))
                            ?? new DefaultHeaderValidator();

            validator.ValidateHeader(headerName, value, controlValue, context, validationContext);
        }

        _log.Debug("Message header validation successful: All values OK");
    }

    /// <summary>
    ///     Retrieves a list of header validators that are available in the given test context.
    /// </summary>
    /// <param name="context">The context of the test which may influence the selection of validators.</param>
    /// <returns>A list of distinct header validators available in the test context.</returns>
    private List<IHeaderValidator> GetHeaderValidators(TestContext context)
    {
        // add validators from resource path lookup
        var validatorMap = new Dictionary<string, IHeaderValidator>(_defaultValidators);

        return validatorMap.Values.Distinct().ToList();
    }

    /// <summary>
    ///     Retrieves the appropriate header name, potentially performing a case-insensitive search if configured.
    /// </summary>
    /// <param name="name">The name of the header to be resolved.</param>
    /// <param name="receivedHeaders">A collection of headers received in the message.</param>
    /// <param name="context">The context of the test, providing auxiliary functions and variables.</param>
    /// <param name="validationContext">The context for header validation, including settings such as case sensitivity.</param>
    /// <returns>The resolved header name, potentially found in a case-insensitive manner.</returns>
    private string GetHeaderName(string name, IDictionary<string, object> receivedHeaders, TestContext context,
        HeaderValidationContext validationContext)
    {
        var headerName = context.ResolveDynamicValue(name);

        if (receivedHeaders.ContainsKey(headerName) || !validationContext.HeaderNameIgnoreCase) return headerName;
        var key = headerName;

        _log.Debug($"Finding case insensitive header for key '{key}'");

        headerName = receivedHeaders
                         .AsParallel()
                         .Where(item => item.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                         .Select(item => item.Key)
                         .FirstOrDefault() ??
                     throw new ValidationException($"Validation failed: No matching header for key '{key}'");

        _log.Debug($"Found matching case insensitive header name: {headerName}");

        return headerName;
    }


    /// <summary>
    ///     Returns the required type of validation context for this message validator.
    /// </summary>
    /// <returns>The <see cref="Type" /> of the required validation context.</returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(HeaderValidationContext);
    }

    /// <summary>
    ///     Adds a header validator to the list.
    /// </summary>
    /// <param name="validator">The <see cref="IHeaderValidator" /> to add.</param>
    public void AddHeaderValidator(IHeaderValidator validator)
    {
        _validators.Add(validator);
    }

    /// <summary>
    ///     Retrieves the list of header validators.
    /// </summary>
    /// <returns>An unmodifiable list of <see cref="IHeaderValidator" />s.</returns>
    public IReadOnlyList<IHeaderValidator> GetValidators()
    {
        return new ReadOnlyCollection<IHeaderValidator>(_validators);
    }

    /// <summary>
    ///     Sets the list of header validators.
    /// </summary>
    /// <param name="validators">The list of <see cref="IHeaderValidator" />s to set.</param>
    public void SetValidators(List<IHeaderValidator> validators)
    {
        _validators = validators;
    }
}