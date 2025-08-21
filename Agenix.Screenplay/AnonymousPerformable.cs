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

namespace Agenix.Screenplay;

/// <summary>
///     A performable task that can be created anonymously with a title and a list of steps.
/// </summary>
public class AnonymousPerformable : IPerformable, IHasCustomFieldValues
{
    private readonly Dictionary<string, object> _fieldValues = new();
    private readonly List<IPerformable> _steps;
    private readonly string _title;

    public AnonymousPerformable()
    {
        _steps = [];
    }

    public AnonymousPerformable(string title, List<IPerformable> steps)
    {
        _title = title;
        _steps = steps;
    }

    /// <summary>
    ///     Gets the collection of custom field values associated with the instance.
    ///     This property provides access to key-value pairs, where the key is the field name
    ///     and the value is the corresponding data.
    /// </summary>
    public IDictionary<string, object> CustomFieldValues => new Dictionary<string, object>(_fieldValues);

    /// <summary>
    ///     Executes the series of performable tasks asynchronously for the given actor.
    /// </summary>
    /// <typeparam name="T">The type of the actor performing the tasks, which must derive from <see cref="Actor" />.</typeparam>
    /// <param name="actor">The actor executing the tasks.</param>
    /// <param name="cancellationToken">
    ///     An optional token to observe while waiting for the task to complete. If cancellation is
    ///     requested, the operation will be terminated.
    /// </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PerformAsAsync<T>(T actor, CancellationToken cancellationToken = default) where T : Actor
    {
        if (cancellationToken.IsCancellationRequested)
        {
            await Task.FromCanceled(cancellationToken);
        }

        await actor.AttemptsToAsync(_steps.ToArray());
    }

    /// <summary>
    ///     Sets the value of a custom field identified by a specific name for this instance of
    ///     <see cref="AnonymousPerformable" />.
    /// </summary>
    /// <param name="fieldName">The name of the field whose value is to be set.</param>
    /// <param name="fieldValue">The value to assign to the field.</param>
    public void SetFieldValue(string fieldName, object fieldValue)
    {
        _fieldValues[fieldName] = fieldValue;
    }

    /// <summary>
    ///     Returns a string representation of the current <see cref="AnonymousPerformable" /> instance, typically the title.
    /// </summary>
    /// <returns>A string containing the title of this <see cref="AnonymousPerformable" />.</returns>
    public override string ToString()
    {
        return _title;
    }
}
