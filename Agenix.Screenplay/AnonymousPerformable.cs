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
    }

    public AnonymousPerformable(string title, List<IPerformable> steps)
    {
        _title = title;
        _steps = steps;
    }

    public IDictionary<string, object> CustomFieldValues =>
        new Dictionary<string, object>(_fieldValues);

    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.AttemptsTo(_steps.ToArray());
    }

    public void SetFieldValue(string fieldName, object fieldValue)
    {
        _fieldValues[fieldName] = fieldValue;
    }

    public override string ToString()
    {
        return _title;
    }
}
