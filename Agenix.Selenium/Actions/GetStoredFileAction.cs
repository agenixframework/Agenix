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

using Agenix.Api.Context;
using Agenix.Selenium.Endpoint;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Retrieves a previously stored file and sets its path in test context variables.
/// </summary>
public class GetStoredFileAction : AbstractSeleniumAction
{
    /// <summary>
    ///     File name to look for in the stored files
    /// </summary>
    private readonly string _fileName;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public GetStoredFileAction(Builder builder) : base("get-stored-file", builder)
    {
        _fileName = builder.FileName;
    }

    /// <summary>
    ///     Gets the file name to retrieve
    /// </summary>
    public string FileName => _fileName;

    /// <summary>
    ///     Executes the action to retrieve a stored file using the Selenium browser and updates the test context with its
    ///     path.
    /// </summary>
    /// <param name="browser">The Selenium browser instance used to locate and retrieve the stored file.</param>
    /// <param name="context">The test context where the file path will be stored as a variable.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        var resolvedFileName = context.ReplaceDynamicContentInString(_fileName);
        var filePath = browser.GetStoredFile(resolvedFileName);
        context.SetVariable(SeleniumHeaders.SeleniumDownloadFile, filePath);
    }

    /// <summary>
    ///     Action builder for GetStoredFileAction
    /// </summary>
    public class Builder : Builder<GetStoredFileAction, Builder>
    {
        public string FileName { get; private set; }

        /// <summary>
        ///     Set the file name to retrieve
        /// </summary>
        public Builder SetFileName(string fileName)
        {
            FileName = fileName;
            return self;
        }

        /// <summary>
        ///     Builds and returns an instance of GetStoredFileAction.
        /// </summary>
        /// <returns>The constructed GetStoredFileAction instance.</returns>
        public override GetStoredFileAction Build()
        {
            return new GetStoredFileAction(this);
        }
    }
}
