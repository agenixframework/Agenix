using Agenix.Api.Exceptions;

namespace Agenix.Core.Util;

/// <summary>
///     Utility class providing helper methods for object validation.
/// </summary>
public class ObjectHelper
{
    // Private constructor to prevent instantiation
    private ObjectHelper()
    {
    }

    // Method to assert that an object is not null with a default error message

    // Method to assert that an object is not null with a custom error message
    public static T AssertNotNull<T>(T obj, string errorMessage = "Provided object must not be null")
    {
        if (obj == null) throw new AgenixSystemException(errorMessage);

        return obj;
    }
}