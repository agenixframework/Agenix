namespace Agenix.Api.Annotations;

/// <summary>
///     Agenix test case annotation used in C# DSL test cases to execute several tests within one single test builder
///     class. Each method annotated with this annotation will result in a separate test execution.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AgenixTestAttribute : Attribute
{
    /// <summary>
    ///     Test name optional - by default, the method name is used as a test name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}