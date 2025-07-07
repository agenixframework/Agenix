#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


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
using Agenix.Api.Log;
using Agenix.Selenium.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Selenium action for storing a file in the browser's temporary storage.
/// </summary>
public class StoreFileAction : AbstractSeleniumAction
{
    /// <summary>
    ///     Logger instance used for logging messages related to the StoreFileAction class.
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(StoreFileAction));

    /// <summary>
    ///     The file path to store.
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    ///     Default constructor.
    /// </summary>
    /// <param name="builder">The action builder</param>
    public StoreFileAction(Builder builder) : base("store-file", builder)
    {
        _filePath = builder._filePath;
    }

    /// <summary>
    ///     Gets the file path.
    /// </summary>
    public string FilePath => _filePath;

    /// <summary>
    ///     Executes the store file action.
    /// </summary>
    /// <param name="browser">The Selenium browser instance</param>
    /// <param name="context">The test context</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        var resolvedFilePath = context.ReplaceDynamicContentInString(_filePath);
        Logger.LogDebug("Storing file: {FilePath}", resolvedFilePath);

        var storedPath = browser.StoreFile(resolvedFilePath);
        Logger.LogInformation("File stored successfully at: {StoredPath}", storedPath);
    }

    /// <summary>
    ///     Builder for creating StoreFileAction instances.
    /// </summary>
    public class Builder : Builder<StoreFileAction, Builder>
    {
        internal string _filePath { get; private set; }

        /// <summary>
        ///     Sets the file path to store.
        /// </summary>
        /// <param name="filePath">The file path</param>
        /// <returns>This builder instance</returns>
        public Builder FilePath(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            return this;
        }

        /// <summary>
        ///     Builds the StoreFileAction instance.
        /// </summary>
        /// <returns>A configured StoreFileAction</returns>
        public override StoreFileAction Build()
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                throw new InvalidOperationException("File path must be specified");
            }

            return new StoreFileAction(this);
        }
    }
}
