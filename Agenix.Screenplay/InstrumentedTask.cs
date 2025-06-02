#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
