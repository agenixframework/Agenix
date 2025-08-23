using System.Reflection;

namespace Agenix.Screenplay.Annotations;

/// <summary>
///     Provides functionality to extract step names from methods decorated with the <see cref="StepAttribute" />.
/// </summary>
public static class StepName
{
    /// <summary>
    ///     Extracts the step name from a method annotated with the <see cref="StepAttribute" />.
    /// </summary>
    /// <param name="testMethod">
    ///     The method to extract the step name from. This method should be decorated with the <see cref="StepAttribute" />.
    /// </param>
    /// <returns>
    ///     The value of the step name defined in the <see cref="StepAttribute" /> if the attribute is present and has a
    ///     non-empty value; otherwise, null.
    /// </returns>
    public static string? FromStepAnnotationIn(MethodInfo testMethod)
    {
        var step = testMethod.GetCustomAttribute<StepAttribute>();
        if (step != null && !string.IsNullOrEmpty(step.Value))
        {
            return step.Value;
        }

        return null;
    }
}
