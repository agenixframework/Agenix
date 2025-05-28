namespace Agenix.Screenplay.Annotations;

/// <summary>
/// Represents a custom attribute used to describe the subject of a class or method.
/// </summary>
/// <remarks>
/// This attribute is intended to annotate classes or methods with a subject, for example,
/// to provide contextual information or to enhance readability and organization
/// within the codebase. The subject is provided as a string value.
/// </remarks>
/// <example>
/// The usage of this attribute can help in categorizing or grouping
/// classes and methods based on a specific theme or topic.
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SubjectAttribute : Attribute
{
    public string Value { get; }

    public SubjectAttribute(string value = "")
    {
        Value = value;
    }
}
