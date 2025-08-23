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

using Agenix.Playwright.Actions;
using Microsoft.Playwright;

namespace Agenix.Screenplay.Playwright.Interactions;

/// <summary>
///     Represents a target element on a web page that can be located using various strategies.
///     This is a high-level abstraction for element identification in the Screenplay pattern.
/// </summary>
public class Target
{
    private readonly List<LocatingElementAction.LocatorDefinition> _locatorDefinitions;

    private Target(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _locatorDefinitions = [];
    }

    /// <summary>
    ///     Gets the human-readable name of this target.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Creates a new Target with the specified name.
    /// </summary>
    /// <param name="name">The human-readable name of the target element.</param>
    /// <returns>A new Target instance.</returns>
    public static Target The(string name)
    {
        return new Target(name);
    }

    /// <summary>
    ///     Gets all locator definitions for this target.
    /// </summary>
    /// <returns>A read-only collection of locator definitions.</returns>
    public IReadOnlyList<LocatingElementAction.LocatorDefinition> GetLocatorDefinitions()
    {
        return _locatorDefinitions.AsReadOnly();
    }

    /// <summary>
    ///     Adds a CSS selector locator to this target.
    /// </summary>
    /// <param name="cssSelector">The CSS selector.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedBy(string cssSelector)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.CSS,
            Value = cssSelector
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds an XPath locator to this target.
    /// </summary>
    /// <param name="xpath">The XPath expression.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByXPath(string xpath)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.XPATH,
            Value = xpath
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a role-based locator to this target.
    /// </summary>
    /// <param name="role">The ARIA role.</param>
    /// <param name="name">The accessible name (optional).</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByRole(AriaRole role, string? name = null)
    {
        var newTarget = Clone();
        var options = name != null ? new PageGetByRoleOptions { Name = name } : null;
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.ROLE,
            Value = role.ToString(),
            Options = options != null
                ? new LocatingElementAction.LocatorOptions { GetByRoleOptions = options }
                : null
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a role-based locator with options to this target.
    /// </summary>
    /// <param name="role">The ARIA role.</param>
    /// <param name="options">The role options.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByRole(AriaRole role, PageGetByRoleOptions options)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.ROLE,
            Value = role.ToString(),
            Options = new LocatingElementAction.LocatorOptions { GetByRoleOptions = options }
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a text-based locator to this target.
    /// </summary>
    /// <param name="text">The text content to search for.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByText(string text)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.TEXT,
            Value = text
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a text-based locator with options to this target.
    /// </summary>
    /// <param name="text">The text content to search for.</param>
    /// <param name="options">The text search options.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByText(string text, PageGetByTextOptions options)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.TEXT,
            Value = text,
            Options = new LocatingElementAction.LocatorOptions { GetByTextOptions = options }
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a label-based locator to this target.
    /// </summary>
    /// <param name="label">The label text.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByLabel(string label)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.LABEL,
            Value = label
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a placeholder-based locator to this target.
    /// </summary>
    /// <param name="placeholder">The placeholder text.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByPlaceholder(string placeholder)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.PLACEHOLDER,
            Value = placeholder
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a test ID locator to this target.
    /// </summary>
    /// <param name="testId">The test ID attribute value.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByTestId(string testId)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.TEST_ID,
            Value = testId
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds an ID locator to this target.
    /// </summary>
    /// <param name="id">The ID attribute value.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedById(string id)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.ID,
            Value = id
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a class name locator to this target.
    /// </summary>
    /// <param name="className">The class name.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByClassName(string className)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.CLASS_NAME,
            Value = className
        });
        return newTarget;
    }

    /// <summary>
    ///     Adds a name attribute locator to this target.
    /// </summary>
    /// <param name="name">The name attribute value.</param>
    /// <returns>A new Target with the added locator.</returns>
    public Target LocatedByName(string name)
    {
        var newTarget = Clone();
        newTarget._locatorDefinitions.Add(new LocatingElementAction.LocatorDefinition
        {
            Strategy = LocatingElementAction.LocatorStrategy.NAME,
            Value = name
        });
        return newTarget;
    }

    /// <summary>
    ///     Creates a shallow copy of this target.
    /// </summary>
    /// <returns>A new Target with the same name and locator definitions.</returns>
    private Target Clone()
    {
        var clone = new Target(Name);
        clone._locatorDefinitions.AddRange(_locatorDefinitions);
        return clone;
    }

    /// <summary>
    ///     Returns a string representation of this target.
    /// </summary>
    public override string ToString()
    {
        return $"Target '{Name}' with {_locatorDefinitions.Count} locator(s)";
    }

    /// <summary>
    ///     Determines whether two Target instances are equal based on name and locators.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Target other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name.Equals(other.Name) &&
               _locatorDefinitions.SequenceEqual(other._locatorDefinitions);
    }

    /// <summary>
    ///     Returns a hash code for this Target.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, _locatorDefinitions.Count);
    }
}
