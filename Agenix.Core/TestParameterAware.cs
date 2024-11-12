using System.Collections.Generic;

namespace Agenix.Core;

/// Interface marks test case to support test parameters.
/// /
public interface ITestParameterAware
{
    /// Sets the parameters.
    /// @param parameterNames the parameter names to set
    /// @param parameterValues the parameters to set
    /// /
    void SetParameters(string[] parameterNames, object[] parameterValues);

    /// Gets the test parameters.
    /// @return the parameters
    /// /
    Dictionary<string, object> GetParameters();
}