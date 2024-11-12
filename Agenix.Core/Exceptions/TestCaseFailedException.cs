using System;

namespace Agenix.Core.Exceptions;

/// <summary>
///     Base exception marking failure of test case.
/// </summary>
[Serializable]
public class TestCaseFailedException(Exception cause)
    : CoreSystemException(!string.IsNullOrWhiteSpace(cause.Message) ? cause.Message : "Test case failed", cause);