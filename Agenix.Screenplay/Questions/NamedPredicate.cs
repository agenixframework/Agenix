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
///     A predicate with a name that can be used for better readability and debugging.
/// </summary>
/// <typeparam name="T">The type of the input to the predicate</typeparam>
/// <summary>
///     A predicate with a name that can be used for better readability and debugging.
/// </summary>
/// <typeparam name="T">The type of the input to the predicate</typeparam>
public class NamedPredicate<T>(string name, Func<T, bool> predicate)
{
    /// <summary>
    ///     Gets the name of the predicate, providing a human-readable identifier for the <see cref="NamedPredicate{T}" />
    ///     instance.
    /// </summary>
    /// <value>
    ///     A string representing the name of the predicate.
    /// </value>
    public string Name => name;

    /// <summary>
    ///     Gets the underlying predicate function wrapped by the <see cref="NamedPredicate{T}" /> class.
    /// </summary>
    /// <value>
    ///     The delegate of type <see cref="Func{T, bool}" /> representing the underlying predicate logic.
    /// </value>
    public Func<T, bool> InnerPredicate => predicate;

    /// <summary>
    ///     Allows the NamedPredicate to be called directly like a function.
    /// </summary>
    /// <param name="obj">The input parameter to evaluate the predicate against.</param>
    /// <returns>true if the input satisfies the predicate; otherwise, false.</returns>
    public bool this[T obj] => predicate(obj);

    /// <summary>
    ///     Returns a string representation of the NamedPredicate object.
    /// </summary>
    /// <returns>The name of the predicate as a string.</returns>
    public override string ToString()
    {
        return name;
    }

    /// <summary>
    ///     Evaluates the predicate against the specified input.
    /// </summary>
    /// <param name="obj">The input parameter to evaluate the predicate against.</param>
    /// <returns>true if the input satisfies the predicate; otherwise, false.</returns>
    public bool Invoke(T obj)
    {
        return predicate(obj);
    }

    /// <summary>
    ///     Combines the current predicate with another predicate using a logical AND operation.
    /// </summary>
    /// <typeparam name="T">The type of the input to the predicate.</typeparam>
    /// <param name="other">The other predicate to combine with the current predicate.</param>
    /// <returns>
    ///     A new instance of <see cref="NamedPredicate{T}" /> representing the logical AND of the current and other
    ///     predicates.
    /// </returns>
    public NamedPredicate<T> And(Func<T, bool> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) && other(x));
    }

    /// <summary>
    ///     Negates the current predicate, creating a new predicate that returns the logical NOT of the original predicate.
    /// </summary>
    /// <typeparam name="T">The type of the input to the predicate.</typeparam>
    /// <returns>A new instance of <see cref="NamedPredicate{T}" /> representing the logical NOT of the current predicate.</returns>
    public NamedPredicate<T> Not()
    {
        return new NamedPredicate<T>(name, x => !predicate(x));
    }

    /// <summary>
    ///     Combines the current predicate with another predicate using a logical OR operation.
    /// </summary>
    /// <typeparam name="T">The type of the input to the predicate.</typeparam>
    /// <param name="other">The other predicate to combine with the current predicate.</param>
    /// <returns>
    ///     A new instance of <see cref="NamedPredicate{T}" /> representing the logical OR of the current and other
    ///     predicates.
    /// </returns>
    public NamedPredicate<T> Or(Func<T, bool> other)
    {
        return new NamedPredicate<T>(name, x => predicate(x) || other(x));
    }

    /// <summary>
    ///     Implicitly converts a NamedPredicate to a Func&lt;T, bool&gt;.
    /// </summary>
    /// <param name="namedPredicate">The NamedPredicate to convert.</param>
    /// <returns>The underlying predicate function.</returns>
    public static implicit operator Func<T, bool>(NamedPredicate<T> namedPredicate)
    {
        return namedPredicate.InnerPredicate;
    }
}
