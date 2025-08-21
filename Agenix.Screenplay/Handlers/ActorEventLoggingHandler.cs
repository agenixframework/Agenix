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

using System.Reflection;
using Agenix.Api.Log;
using Agenix.Screenplay.Annotations;
using Agenix.Screenplay.Events;
using Agenix.Screenplay.Utils;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Agenix.Screenplay.Handlers;

/// <summary>
///     Handles logging of actor performance events with hierarchical indentation and step annotation support.
/// </summary>
public class ActorEventLoggingHandler :
    INotificationHandler<ActorBeginsPerformanceEvent>,
    INotificationHandler<ActorEndsPerformanceEvent>,
    INotificationHandler<ActorPerforms>,
    INotificationHandler<ActorAsksQuestion>
{
    private const string AnsiGreen = "\u001B[32m";
    private const string AnsiReset = "\u001B[0m";

    private static readonly ThreadLocal<int> Level = new(() => 0);
    private readonly ILogger _logger = LogManager.GetLogger("agenix-screenplay");

    /// <summary>
    ///     Handles the <see cref="IActorAsksQuestionEvent" /> notification.
    ///     Logs the details of the actor's question, including the question subject.
    /// </summary>
    /// <param name="notification">The event metadata containing information about the question being asked.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Handle(ActorAsksQuestion notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.QuestionSubject))
        {
            return Task.CompletedTask;
        }

        var stepTitle = notification.QuestionSubject.Replace("{0}", notification.Name);
        var logMessage = $"{AnsiGreen}{GetIndentation(Level.Value)} {stepTitle}{AnsiReset}";
        _logger.LogInformation(logMessage);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the <see cref="ActorBeginsPerformanceEvent" /> notification.
    ///     Updates the current actor's name and increments the performance level.
    /// </summary>
    /// <param name="notification">The event metadata containing the actor's information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Handle(ActorBeginsPerformanceEvent notification, CancellationToken cancellationToken)
    {
        Level.Value++;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the <see cref="ActorEndsPerformanceEvent" /> notification.
    ///     Decrements the performance level when an actor completes a performance.
    /// </summary>
    /// <param name="notification">The event metadata containing details about the actor's concluded performance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Handle(ActorEndsPerformanceEvent notification, CancellationToken cancellationToken)
    {
        Level.Value--;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Handles the <see cref="ActorPerforms" /> notification.
    ///     Logs the actor's performance step with hierarchical indentation formatting.
    /// </summary>
    /// <param name="notification">The event metadata including details of the performable action and the actor.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task Handle(ActorPerforms notification, CancellationToken cancellationToken)
    {
        var stepTitle = GetStepTitle(notification.Performable);
        stepTitle = stepTitle.Replace("{0}", notification.Name);

        var logMessage = $"{AnsiGreen}{GetIndentation(Level.Value)} {stepTitle}{AnsiReset}";
        _logger.LogInformation(logMessage);

        return Task.CompletedTask;
    }

    private string GetStepTitle(IPerformable performable)
    {
        return GetTitleFromParent(performable);
    }

    private string GetTitleFromParent(IPerformable performable)
    {
        try
        {
            var performAsMethod =
                // First check the actual type for async first, then sync
                performable.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "PerformAsAsync" && m.GetCustomAttribute<StepAttribute>() != null)
                ?? performable.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == "PerformAs" && m.GetCustomAttribute<StepAttribute>() != null);

            // If not found, check the base type
            if (performAsMethod == null)
            {
                var baseMethods = performable.GetType().BaseType
                    ?.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                if (baseMethods != null)
                {
                    performAsMethod = baseMethods.FirstOrDefault(m =>
                                          m.Name == "PerformAsAsync" && m.GetCustomAttribute<StepAttribute>() != null)
                                      ?? baseMethods.FirstOrDefault(m =>
                                          m.Name == "PerformAs" && m.GetCustomAttribute<StepAttribute>() != null);
                }
            }

            if (performAsMethod != null)
            {
                var stepName = GetStepNameFromAnnotation(performAsMethod);
                if (!string.IsNullOrEmpty(stepName))
                {
                    var fieldValues = GetFieldValuesFrom(performable);
                    return ReplaceFieldTokensWithValues(stepName, fieldValues);
                }
            }

            return NameConverter.Humanize(performable.GetType().Name);
        }
        catch (Exception)
        {
            return NameConverter.Humanize(performable.GetType().Name);
        }
    }

    private string GetStepNameFromAnnotation(MethodInfo method)
    {
        // Look for the Step attribute or similar annotation
        var stepAttribute = method.GetCustomAttribute<StepAttribute>();
        return stepAttribute?.Value ?? string.Empty;
    }

    private Dictionary<string, object> GetFieldValuesFrom(IPerformable performable)
    {
        var fieldValues = new Dictionary<string, object>();
        var allFields = GetAllFields(performable.GetType());

        foreach (var field in allFields)
        {
            try
            {
                var value = field.GetValue(performable);
                fieldValues[field.Name] = value?.ToString() ?? string.Empty;
            }
            catch (Exception)
            {
                // Skip fields that can't be accessed
            }
        }

        return fieldValues;
    }

    private List<FieldInfo> GetAllFields(Type type)
    {
        var fields = new List<FieldInfo>();
        var currentType = type;

        while (currentType != null)
        {
            fields.AddRange(currentType.GetFields(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly));
            currentType = currentType.BaseType;
        }

        return fields;
    }

    private string ReplaceFieldTokensWithValues(string description, Dictionary<string, object> fields)
    {
        return fields.Keys.Aggregate(description, (current, field) =>
            ReplaceField.In(current).TheFieldCalled(field).With(fields[field]));
    }

    private string GetIndentation(int level)
    {
        if (level <= 0)
        {
            return "|";
        }

        return "|" + new string(' ', level * 2) + "-"; // Use spaces instead of dashes
    }
}
