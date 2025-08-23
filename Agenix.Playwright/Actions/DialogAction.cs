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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for handling JavaScript dialogs (alerts, confirms, and prompts).
///     This action can accept, dismiss, or interact with browser dialog boxes.
/// </summary>
public class DialogAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Enumeration of dialog action types.
    /// </summary>
    public enum DialogActionType
    {
        /// <summary>
        ///     Accept the dialog (click OK).
        /// </summary>
        ACCEPT,

        /// <summary>
        ///     Dismiss the dialog (click Cancel).
        /// </summary>
        DISMISS,

        /// <summary>
        ///     Get the dialog text.
        /// </summary>
        GET_TEXT,

        /// <summary>
        ///     Send keys to the dialog (for prompts).
        /// </summary>
        SEND_KEYS
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DialogAction));

    private readonly string? _text;

    /// <summary>
    ///     Provides a Playwright action for managing JavaScript dialogs, including alerts, confirms, and prompts.
    ///     Enables actions such as accepting, dismissing, retrieving text, or sending input to dialog boxes.
    /// </summary>
    public DialogAction(Builder builder) : this("dialog", builder) { }

    /// <summary>
    ///     Provides a Playwright action for handling JavaScript dialogs such as alerts, confirms, and prompts.
    /// </summary>
    public DialogAction(string name, Builder builder) : base(name, builder)
    {
        ActionType = builder.ActionType;
        _text = builder.Text;
        WaitForDialog = builder.WaitForDialog;
        TimeoutMs = builder.TimeoutMs;
    }

    /// <summary>
    ///     Gets the type of dialog action to be performed (e.g., accept, dismiss, retrieve text, or send input).
    ///     This property is used to define the specific action type for handling JavaScript dialogs such as alerts, confirms,
    ///     and prompts.
    /// </summary>
    public DialogActionType ActionType { get; }

    /// <summary>
    ///     Gets the text to be sent as input in a JavaScript dialog, such as a prompt.
    ///     This property is applicable when the action type is set to sending input.
    /// </summary>
    public string Text => _text;

    /// <summary>
    ///     Gets the timeout value in milliseconds for the dialog action.
    ///     This property specifies the maximum time to wait for the dialog interaction to complete.
    /// </summary>
    public int TimeoutMs { get; }

    /// <summary>
    ///     Determines whether the action should wait for a JavaScript dialog to appear before executing other operations.
    ///     If set to true, the action will pause and wait for the dialog to open before continuing execution.
    ///     This property is useful when handling dialog boxes that may appear during a specific step in a test sequence.
    /// </summary>
    public bool WaitForDialog { get; }

    /// <summary>
    ///     Executes the dialog action within the context of the provided browser and test context.
    /// </summary>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context containing state and configuration for the test execution</param>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogInformation("Executing dialog action: {ActionType}", ActionType);

            var page = browser.GetCurrentPage();
            if (page == null)
            {
                throw new InvalidOperationException("No active page available");
            }

            // For GetText action, we need to handle it differently
            if (ActionType == DialogActionType.GET_TEXT)
            {
                await HandleGetTextDialog(page, context);
                return;
            }

            // Set up a dialog handler for other actions
            var tcs = new TaskCompletionSource<bool>();

            void DialogHandler(object? sender, IDialog dialog)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Logger.LogDebug("Dialog detected - Type: {Type}, Message: {Message}", dialog.Type,
                            dialog.Message);

                        switch (ActionType)
                        {
                            case DialogActionType.ACCEPT:
                                await dialog.AcceptAsync(_text);
                                Logger.LogDebug("Dialog accepted with text: {Text}", _text ?? "(no text)");
                                break;

                            case DialogActionType.DISMISS:
                                await dialog.DismissAsync();
                                Logger.LogDebug("Dialog dismissed");
                                break;

                            case DialogActionType.SEND_KEYS:
                                await dialog.AcceptAsync(_text);
                                Logger.LogDebug("Dialog accepted with input text: {Text}", _text);
                                break;

                            default:
                                await dialog.DismissAsync();
                                Logger.LogWarning("Unknown dialog action type: {ActionType}, dismissing dialog",
                                    ActionType);
                                break;
                        }

                        _ = true;
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error handling dialog");
                        tcs.SetException(ex);
                    }
                });
            }

            page.Dialog += DialogHandler;

            try
            {
                // If we need to wait for a dialog, wait for it
                if (WaitForDialog)
                {
                    Logger.LogDebug("Waiting for dialog to appear");

                    using var cts = new CancellationTokenSource(TimeoutMs);
                    try
                    {
                        await tcs.Task.WaitAsync(cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        throw new TimeoutException($"Dialog did not appear within {TimeoutMs}ms");
                    }
                }

                Logger.LogInformation("Dialog action completed successfully");
            }
            finally
            {
                page.Dialog -= DialogHandler;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute dialog action");
            throw new AgenixSystemException("Failed to execute dialog action", ex);
        }
    }

    /// <summary>
    ///     Handles getting text from a dialog.
    /// </summary>
    /// <param name="page">The page instance</param>
    /// <param name="context">The test context</param>
    private async Task HandleGetTextDialog(IPage page, TestContext context)
    {
        Logger.LogDebug("Setting up dialog text capture");

        string dialogText;
        var tcs = new TaskCompletionSource<string>();

        page.Dialog += DialogHandler;

        try
        {
            if (WaitForDialog)
            {
                Logger.LogDebug("Waiting for dialog to capture text");

                using var cts = new CancellationTokenSource(TimeoutMs);
                try
                {
                    dialogText = await tcs.Task.WaitAsync(cts.Token);
                    // At this point, the context variable should already be set in the handler
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException($"Dialog did not appear within {TimeoutMs}ms");
                }
            }

            // If we're not waiting for dialog, the context variable will be set when the dialog actually appears
            // No need to set it here since it will be empty
        }
        finally
        {
            page.Dialog -= DialogHandler;
        }

        return;

        void DialogHandler(object? sender, IDialog dialog)
        {
            Task.Run(async () =>
            {
                try
                {
                    dialogText = dialog.Message;
                    Logger.LogDebug("Dialog text captured: {Text}", dialogText);

                    // Dismiss the dialog after capturing text
                    await dialog.DismissAsync();
                    _ = true;

                    // Store dialog text in context immediately after capturing
                    if (!string.IsNullOrEmpty(dialogText))
                    {
                        context.SetVariable("DIALOG_TEXT", dialogText);
                        Logger.LogDebug("Dialog text stored in context: {Text}", dialogText);
                    }

                    tcs.SetResult(dialogText);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error capturing dialog text");
                    tcs.SetException(ex);
                }
            });
        }
    }

    /// <summary>
    ///     Builder class for creating DialogAction instances with fluent API.
    /// </summary>
    public class Builder : Builder<DialogAction, Builder>
    {
        internal DialogActionType ActionType { get; private set; } = DialogActionType.ACCEPT;
        internal string? Text { get; private set; }
        internal bool WaitForDialog { get; private set; }
        internal int TimeoutMs { get; private set; } = 5000;

        /// <summary>
        ///     Sets the action to accept the dialog.
        /// </summary>
        /// <param name="text">Optional text to send when accepting (for prompts)</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Accept(string? text = null)
        {
            ActionType = DialogActionType.ACCEPT;
            Text = text;
            return this;
        }

        /// <summary>
        ///     Sets the action to dismiss the dialog.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder Dismiss()
        {
            ActionType = DialogActionType.DISMISS;
            Text = null;
            return this;
        }

        /// <summary>
        ///     Sets the action to get the dialog text.
        /// </summary>
        /// <returns>The builder instance for method chaining</returns>
        public Builder GetText()
        {
            ActionType = DialogActionType.GET_TEXT;
            Text = null;
            return this;
        }

        /// <summary>
        ///     Sets the action to send keys to the dialog (for prompts).
        /// </summary>
        /// <param name="text">The text to send to the prompt</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder SendKeys(string text)
        {
            ActionType = DialogActionType.SEND_KEYS;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            return this;
        }

        /// <summary>
        ///     Sets whether to wait for the dialog to appear.
        /// </summary>
        /// <param name="wait">True to wait for dialog, false otherwise</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WaitForDialogToAppear(bool wait = true)
        {
            WaitForDialog = wait;
            return this;
        }

        /// <summary>
        ///     Sets the timeout for waiting for the dialog to appear.
        /// </summary>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>The builder instance for method chaining</returns>
        public Builder WithTimeout(int timeoutMs)
        {
            TimeoutMs = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Builds the DialogAction instance.
        /// </summary>
        /// <returns>A new DialogAction instance</returns>
        public override DialogAction Build()
        {
            return new DialogAction(this);
        }
    }
}
