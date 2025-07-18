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
using System.Reflection.Emit;

namespace Agenix.Screenplay;

/// <summary>
///     A utility class for dynamically creating instances of types using runtime-generated constructors.
/// </summary>
/// <remarks>
///     This is particularly useful when you need to create instances of a type where the constructor is dynamically
///     generated
///     or requires flexibility in passing parameters.
/// </remarks>
public static class DynamicConstructorBuilder
{
    /// <summary>
    ///     Creates an instance of the specified type <typeparamref name="T" /> using a dynamically generated constructor.
    /// </summary>
    /// <typeparam name="T">The type of object to create.</typeparam>
    /// <param name="args">
    ///     Parameters to be passed to the constructor of the dynamically created instance. Ensure the parameters match the
    ///     expected
    ///     constructor signature of the specified type <typeparamref name="T" />.
    /// </param>
    /// <returns>Returns an instance of the specified type <typeparamref name="T" /> created using a dynamic constructor.</returns>
    public static T CreateInstance<T>(params object[] args)
    {
        var type = typeof(T);
        var dynamicType = CreateDynamicType<T>();

        // Create instance of our dynamic type
        var instance = (T)Activator.CreateInstance(dynamicType, args);
        return instance;
    }

    /// <summary>
    ///     Dynamically creates and returns a new type based on the specified type <typeparamref name="T" />.
    ///     The new type has a constructor that mirrors the constructor of the original type.
    /// </summary>
    /// <typeparam name="T">The base type on which the dynamic type is generated.</typeparam>
    /// <returns>Returns a dynamically generated type that inherits from the specified type <typeparamref name="T" />.</returns>
    private static Type CreateDynamicType<T>()
    {
        var originalType = typeof(T);
        var assemblyName = new AssemblyName($"{originalType.Name}Dynamic");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

        var typeBuilder = moduleBuilder.DefineType(
            $"{originalType.Namespace}.{originalType.Name}Dynamic",
            TypeAttributes.Public,
            originalType
        );

        // Copy the constructor
        var originalCtor = originalType.GetConstructors().First();
        var parameters = originalCtor.GetParameters();
        var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            parameterTypes
        );

        var il = constructorBuilder.GetILGenerator();

        // Call base constructor
        il.Emit(OpCodes.Ldarg_0); // Load "this"
        for (var i = 0; i < parameters.Length; i++)
        {
            il.Emit(OpCodes.Ldarg, i + 1); // Load parameter
        }

        il.Emit(OpCodes.Call, originalCtor);
        il.Emit(OpCodes.Ret);

        return typeBuilder.CreateType();
    }
}
