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

using Agenix.Api.Util;

namespace Agenix.Screenplay;

/// <summary>
///     Provides a factory for creating instrumented instances of a specified type.
///     Enables dynamic instantiation and customization of object construction,
///     which is particularly useful in contexts such as dependency injection or automated testing.
/// </summary>
public class Instrumented
{
    /// <summary>
    ///     Creates an instrumented builder for the specified type, allowing
    ///     dynamic instantiation and configuration of its objects.
    /// </summary>
    /// <returns>
    ///     An instance of <see cref="Instrumented.InstrumentedBuilder{T}" /> to facilitate the creation
    ///     and configuration of objects of the specified type.
    /// </returns>
    public static InstrumentedBuilder<T> InstanceOf<T>()
    {
        return new InstrumentedBuilder<T>(typeof(T));
    }

    public static InstrumentedBuilder<T> InstanceOf<T>(Type type)
    {
        return new InstrumentedBuilder<T>(type);
    }

    /// <summary>
    ///     Represents a builder for creating and configuring instrumented instances of a specified type.
    ///     Allows customization of constructor parameters to dynamically instantiate objects.
    ///     This class is typically used to facilitate dependency injection or dynamic testing scenarios.
    /// </summary>
    /// <typeparam name="T">The type of the object this builder is designed to instantiate.</typeparam>
    public class InstrumentedBuilder<T>(Type instanceType, object[] constructorParameters)
    {
        public InstrumentedBuilder(Type instanceType)
            : this(instanceType, [])
        {
        }

        /// <summary>
        ///     Creates a new instance of the specified type using its zero-argument constructor
        ///     and any provided constructor parameters.
        /// </summary>
        /// <returns>
        ///     A newly instantiated object of the specified type.
        /// </returns>
        public T NewInstance()
        {
            return (T)ObjectUtils.InstantiateType(ObjectUtils.GetZeroArgConstructorInfo(instanceType),
                constructorParameters);
        }

        /// <summary>
        ///     Creates a new instance of the object type with updated properties by utilizing the specified constructor
        ///     parameters.
        /// </summary>
        /// <param name="newConstructorParameters">
        ///     The parameters to be used while constructing the new instance, which can override or supplement
        ///     the original constructor parameters.
        /// </param>
        /// <returns>
        ///     A newly created instance of the specified object type with properties initialized using the provided parameters.
        /// </returns>
        public T WithProperties(params object[] newConstructorParameters)
        {
            return new InstrumentedBuilder<T>(instanceType, newConstructorParameters).NewInstance();
        }
    }
}
