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

using NHamcrest;

namespace Agenix.Screenplay.Questions;

/// <summary>
/// </summary>
public class QuestionHints
{
    /// <summary>
    ///     Retrieves a collection of types from the provided matcher that implement the IQuestionHint interface,
    ///     excluding the base IQuestionHint interface itself.
    /// </summary>
    /// <param name="matcher">The matcher to analyze for question hint interfaces.</param>
    /// <returns>A set of types that implement IQuestionHint from the provided matcher.</returns>
    public static HashSet<Type> FromAssertion<T>(IMatcher<T> matcher)
    {
        return matcher.GetType()
            .GetInterfaces()
            .Where(interfaceType => typeof(IQuestionHint).IsAssignableFrom(interfaceType))
            .Where(interfaceType => typeof(IQuestionHint) != interfaceType)
            .ToHashSet();
    }

    /// <summary>
    ///     Creates an instance of the HintAdder utility class to associate a set of hints with questions that support hint
    ///     functionality.
    /// </summary>
    /// <param name="hints">The collection of hints to be applied to a question.</param>
    /// <returns>A HintAdder instance for applying the specified hints to a question.</returns>
    public static HintAdder AddHints(HashSet<Type> hints)
    {
        return new HintAdder(hints);
    }

    /// <summary>
    ///     Represents a utility class that allows adding hints to questions that support hints functionality.
    ///     This class is used to apply a collection of hints to a question.
    /// </summary>
    public class HintAdder(HashSet<Type> hints)
    {
        /// <summary>
        ///     Applies the provided hints to the specified question if the question supports hint functionality.
        /// </summary>
        /// <typeparam name="T">The type of the answer that the question provides.</typeparam>
        /// <param name="question">The question to which the hints will be applied, if it supports hint functionality.</param>
        public void To<T>(IQuestion<T> question)
        {
            var acceptsHints = question as IAcceptsHints;
            if (acceptsHints != null)
            {
                var hintInstances = hints
                    .Where(type => !type.IsAbstract && !type.IsInterface)
                    .Select(type =>
                    {
                        try
                        {
                            return (IQuestionHint)Activator.CreateInstance(type)!;
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .Where(hint => hint != null)
                    .ToHashSet();

                acceptsHints.Apply(hintInstances);
            }
        }
    }
}
