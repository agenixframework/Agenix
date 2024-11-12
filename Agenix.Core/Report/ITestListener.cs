using System;

namespace Agenix.Core.Report;

/// Interface representing a test listener. Invoked at various stages of the test lifecycle.
/// /
public interface ITestListener
{
 /// Invoked when test gets started
 /// @param test Test case to be started
 /// /
 void OnTestStart(ITestCase test);

 /// Invoked when test gets finished
 /// @param test Test case that has finished
 void OnTestFinish(ITestCase test);

 /// Invoked when test finished with success
 /// <param name="test">Test case that finished successfully</param>
 void OnTestSuccess(ITestCase test);

 /// Invoked when test finished with failure
 /// <param name="test">Test case that finished with failure</param>
 /// <param name="cause">Exception that caused the test failure</param>
 void OnTestFailure(ITestCase test, Exception cause);

 /// Invoked when test is skipped
 /// <param name="test">Test case that is being skipped</param>
 void OnTestSkipped(ITestCase test);
}