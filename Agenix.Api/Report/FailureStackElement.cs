namespace Agenix.Api.Report;

/// <summary>
///     Failure stack element provides access to the detailed failure stack message and
///     the location in the test case XML where error happened.
/// </summary>
public class FailureStackElement
{
    /// <summary>
    ///     The name of the failed action
    /// </summary>
    private readonly string _actionName;

    /// <summary>
    ///     Line number in XML test case where error happened
    /// </summary>
    private readonly long _lineNumberStart;

    /// <summary>
    ///     Path to XML test file
    /// </summary>
    private readonly string _testFilePath;

    /// <summary>
    ///     Failing action in XML test case ends in this line
    /// </summary>
    private long _lineNumberEnd;

    /// <summary>
    ///     Default constructor using fields.
    /// </summary>
    /// <param name="testFilePath">file path of failed test.</param>
    /// <param name="actionName">the failed action name.</param>
    /// <param name="lineNumberStart">the line number where the error happened.</param>
    public FailureStackElement(string testFilePath, string actionName, long lineNumberStart)
    {
        _testFilePath = testFilePath;
        _actionName = actionName;
        _lineNumberStart = lineNumberStart;
    }

    /// <summary>
    ///     Gets the line number where error happened.
    /// </summary>
    /// <returns>the line number</returns>
    public long LineNumberStart => _lineNumberStart;

    /// <summary>
    ///     Gets the line number where failing action ends.
    /// </summary>
    /// <returns>the toLineNumber</returns>
    public long LineNumberEnd => _lineNumberEnd;

    /// <summary>
    ///     Gets the test file path for the failed test.
    /// </summary>
    /// <returns>the testFilePath</returns>
    public string TestFilePath => _testFilePath;

    /// <summary>
    ///     Constructs the stack trace message.
    /// </summary>
    /// <returns>the stack trace message.</returns>
    public string GetStackMessage()
    {
        if (_lineNumberEnd > 0 && _lineNumberStart != _lineNumberEnd)
        {
            return $"at {_testFilePath}({_actionName}:{_lineNumberStart}-{_lineNumberEnd})";
        }

        return $"at {_testFilePath}({_actionName}:{_lineNumberStart})";
    }

    /// <summary>
    ///     Sets the line number where failing action ends.
    /// </summary>
    /// <param name="toLineNumber">the toLineNumber to set</param>
    public void SetLineNumberEnd(long toLineNumber)
    {
        _lineNumberEnd = toLineNumber;
    }
}
