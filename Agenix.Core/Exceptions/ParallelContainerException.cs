using System.Collections.Generic;
using System.Text;

namespace Agenix.Core.Exceptions;

/// Represents a custom exception that is thrown when multiple actions in a parallel container fail.
/// This exception aggregates a list of nested exceptions that caused the parallel execution to fail,
/// providing a comprehensive message detailing each individual exception.
/// /
public class ParallelContainerException(List<CoreSystemException> nestedExceptions) : CoreSystemException
{
    public override string Message
    {
        get
        {
            var builder = new StringBuilder();

            builder.Append("Several actions failed in parallel container");
            foreach (var exception in nestedExceptions)
                builder.Append("\n\t+ ").Append(exception.GetType().Name).Append(": ").Append(exception.Message);

            return builder.ToString();
        }
    }
}