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

using System.Text.Json;
using Agenix.Api.Context;
using Agenix.Selenium.Endpoint;
using OpenQA.Selenium;

namespace Agenix.Selenium.Actions;

/// <summary>
///     Fill out form with given key-value pairs where each key is used to find the form field.
///     Sets field values with set input action that supports both input and select form controls.
///     Supports to submit the form after all fields are set.
/// </summary>
public class FillFormAction : AbstractSeleniumAction
{
    /// <summary>
    ///     Key value pairs representing the form fields to fill
    /// </summary>
    private readonly Dictionary<By, string> _formFields;

    /// <summary>
    ///     Optional submit button that gets clicked after fields are filled
    /// </summary>
    private readonly By? _submitButton;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public FillFormAction(Builder builder) : base("fill-form", builder)
    {
        _formFields = builder.FormFields;
        _submitButton = builder.SubmitButton;
    }

    /// <summary>
    ///     Gets the form fields
    /// </summary>
    public Dictionary<By, string> FormFields => _formFields;

    /// <summary>
    ///     Gets the submit button
    /// </summary>
    public By SubmitButton => _submitButton;

    /// <summary>
    ///     Executes the action to fill out the form using the specified fields and optionally submits the form.
    /// </summary>
    /// <param name="browser">The Selenium browser instance used for form interaction.</param>
    /// <param name="context">The test execution context containing information about the current test state.</param>
    protected override void Execute(SeleniumBrowser browser, TestContext context)
    {
        foreach (var setInputAction in _formFields.Select(field => new SetInputAction.Builder()
                     .WithBrowser(browser)
                     .SetValue(field.Value)
                     .Element(field.Key)
                     .Build()))
        {
            setInputAction.Execute(context);
        }

        if (_submitButton == null)
        {
            return;
        }

        var clickAction = new ClickAction.Builder()
            .WithBrowser(browser)
            .Element(_submitButton)
            .Build();

        clickAction.Execute(context);
    }

    /// <summary>
    ///     Action builder for FillFormAction
    /// </summary>
    public class Builder : Builder<FillFormAction, Builder>
    {
        public Dictionary<By, string> FormFields { get; } = new();
        public By SubmitButton { get; private set; }

        /// <summary>
        ///     Add a form field by By selector and value
        /// </summary>
        public Builder Field(By by, string value)
        {
            FormFields[by] = value;
            return self;
        }

        /// <summary>
        ///     Add a form field by ID and value
        /// </summary>
        public Builder Field(string id, string value)
        {
            return Field(By.Id(id), value);
        }

        /// <summary>
        ///     Set form fields from JSON string
        /// </summary>
        public Builder FromJson(string formFieldsJson)
        {
            var fieldsDict = JsonSerializer.Deserialize<Dictionary<string, string>>(formFieldsJson);
            return Fields(fieldsDict);
        }

        /// <summary>
        ///     Set default submit button (input[type='submit'])
        /// </summary>
        public Builder Submit()
        {
            SubmitButton = By.XPath("//input[@type='submit']");
            return self;
        }

        /// <summary>
        ///     Set submit button by ID
        /// </summary>
        public Builder Submit(string id)
        {
            return Submit(By.Id(id));
        }

        /// <summary>
        ///     Set submit button by By selector
        /// </summary>
        public Builder Submit(By button)
        {
            SubmitButton = button;
            return self;
        }

        /// <summary>
        ///     Add multiple fields from dictionary
        /// </summary>
        public Builder Fields(Dictionary<string, string> fields)
        {
            foreach (var field in fields)
            {
                Field(field.Key, field.Value);
            }

            return self;
        }

        /// <summary>
        /// Builds and returns an instance of <see cref="FillFormAction"/>.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="FillFormAction"/> configured using the builder.
        /// </returns>
        public override FillFormAction Build()
        {
            return new FillFormAction(this);
        }
    }
}
