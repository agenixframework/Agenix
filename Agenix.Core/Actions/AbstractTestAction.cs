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

using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Abstract base class for test actions. Class provides a default name and description.
/// </summary>
public abstract class AbstractTestAction : ITestAction, INamed, IDescribed
{
    /// <summary>
    ///     Describing the test action
    /// </summary>
    protected string description;

    public AbstractTestAction()
    {
        Name = GetType().Name;
    }

    public AbstractTestAction(string name, string description)
    {
        Name = name;
        this.description = description;
    }

    public AbstractTestAction(string name,
        AbstractTestActionBuilder<ITestAction, dynamic> builder)
    {
        Name = builder.GetName() ?? name;
        description = builder.GetDescription();
    }

    public string GetDescription()
    {
        return description;
    }

    public ITestAction SetDescription(string description)
    {
        this.description = description;
        return this;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     TestAction name injected as spring bean name
    /// </summary>
    public string Name { get; private set; }

    public virtual bool IsDisabled(TestContext context)
    {
        return false;
    }

    /// <summary>
    ///     Do basic logging and delegate execution to subclass.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Execute(TestContext context)
    {
        if (!IsDisabled(context)) DoExecute(context);
    }

    /// <summary>
    ///     Subclasses may add custom execution logic here.
    /// </summary>
    /// <param name="context"></param>
    public abstract void DoExecute(TestContext context);
}
