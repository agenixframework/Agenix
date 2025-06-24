#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System;
using System.Collections.Generic;
using System.Xml;
using Agenix.Api;
using Agenix.Api.Container;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Report;
using Agenix.Core.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Report;

/// <summary>
///     Specialized test listener responsible for handling test failures
///     and populating the failure stack for failed test cases.
/// </summary>
public class FailureStackTestListener : AbstractTestListener
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixSettings));

    public override void OnTestFailure(ITestCase test, Exception cause)
    {
        if (cause is AgenixSystemException AgenixException)
        {
            AgenixException.SetFailureStack(GetFailureStack(test));
        }
    }

    /// <summary>
    ///     Get failure stack from given test.
    /// </summary>
    /// <param name="test"></param>
    /// <returns></returns>
    public static List<FailureStackElement> GetFailureStack(ITestCase test)
    {
        var failureStack = new List<FailureStackElement>();

        try
        {
            var testFilePath = test.GetNamespaceName().Replace('.', '/') + "/" + test.Name;

            var testFileResource = FileUtils.GetFileResource(testFilePath + FileUtils.FILE_EXTENSION_XML);

            if (!testFileResource.Exists)
            {
                return failureStack;
            }

            // first check if the test failed during setup
            if (test.GetActiveAction() == null)
            {
                failureStack.Add(new FailureStackElement(testFilePath, "init", 0L));
                // no actions were executed, yet failure caused by test setup: abort
                return failureStack;
            }

            var readerSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };

            using var stream = testFileResource.InputStream;
            using var xmlReader = XmlReader.Create(stream, readerSettings);

            var contentHandler = new FailureStackContentHandler(failureStack, test, testFilePath);
            contentHandler.Parse(xmlReader);
        }
        catch (Exception e)
        {
            Log.LogWarning(e, "Failed to locate line numbers for failure stack trace");
        }

        return failureStack;
    }

    /// <summary>
    ///     Special content handler responsible for filling the failure stack.
    /// </summary>
    private sealed class FailureStackContentHandler
    {
        /// <summary>
        ///     The failure stack to work on
        /// </summary>
        private readonly List<FailureStackElement> _failureStack;

        /// <summary>
        ///     The actual test case
        /// </summary>
        private readonly ITestCase _test;

        /// <summary>
        ///     The test file path
        /// </summary>
        private readonly string _testFilePath;

        /// <summary>
        ///     The name of action which caused the error
        /// </summary>
        private string _failedActionName;

        /// <summary>
        ///     Start/stop to listen for the error line ending
        /// </summary>
        private bool _findLineEnding;

        /// <summary>
        ///     Line info providing actual line number information
        /// </summary>
        private IXmlLineInfo _lineInfo;

        /// <summary>
        ///     Failure stack finder
        /// </summary>
        private FailureStackFinder _stackFinder;

        /// <summary>
        ///     Default constructor using fields.
        /// </summary>
        /// <param name="failureStack"></param>
        /// <param name="test"></param>
        /// <param name="testFilePath"></param>
        public FailureStackContentHandler(List<FailureStackElement> failureStack, ITestCase test, string testFilePath)
        {
            _failureStack = failureStack;
            _test = test;
            _testFilePath = testFilePath;
        }

        public void Parse(XmlReader reader)
        {
            _lineInfo = reader as IXmlLineInfo;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        StartElement(reader.LocalName);
                        break;
                    case XmlNodeType.EndElement:
                        EndElement(reader.LocalName);
                        break;
                }
            }
        }

        private void StartElement(string qName)
        {
            //start when an action element is reached
            if (qName.Equals("actions"))
            {
                _stackFinder = new FailureStackFinder(_test);
                return;
            }

            if (_stackFinder == null || !_stackFinder.IsFailureStackElement(qName))
            {
                return;
            }

            var lineNumber = GetCurrentLineNumber();
            _failureStack.Add(new FailureStackElement(_testFilePath, qName, lineNumber));

            if (_stackFinder.GetNestedActionContainer() != null &&
                _stackFinder.GetNestedActionContainer().GetActiveAction() != null)
            {
                //continue with nested action container, to find out which action caused the failure
                _stackFinder = new FailureStackFinder(_stackFinder.GetNestedActionContainer());
            }
            else
            {
                //stop failure stack evaluation as failure-causing action was found
                _stackFinder = null;

                //now start to find ending line number
                _findLineEnding = true;
                _failedActionName = qName;
            }
        }

        private void EndElement(string qName)
        {
            if (_findLineEnding && qName.Equals(_failedActionName))
            {
                // get last failure stack element
                var failureStackElement = _failureStack[_failureStack.Count - 1];
                var endLineNumber = GetCurrentLineNumber();
                failureStackElement.SetLineNumberEnd(endLineNumber);
                _findLineEnding = false;
            }
        }

        /// <summary>
        ///     Gets the current line number from the XML reader
        /// </summary>
        /// <returns>Current line number or 0 if line info is not available</returns>
        private long GetCurrentLineNumber()
        {
            return _lineInfo?.HasLineInfo() == true ? _lineInfo.LineNumber : 0L;
        }
    }


    /// <summary>
    ///     Failure stack finder listens for actions in a testcase
    /// </summary>
    private class FailureStackFinder
    {
        /// <summary>
        ///     Action list
        /// </summary>
        private readonly Stack<ITestAction> _actionStack = new();

        /// <summary>
        ///     Test action we are currently working on
        /// </summary>
        private ITestAction _action;

        /// <summary>
        ///     Default constructor using fields.
        /// </summary>
        /// <param name="container"></param>
        public FailureStackFinder(ITestActionContainer container)
        {
            var lastActionIndex = container.GetActionIndex(container.GetActiveAction());

            for (var i = lastActionIndex; i >= 0; i--)
            {
                _actionStack.Push(container.GetActions()[i]);
            }
        }

        /// <summary>
        ///     Checks whether the target action is reached within the action container.
        ///     Method counts the actions inside the action container and waits for the target index
        ///     to be reached.
        /// </summary>
        /// <param name="eventElement">
        ///     actual action name can also be a nested element in the XML DOM tree so check name before
        ///     evaluation
        /// </param>
        /// <returns>boolean flag to mark that target action is reached or not</returns>
        public bool IsFailureStackElement(string eventElement)
        {
            if (_action == null)
            {
                _action = _actionStack.Pop();
            }

            /* filter method calls that actually are based on other elements within the DOM
             * tree. SAX content handler cannot differ between action elements and other nested elements
             * in startElement event.
             */
            if (eventElement.Equals(_action.Name))
            {
                if (_action is ITestActionContainer container && _actionStack.Count > 0)
                {
                    for (var i = container.GetActions().Count - 1; i >= 0; i--)
                    {
                        _actionStack.Push(container.GetActions()[i]);
                    }
                }

                if (_actionStack.Count > 0)
                {
                    _action = null;
                }
            }
            else
            {
                return false;
            }

            return _actionStack.Count == 0;
        }

        /// <summary>
        ///     Is the target action a container itself? If yes the stack evaluation should
        ///     continue with the nested container to get the nested action that caused the failure.
        /// </summary>
        /// <returns>the nested container or null</returns>
        public ITestActionContainer GetNestedActionContainer()
        {
            if (_action is ITestActionContainer container)
            {
                return container;
            }

            return null;
        }
    }
}
