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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Convenience class to convert an object into a Question
/// </summary>
public static class TheValue
{
    /// <summary>
    ///     Creates a question that represents the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value that the question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the provided value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(TAnswer value)
    {
        return new SimpleQuestion<TAnswer>(actor => value);
    }

    /// <summary>
    ///     Creates a question representing the given value.
    /// </summary>
    /// <typeparam name="TAnswer">The type of the value this question will produce.</typeparam>
    /// <param name="value">The value to represent as a question.</param>
    /// <returns>An IQuestion instance containing the supplied value.</returns>
    public static IQuestion<TAnswer> Of<TAnswer>(string subject, TAnswer value)
    {
        return new QuestionWithDefinedSubject<TAnswer>(Of(value), subject);
    }
}

internal class SimpleQuestion<TAnswer> : IQuestion<TAnswer>
{
    private readonly Func<Actor, TAnswer> _answerFunction;

    public SimpleQuestion(Func<Actor, TAnswer> answerFunction)
    {
        _answerFunction = answerFunction;
    }

    public TAnswer AnsweredBy(Actor actor)
    {
        return _answerFunction(actor);
    }
}
