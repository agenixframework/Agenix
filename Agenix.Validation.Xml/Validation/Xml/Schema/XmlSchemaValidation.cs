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


using System.Text;
using System.Xml;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Schema;
using Agenix.Validation.Xml.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml.Schema;

public class XmlSchemaValidation : ISchemaValidator<ISchemaValidationContext>
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(XmlSchemaValidation));

    /**
     * fail if no schema found property
     */
    private readonly ValidationStrategy _noSchemaFoundStrategy;

    public XmlSchemaValidation() : this(GetSchemaValidationStrategy())
    {
    }

    public XmlSchemaValidation(ValidationStrategy noSchemaFoundStrategy)
    {
        _noSchemaFoundStrategy = noSchemaFoundStrategy;
    }

    public void Validate(IMessage message, TestContext context, ISchemaValidationContext validationContext)
    {
        ValidateSchema(message, context, validationContext);
    }

    /// <summary>
    ///     Determines whether the given message type is supported for validation based on its type and payload.
    /// </summary>
    /// <param name="messageType">The type of the message as a string to be checked for support.</param>
    /// <param name="message">The <c>IMessage</c> instance representing the message to be validated.</param>
    /// <returns>A boolean value indicating whether the message type is supported for validation.</returns>
    public bool SupportsMessageType(string messageType, IMessage message)
    {
        return string.Equals(messageType, "XML", StringComparison.OrdinalIgnoreCase) ||
               (message != null && IsXmlPredicate.Instance.Test(message.GetPayload<string>()));
    }

    /// <summary>
    ///     Determines whether the provided message can be validated based on the enabled schema validation
    ///     setting and whether the message satisfies the XML predicate test.
    /// </summary>
    /// <param name="message">The <c>IMessage</c> instance representing the message to be validated.</param>
    /// <param name="schemaValidationEnabled">A boolean value indicating whether schema validation is explicitly enabled.</param>
    /// <returns>A boolean value indicating whether the message is validatable with XML schema rules.</returns>
    public bool CanValidate(IMessage message, bool schemaValidationEnabled)
    {
        return (IsXmlSchemaValidationEnabled() || schemaValidationEnabled)
               && IsXmlPredicate.Instance.Test(message.GetPayload<string>());
    }


    /// <summary>
    ///     Validates the provided message against the specified XML schema by creating a validation
    ///     context using the schema and schema repository, then delegating validation to the existing
    ///     mechanism with the constructed validation context.
    /// </summary>
    /// <param name="message">The message to be validated. Must conform to the specified XML schema.</param>
    /// <param name="context">The test execution context, which may provide additional information for validation.</param>
    /// <param name="schemaRepository">The repository or location where the XML schema is stored.</param>
    /// <param name="schema">The name or identifier of the specific XML schema to validate against.</param>
    public void Validate(IMessage message, TestContext context, string schemaRepository, string schema)
    {
        var validationContext = XmlMessageValidationContext.Builder.Xml()
            .SchemaValidation(true)
            .Schema(schema)
            .SchemaRepository(schemaRepository)
            .Build();
        Validate(message, context, validationContext);
    }

    private void ValidateSchema(IMessage message, TestContext context, ISchemaValidationContext validationContext)
    {
        if (message.GetPayload<string>() == null || string.IsNullOrWhiteSpace(message.GetPayload<string>()))
        {
            return;
        }

        try
        {
            var doc = XmlUtils.ParseMessagePayload(message.GetPayload<string>());

            if (string.IsNullOrWhiteSpace(doc.FirstChild?.NamespaceURI))
            {
                return;
            }

            Logger.LogDebug("Starting XML schema validation ...");

            XmlSchemaValidator validator = null;
            IXsdSchema xsdSchema = null;
            XsdSchemaRepository schemaRepository = null;
            var schemaRepositories = XmlValidationHelper.GetSchemaRepositories(context);

            if (validationContext.Schema != null)
            {
                xsdSchema = context.ReferenceResolver.Resolve<IXsdSchema>(validationContext.Schema);
            }
            else if (validationContext.SchemaRepository != null)
            {
                schemaRepository =
                    context.ReferenceResolver.Resolve<XsdSchemaRepository>(validationContext.SchemaRepository);
            }
            else
            {
                switch (schemaRepositories.Count)
                {
                    case 1:
                        schemaRepository = schemaRepositories[0];
                        break;
                    case > 0:
                    {
                        schemaRepository = schemaRepositories
                            .FirstOrDefault(repository => repository.CanValidate(doc));

                        if (schemaRepository == null)
                        {
                            throw new AgenixSystemException(
                                $"Failed to find proper schema repository for validating element '{doc.FirstChild.LocalName}({doc.FirstChild.NamespaceURI})'");
                        }

                        break;
                    }
                    default:
                        Logger.LogWarning(
                            "Neither schema instance nor schema repository defined - skipping XML schema validation");
                        return;
                }
            }
            // TODO: To be reviwed how handle when schema respoistory is not null
            /*if (schemaRepository != null)
            {
                if (!schemaRepository.CanValidate(doc))
                {
                    var noSchemaFoundStrategy = GetSchemaValidationStrategy();
                    if (noSchemaFoundStrategy == ValidationStrategy.FAIL)
                    {
                        throw new AgenixSystemException(
                            $"Unable to find proper XML schema definition for element '{doc.FirstChild.LocalName}({doc.FirstChild.NamespaceURI})' in schema repository '{schemaRepository.Name}'");
                    }
                    else
                    {
                        if (Logger.IsEnabled(LogLevel.Trace))
                        {
                            Logger.LogTrace(CreateSchemaNotFoundMessage(doc, schemaRepository));
                        }
                        return;
                    }
                }

                var schemas = new List<IResource>();
                foreach (var xsdSchema in schemaRepository.GetSchemas())
                {
                    switch (xsdSchema)
                    {
                        case XsdSchemaCollection xsdSchemaCollection:
                            schemas.AddRange(xsdSchemaCollection.GetSchemaResources());
                            break;
                        case WsdlXsdSchema wsdlXsdSchema:
                            schemas.AddRange(wsdlXsdSchema.GetSchemaResources());
                            break;
                        default:
                            lock (TransformerFactory)
                            {
                                using var memoryStream = new MemoryStream();
                                try
                                {
                                    var transformer = TransformerFactory.NewTransformer();
                                    transformer.Transform(xsdSchema.GetSource(), new StreamResult(memoryStream));
                                }
                                catch (TransformerException e)
                                {
                                    throw new AgenixSystemException($"Failed to read schema {xsdSchema.TargetNamespace}", e);
                                }
                                schemas.Add(Resources.Create(memoryStream.ToArray()));
                            }
                            break;
                    }
                }

                var springResources = schemas
                    .Select(AbstractSchemaCollection.ToSpringResource)
                    .ToArray();

                validator = XmlValidatorFactory.CreateValidator(springResources, W3C_XML_SCHEMA_NS_URI);
            }*/


            if (xsdSchema != null)
            {
                var results = xsdSchema.Validate(doc);
                if (results.Count == 0)
                {
                    Logger.LogDebug("XML schema validation successful: All values OK");
                }
                else
                {
                    if (Logger.IsEnabled(LogLevel.Error))
                    {
                        Logger.LogError("XML schema validation failed for message:\n{PrettyPrint}",
                            XmlUtils.PrettyPrint(message.GetPayload<string>()));
                    }

                    // Report all parsing errors
                    Logger.LogDebug("Found {ResultsCount} schema validation errors", results.Count);
                    var errors = new StringBuilder();
                    foreach (var error in results)
                    {
                        errors.AppendLine(error);
                    }

                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug("Errors: {errors}", string.Join(", ", errors));
                    }

                    throw new ValidationException("XML schema validation failed:" + Environment.NewLine + errors);
                    ;
                }
            }
        }
        catch (IOException e)
        {
            throw new AgenixSystemException(e.Message);
        }
    }

    /// <summary>
    ///     Creates a detailed error message indicating the absence of a suitable XML schema
    ///     definition for the provided XML document within the specified schema repository.
    /// </summary>
    /// <param name="doc">
    ///     The <c>XmlDocument</c> instance representing the XML content
    ///     for which a schema could not be found.
    /// </param>
    /// <param name="schemaRepository">
    ///     The <c>XsdSchemaRepository</c> instance where the
    ///     schema lookup was performed.
    /// </param>
    /// <returns>
    ///     A string message describing the schema lookup failure, including the
    ///     XML element details and the name of the schema repository.
    /// </returns>
    private static string CreateSchemaNotFoundMessage(XmlDocument doc, XsdSchemaRepository schemaRepository)
    {
        return
            $"Unable to find proper XML schema definition for element '{doc.FirstChild.LocalName}({doc.FirstChild.NamespaceURI})' in schema repository '{schemaRepository.Name}'";
    }


    /// <summary>
    ///     Determines the appropriate validation strategy for XML schema validation
    ///     based on application configuration settings. If the configuration is invalid
    ///     or missing, a default strategy is applied, or an exception is thrown.
    /// </summary>
    /// <returns>
    ///     A <c>ValidationStrategy</c> value representing the configured or default
    ///     schema validation handling behavior.
    /// </returns>
    private static ValidationStrategy GetSchemaValidationStrategy()
    {
        var strategy = AgenixSettings.GetNoSchemaFoundStrategy();

        if (!strategy.IsPresent || string.IsNullOrWhiteSpace(strategy.Value))
        {
            return ValidationStrategy.FAIL;
        }

        var upperValue = strategy.Value.ToUpperInvariant();

        if (Enum.TryParse<ValidationStrategy>(upperValue, out var result))
        {
            return result;
        }

        throw new AgenixSystemException($"Invalid property value '{strategy.Value}' for no schema found strategy");
    }


    /// <summary>
    ///     Determines if XML schema validation is enabled based on application settings.
    ///     Combines multiple configuration checks to infer the validation status.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether XML schema validation is enabled.
    /// </returns>
    private static bool IsXmlSchemaValidationEnabled()
    {
        return AgenixSettings.IsOutboundSchemaValidationEnabled() ||
               AgenixSettings.IsOutboundXmlSchemaValidationEnabled();
    }
}
