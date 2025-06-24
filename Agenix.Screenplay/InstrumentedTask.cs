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
using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay;

public class InstrumentedTask
{
    public static T Of<T>(T task) where T : IPerformable
    {
        if (IsInstrumented(task) || !ShouldInstrument(task))
        {
            return task;
        }

        return (T)InstrumentedCopyOf(task, task.GetType());
    }

    private static bool ShouldInstrument<T>(T task) where T : IPerformable
    {
        var performAs = task.GetType().GetMethods()
            .FirstOrDefault(method => method.Name.Equals("PerformAs"));

        return performAs != null && DefaultConstructorPresentFor(task.GetType());
    }

    private static bool DefaultConstructorPresentFor(Type taskClass)
    {
        return FindAllConstructorsIn(taskClass)
            .Any(constructor => constructor.GetParameters().Length == 0);
    }

    private static IEnumerable<ConstructorInfo> FindAllConstructorsIn(Type taskClass)
    {
        var allConstructors = new List<ConstructorInfo>();

        allConstructors.AddRange(taskClass.GetConstructors());
        allConstructors.AddRange(taskClass.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance));

        return allConstructors;
    }

    private static IPerformable InstrumentedCopyOf(IPerformable task, Type taskClass)
    {
        IPerformable instrumentedTask;
        try
        {
            instrumentedTask = (IPerformable)Instrumented.InstanceOf<object>(taskClass).NewInstance();
        }
        catch (ArgumentException)
        {
            throw new TaskInstantiationException(
                $"Could not instantiate {taskClass}. " +
                "If you are not instrumenting a Task class explicitly you need to give the class a default constructor. " +
                "A task class cannot be instrumented if it is sealed (so if you are writing in C#, make sure the task class is not sealed).");
        }

        CopyNonNullProperties.From(task).To(instrumentedTask);
        return instrumentedTask;
    }

    private static bool IsInstrumented(IPerformable task)
    {
        try
        {
            return task.GetType().Name.Contains("ByteBuddy");
        }
        catch (NullReferenceException)
        {
            throw new TaskInstantiationException("Your Task class must have a public constructor.");
        }
    }
}
