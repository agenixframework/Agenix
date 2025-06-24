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

namespace Agenix.Api.Util;

/// <summary>
///     Represents a container object which may or may not contain a non-null value.
///     This class is primarily used to avoid null reference checks when working with objects that may or may not contain a
///     value.
/// </summary>
/// <typeparam name="T">The type of the value that this container may hold.</typeparam>
public class Optional<T>
{
    /// <summary>
    ///     Holds the value contained within this instance of <see cref="Optional{T}" /> if a value is present.
    /// </summary>
    private readonly T _value;

    /// <summary>
    ///     Represents a container object which may or may not contain a non-null value.
    ///     This is designed to streamline operations by eliminating direct null reference checks.
    /// </summary>
    /// <typeparam name="T">The type of the value that the container may hold.</typeparam>
    private Optional()
    {
        IsPresent = false;
    }

    /// <summary>
    ///     Represents a container object which may or may not contain a non-null value of type T.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    private Optional(T value)
    {
        _value = value;
        IsPresent = true;
    }

    /// <summary>
    ///     Gets an empty instance of <see cref="Optional{T}" />.
    /// </summary>
    public static Optional<T> Empty => new();

    /// <summary>
    ///     Gets a value indicating whether a value is present in the current instance.
    /// </summary>
    public bool IsPresent { get; }

    /// <summary>
    ///     Gets the value held by the current instance if a value is present.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when attempting to access the value if no value is present.
    /// </exception>
    public T Value => IsPresent ? _value : throw new InvalidOperationException("No value present");

    /// <summary>
    ///     Returns an Optional containing the specified value if it is not null.
    /// </summary>
    /// <param name="value">The value to be wrapped in an Optional. Must not be null.</param>
    /// <returns>An Optional containing the specified value if it is not null; otherwise, throws an ArgumentNullException.</returns>
    public static Optional<T> Of(T value)
    {
        return value != null ? new Optional<T>(value) : throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Returns an Optional containing the specified value if it is not null; otherwise, returns an empty Optional.
    /// </summary>
    /// <param name="value">The value to be wrapped in an Optional if it is not null.</param>
    /// <returns>An Optional containing the specified value if it is not null; otherwise, an empty Optional.</returns>
    public static Optional<T> OfNullable(T value)
    {
        return value != null ? new Optional<T>(value) : Empty;
    }

    /// <summary>
    ///     Returns the contained value if present; otherwise, returns the specified alternative value.
    /// </summary>
    /// <param name="other">An alternative value to be returned if no value is present.</param>
    /// <returns>The contained value if present; otherwise, the specified alternative value.</returns>
    public T OrElse(T other)
    {
        return IsPresent ? _value : other;
    }

    /// <summary>
    ///     Returns the contained value if present; otherwise, returns the result produced by the specified supplier function.
    /// </summary>
    /// <param name="other">A supplier function that produces an alternative value to be returned if no value is present.</param>
    /// <returns>The contained value if present; otherwise, the result of the supplier function.</returns>
    public T OrElseGet(Func<T> other)
    {
        return IsPresent ? _value : other();
    }

    /// <summary>
    ///     Returns the contained value if present, otherwise throws an exception provided by the specified supplier.
    /// </summary>
    /// <param name="exceptionSupplier">A supplier function that provides the exception to be thrown if no value is present.</param>
    /// <returns>The contained value if it is present.</returns>
    /// <exception cref="Exception">The exception provided by the supplier if no value is present.</exception>
    public T OrElseThrow(Func<Exception> exceptionSupplier)
    {
        if (IsPresent)
        {
            return _value;
        }

        throw exceptionSupplier();
    }

    /// <summary>
    ///     Creates an Optional instance containing the specified value.
    /// </summary>
    /// <param name="value">The value to be stored in the Optional instance.</param>
    /// <returns>An Optional instance containing the specified value.</returns>
    public static Optional<T> From(T value)
    {
        return new Optional<T>(value);
    }
}
