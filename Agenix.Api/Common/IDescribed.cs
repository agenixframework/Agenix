namespace Agenix.Api.Common;

/// <summary>
/// Provides a contract for objects that have a description.
/// The description is intended to give additional context or details about the implementing object.
/// </summary>
public interface IDescribed
{
    /// Retrieves the description associated with this instance.
    /// @return The description as a string.
    /// /
    public string GetDescription()
    {
        return "";
    }

    /// Sets the description for this instance.
    /// <param name="description">The description to be set.</param>
    /// <return>The current instance with the updated description.</return>
    ITestAction SetDescription(string description);
}