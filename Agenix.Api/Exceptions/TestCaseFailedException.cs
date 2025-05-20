namespace Agenix.Api.Exceptions;

/// <summary>
///     Base exception marking failure of test case.
/// </summary>
[Serializable]
public class TestCaseFailedException(Exception cause)
    : AgenixSystemException(!string.IsNullOrWhiteSpace(cause.Message) ? cause.Message : "Test case failed", cause);