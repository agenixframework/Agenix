using System.Diagnostics;
using Agenix.Screenplay.Utils;

namespace Agenix.Screenplay;

/// <summary>
/// Provides functionality for generating a human-readable task name based on the currently executing method.
/// </summary>
public class HumanReadableTaskName
{
    /// Generates a human-readable task name for the currently executing method.
    /// <returns>
    /// A string representing the human-readable name for the current method,
    /// formatted by combining the class name and method name, and processed through the NameConverter.Humanize method.
    /// The result excludes any methods defined within the "Agenix" namespace hierarchy.
    /// </returns>
    public static string ForCurrentMethod()
    {
        var stackTrace = new StackTrace();
        var businessIndex = 1;
            
        for (var methodIndex = 1; methodIndex < stackTrace.FrameCount; methodIndex++)
        {
            var frame = stackTrace.GetFrame(methodIndex);
            var method = frame?.GetMethod();
            if (method?.DeclaringType?.FullName?.StartsWith("Agenix") == true) continue;
            businessIndex = methodIndex;
            break;
        }

        var newFrame = stackTrace.GetFrame(businessIndex);
        var declaringType = newFrame?.GetMethod()?.DeclaringType;
        var className = declaringType?.FullName ?? "";
        var methodName = newFrame?.GetMethod()?.Name ?? "";
            
        var simpleClassName = className.Contains(".")
            ? className.Substring(className.LastIndexOf('.') + 1)
            : className;
            
        return NameConverter.Humanize($"{simpleClassName}_{methodName}");
    }
  
}