using Agenix.Api.Validation;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Dsl;

/// <summary>
/// Provides a set of static methods to support XML-related validation processes.
/// </summary>
public static class XmlSupport
{
    /// <summary>
    /// Entrance for all XML-related validation functionalities.
    /// </summary>
    /// <returns>XML message validation context builder</returns>
    public static XmlMessageValidationContext.Builder Xml()
    {
        return XmlMessageValidationContext.Builder.Xml();
    }

    /// <summary>
    /// Marshaling validation processor builder entrance.
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="validationProcessor">The validation processor</param>
    /// <returns>XML marshaling validation processor builder</returns>
    public static XmlMarshallingValidationProcessor<T>.Builder<T> Validate<T>(GenericValidationProcessor<T> validationProcessor)
    {
        return XmlMarshallingValidationProcessor<T>.Builder<T>.Validate(validationProcessor);
    }
}
