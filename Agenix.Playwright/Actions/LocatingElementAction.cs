using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for locating elements on the page with support for chaining locators.
///     Supports all Playwright locator strategies and provides 'And' chaining for complex element location.
/// </summary>
public class LocatingElementAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Strategy for locating elements on the page using their tag name.
    ///     This locator identifies elements based on their HTML tag, enabling a direct and efficient mechanism
    ///     to select elements by the type of tag they represent, such as 'div', 'span', or 'input'.
    /// </summary>
    public enum LocatorStrategy
    {
        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their ARIA roles.
        ///     Roles are used to define the purpose of an element in a user interface, improving accessibility and interaction.
        /// </summary>
        ROLE,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their visible text content.
        ///     This strategy is useful for selecting elements where the display text is known or is the primary identifier.
        /// </summary>
        TEXT,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their associated labels.
        ///     Useful for selecting form controls such as input fields or text areas through the text of their corresponding
        ///     labels.
        /// </summary>
        LABEL,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their placeholder text.
        ///     This is typically used to locate input fields where the placeholder attribute provides
        ///     a hint or description of the expected input.
        /// </summary>
        PLACEHOLDER,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their associated alternative text (alt text).
        ///     Alt text is commonly used to describe the content or purpose of an element, primarily for accessibility purposes.
        /// </summary>
        ALT_TEXT,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their title attribute.
        ///     The title attribute provides additional information about the element, often displayed as a tooltip on hover.
        /// </summary>
        TITLE,

        /// <summary>
        ///     Represents a locator strategy used to identify elements based on a test-specific identifier.
        ///     This strategy is typically employed in scenarios where elements are tagged with a unique identifier
        ///     specifically for testing purposes, aiding in reliable element selection during automated testing.
        /// </summary>
        TEST_ID,

        /// <summary>
        ///     Represents a locator strategy that identifies elements using CSS selectors.
        ///     CSS selectors provide a powerful and flexible way to pinpoint elements based on
        ///     attributes, hierarchy, and other styles applied in the DOM structure.
        /// </summary>
        CSS,

        /// <summary>
        ///     Represents a locator strategy used to identify elements within the DOM
        ///     based on XML Path Language (XPath) queries. XPath is a powerful syntax
        ///     for navigating and locating elements in an XML or HTML document structure,
        ///     enabling precise element selection through conditions, attributes, and hierarchy.
        /// </summary>
        XPATH,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their unique ID attribute.
        ///     The ID is typically a unique identifier assigned to an element within the DOM, allowing for precise selection.
        /// </summary>
        ID,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their class name attribute.
        ///     Class names are used to group elements in a structured manner, typically for styling and layout purposes.
        /// </summary>
        CLASS_NAME,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their "name" attribute value.
        ///     The "name" attribute is commonly used in form controls and other elements, providing a means
        ///     to programmatically target those elements for interaction or verification.
        /// </summary>
        NAME,

        /// <summary>
        ///     Represents a locator strategy that identifies elements based on their HTML tag name.
        ///     This strategy allows for selecting elements directly by specifying their tag type, such as 'div', 'input', or
        ///     'button'.
        /// </summary>
        TAG_NAME,

        /// <summary>
        ///     Represents a locator strategy that identifies hyperlink elements based on their exact visible text.
        ///     This strategy is generally used to locate anchor elements by matching their displayed text, which helps
        ///     to uniquely identify links on the page.
        /// </summary>
        LINK_TEXT,

        /// <summary>
        ///     Represents a locator strategy that identifies elements by matching a subset of their visible link text.
        ///     Useful for locating hyperlinks when only a portion of the link text is known or consistent.
        /// </summary>
        PARTIAL_LINK_TEXT
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(LocatingElementAction));

    private readonly List<LocatorDefinition> _locators;


    /// <summary>
    ///     Represents an action for locating web elements using various strategies provided by Playwright.
    ///     This class serves as the base for other actions that require precise element identification.
    /// </summary>
    public LocatingElementAction(Builder builder) : this("locate", builder) { }


    /// <summary>
    ///     Serves as a base class for actions that involve locating web elements using specified locators.
    ///     This class ensures that elements are properly identified prior to performing further interactions.
    /// </summary>
    public LocatingElementAction(string name, Builder builder) : base(name, builder)
    {
        _locators = builder.Locators ??
                    throw new ArgumentException("At least one locator is required", nameof(builder));
    }

    /// <summary>
    ///     Represents an action for locating elements in a web page using various locators.
    ///     This class is part of the Playwright action hierarchy and is used as the basis
    ///     for actions that require precise targeting of one or more web elements.
    /// </summary>
    protected LocatingElementAction(string name, ExpectLocatorAction.Builder builder) : base(name, builder)
    {
        _locators = builder.Locators ??
                    throw new ArgumentException("At least one locator is required", nameof(builder));
    }

    /// <summary>
    ///     Executes the primary action for locating elements using the defined locators in the context of the provided
    ///     browser.
    /// </summary>
    /// <param name="browser">The browser instance used to interact with the web application during the test</param>
    /// <param name="context">The test context containing state, configuration, and data for the test execution</param>
    /// <returns>A task representing the asynchronous operation</returns>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            Logger.LogDebug("Locating element with {Count} locator(s)", _locators.Count);

            var page = browser.Page;
            var locator = BuildChainedLocator(page, _locators, context);

            // Execute the virtual method with the found locator
            await Execute(locator, browser, context);

            Logger.LogInformation("Successfully located and processed element");
        }
        catch (Exception ex)
        {
            throw new AgenixSystemException("Failed to locate element", ex);
        }
    }

    /// <summary>
    ///     Virtual method to be overridden by derived classes to perform specific actions on the located element
    /// </summary>
    /// <param name="locator">The located element</param>
    /// <param name="browser">The browser instance</param>
    /// <param name="context">The test context</param>
    protected virtual Task Execute(ILocator locator, PlaywrightBrowser browser, TestContext context)
    {
        return Task.CompletedTask;
    }

    private static ILocator BuildChainedLocator(IPage page, List<LocatorDefinition> locators, TestContext context)
    {
        if (locators.Count == 0)
        {
            throw new ArgumentException("At least one locator is required");
        }

        var baseLocator = CreateLocator(page, locators[0], context);

        // Chain additional locators using 'and' method
        for (var i = 1; i < locators.Count; i++)
        {
            var nextLocator = CreateLocator(page, locators[i], context);
            baseLocator = baseLocator.And(nextLocator);
        }

        return baseLocator;
    }

    private static ILocator CreateLocator(IPage page, LocatorDefinition definition, TestContext context)
    {
        definition.Value = context.ReplaceDynamicContentInString(definition.Value);
        return definition.Strategy switch
        {
            LocatorStrategy.ROLE => CreateRoleLocator(page, definition.Value, definition.Options),
            LocatorStrategy.TEXT => page.GetByText(definition.Value, definition.Options?.GetByTextOptions),
            LocatorStrategy.LABEL => page.GetByLabel(definition.Value, definition.Options?.GetByLabelOptions),
            LocatorStrategy.PLACEHOLDER => page.GetByPlaceholder(definition.Value,
                definition.Options?.GetByPlaceholderOptions),
            LocatorStrategy.ALT_TEXT => page.GetByAltText(definition.Value, definition.Options?.GetByAltTextOptions),
            LocatorStrategy.TITLE => page.GetByTitle(definition.Value, definition.Options?.GetByTitleOptions),
            LocatorStrategy.TEST_ID => page.GetByTestId(definition.Value),
            LocatorStrategy.CSS => page.Locator(definition.Value, definition.Options?.PageLocatorOptions),
            LocatorStrategy.XPATH => page.Locator(definition.Value, definition.Options?.PageLocatorOptions),
            LocatorStrategy.ID => page.Locator($"#{definition.Value}", definition.Options?.PageLocatorOptions),
            LocatorStrategy.CLASS_NAME => page.Locator($".{definition.Value}", definition.Options?.PageLocatorOptions),
            LocatorStrategy.NAME => page.Locator($"[name='{definition.Value}']",
                definition.Options?.PageLocatorOptions),
            LocatorStrategy.TAG_NAME => page.Locator(definition.Value, definition.Options?.PageLocatorOptions),
            LocatorStrategy.LINK_TEXT => page.GetByRole(AriaRole.Link,
                new PageGetByRoleOptions { Name = definition.Value }),
            LocatorStrategy.PARTIAL_LINK_TEXT => page.GetByRole(AriaRole.Link, new PageGetByRoleOptions
            {
                NameRegex = new Regex(
                    definition.Value,
                    RegexOptions.None,
                    TimeSpan.FromSeconds(5)) // 5-second timeout
            }),

            _ => throw new ArgumentException($"Unsupported locator strategy: {definition.Strategy}")
        };
    }

    private static ILocator CreateRoleLocator(IPage page, string value, LocatorOptions? options)
    {
        if (!Enum.TryParse<AriaRole>(value, true, out var role))
        {
            throw new ArgumentException($"Invalid ARIA role: {value}");
        }

        var roleOptions = options?.GetByRoleOptions ?? new PageGetByRoleOptions();
        return page.GetByRole(role, roleOptions);
    }

    /// <summary>
    ///     Represents a definition for locating elements using specific strategies within a Playwright action.
    ///     Specifies the strategy, value, and additional options for identifying elements in a structured manner.
    /// </summary>
    public class LocatorDefinition
    {
        /// <summary>
        ///     Represents the strategy used to locate an element within a webpage.
        /// </summary>
        /// <remarks>
        ///     The Strategy property is used to define the approach or method for identifying
        ///     elements on a webpage. It is a part of the LocatorDefinition class and allows
        ///     selection from a variety of predefined strategies such as ROLE, TEXT, CSS,
        ///     X_PATH, ID, and others.
        /// </remarks>
        public LocatorStrategy Strategy { get; set; }

        /// <summary>
        ///     Represents the value used by a locator strategy to identify an element.
        /// </summary>
        /// <remarks>
        ///     The Value property specifies the identifier or search criterion, such as a CSS selector,
        ///     text, ID, or any other string required based on the chosen strategy in the LocatorDefinition class.
        ///     It serves as the primary input for locating elements on a webpage.
        /// </remarks>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        ///     Represents additional configuration or parameters applied to a locator strategy when identifying web elements.
        /// </summary>
        /// <remarks>
        ///     The Options property provides a way to specify detailed settings that can be used in conjunction with various
        ///     locator strategies. These settings may include attributes like role-specific options, text matching options,
        ///     placeholder handling, or CSS-related configurations. It ensures fine-grained control over the element
        ///     location process in Playwright-based actions.
        /// </remarks>
        public LocatorOptions? Options { get; set; }
    }

    /// <summary>
    ///     Represents configuration options to refine locator selection for various Playwright locator strategies.
    ///     Provides customizable parameters for locating elements by role, text, label, placeholder, alt text, title, and
    ///     other supported strategies.
    /// </summary>
    public class LocatorOptions
    {
        /// <summary>
        ///     Represents the options used to configure the selection of elements by their ARIA role.
        /// </summary>
        /// <remarks>
        ///     The GetByRoleOptions property allows the customization of locators that identify elements by their ARIA roles.
        ///     It enables the specification of additional parameters such as the name of the role or strictness of the match
        ///     to refine the search behavior when locating elements within a webpage.
        /// </remarks>
        public PageGetByRoleOptions? GetByRoleOptions { get; set; }

        /// <summary>
        ///     Defines options for locating an element based on a visible text on a webpage.
        /// </summary>
        /// <remarks>
        ///     The GetByTextOptions property allows customization of the behavior when locating elements
        ///     using textual criteria. This includes configuring settings such as case sensitivity or matching
        ///     text exactly. It is used in conjunction with the text-based locator strategy to refine element selection
        ///     based on specific text matching needs.
        /// </remarks>
        public PageGetByTextOptions? GetByTextOptions { get; set; }

        /// <summary>
        ///     Represents the configuration options for locating an element using its associated label text.
        /// </summary>
        /// <remarks>
        ///     The GetByLabelOptions property is utilized to define optional parameters or settings
        ///     when identifying an element by its label text. It is commonly used when employing
        ///     the LABEL locator strategy and may include options such as whether an exact match
        ///     for the label text is required.
        /// </remarks>
        public PageGetByLabelOptions? GetByLabelOptions { get; set; }

        /// <summary>
        ///     Gets or sets the options for locating elements by their placeholder text.
        /// </summary>
        /// <remarks>
        ///     The GetByPlaceholderOptions property specifies the configuration options
        ///     used when searching for elements using placeholder text as the locator strategy.
        ///     These options can help refine the search, such as specifying whether the match
        ///     should be exact or not.
        /// </remarks>
        public PageGetByPlaceholderOptions? GetByPlaceholderOptions { get; set; }

        /// <summary>
        ///     Represents the options used when locating an element by its alternative text (alt text).
        /// </summary>
        /// <remarks>
        ///     The GetByAltTextOptions property provides additional configuration for locating elements
        ///     with a specified alternative text (alt text) attribute. This can include parameters
        ///     determining whether to search for an exact match or other criteria relevant to the
        ///     element resolution process.
        /// </remarks>
        public PageGetByAltTextOptions? GetByAltTextOptions { get; set; }

        /// <summary>
        ///     Represents the configuration options for locating an element by its title attribute.
        /// </summary>
        /// <remarks>
        ///     The GetByTitleOptions property is used to specify parameters when identifying elements
        ///     by their title attribute. It is primarily used to refine the search criteria when working
        ///     with locators that rely on the title attribute for element selection. These options may include
        ///     settings such as matching the title exactly or using other rules for element identification.
        /// </remarks>
        public PageGetByTitleOptions? GetByTitleOptions { get; set; }

        /// <summary>
        ///     Specifies configuration options for fine-tuning locator behavior in Playwright.
        /// </summary>
        /// <remarks>
        ///     The PageLocatorOptions property allows customization of locator actions when interacting
        ///     with elements on a web page. It provides additional parameters that refine the search
        ///     scope and behavior, enhancing precision and flexibility for element selection.
        /// </remarks>
        public PageLocatorOptions? PageLocatorOptions { get; }
    }

    /// <summary>
    ///     Provides a fluent builder for configuring options when locating elements by role.
    ///     Allows customization of various parameters such as name, checked state, visibility,
    ///     and more to refine the search for elements on the page.
    /// </summary>
    public class RoleOptionsBuilder
    {
        private readonly PageGetByRoleOptions _options = new();

        /// <summary>
        ///     Specifies the name attribute of the element to locate when building role-based options.
        ///     This method refines the search for elements with a matching name.
        /// </summary>
        /// <param name="name">The name of the element to locate.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithName(string name)
        {
            _options.Name = name;
            return this;
        }

        /// <summary>
        ///     Specifies a regular expression to match the name of the element when building role-based options.
        ///     This method enhances the ability to locate elements with names that match the specified pattern.
        /// </summary>
        /// <param name="nameRegex">The regular expression to match the name of the element.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithNameRegex(Regex nameRegex)
        {
            _options.NameRegex = nameRegex;
            return this;
        }

        /// <summary>
        ///     Configures the exact_match setting for locating elements by role, ensuring the search strictly matches the role's
        ///     characteristics.
        /// </summary>
        /// <param name="exact">Specifies whether the match should be exact. Defaults to true.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance for method chaining.</returns>
        public RoleOptionsBuilder WithExact(bool exact = true)
        {
            _options.Exact = exact;
            return this;
        }

        /// <summary>
        ///     Specifies the checked state of the element to locate when building role-based options.
        ///     This method refines the search for elements based on their checked or unchecked state.
        /// </summary>
        /// <param name="checked">
        ///     The desired checked state of the element. Set to true for checked elements, false for unchecked
        ///     elements, or null to ignore the checked state.
        /// </param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithChecked(bool? @checked = true)
        {
            _options.Checked = @checked;
            return this;
        }

        /// <summary>
        ///     Specifies whether the element to locate should be disabled.
        ///     This method refines the search for elements based on their disabled state.
        /// </summary>
        /// <param name="disabled">A boolean value indicating whether the element is disabled. Defaults to true.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithDisabled(bool disabled = true)
        {
            _options.Disabled = disabled;
            return this;
        }

        /// <summary>
        ///     Specifies the expanded state of the element to locate when building role-based options.
        ///     This method is used to refine the search for elements based on their expanded or collapsed state.
        /// </summary>
        /// <param name="expanded">
        ///     A nullable boolean value indicating whether the element is expanded.
        ///     Pass <c>true</c> for expanded, <c>false</c> for collapsed, or <c>null</c> for no preference.
        /// </param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithExpanded(bool? expanded = true)
        {
            _options.Expanded = expanded;
            return this;
        }

        /// <summary>
        ///     Specifies the pressed state of the element to locate when building role-based options.
        ///     This method filters elements based on their pressed state, enabling precise selection of toggle or pressable
        ///     elements.
        /// </summary>
        /// <param name="pressed">
        ///     The pressed state of the element to locate. A value of <c>true</c>, <c>false</c>, or <c>null</c>
        ///     can be provided to refine the search.
        /// </param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithPressed(bool? pressed = true)
        {
            _options.Pressed = pressed;
            return this;
        }

        /// <summary>
        ///     Specifies the selected state of the element to locate when building role-based options.
        ///     This method refines the search for elements based on their selected state.
        /// </summary>
        /// <param name="selected">Indicates whether the element should be in a selected state. Pass null to ignore this parameter.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithSelected(bool? selected = true)
        {
            _options.Selected = selected;
            return this;
        }

        /// <summary>
        ///     Specifies the hierarchical level of the element when building role-based options.
        ///     This method is used to locate elements based on their level within a structural hierarchy.
        /// </summary>
        /// <param name="level">The hierarchical level of the element to locate.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithLevel(int level)
        {
            _options.Level = level;
            return this;
        }

        /// <summary>
        ///     Specifies whether hidden elements should be included when building role-based options.
        ///     This method refines the search by optionally including elements that are not visible on the page.
        /// </summary>
        /// <param name="includeHidden">A boolean indicating whether to include hidden elements. Defaults to true.</param>
        /// <returns>The updated <see cref="RoleOptionsBuilder" /> instance to allow method chaining.</returns>
        public RoleOptionsBuilder WithIncludeHidden(bool includeHidden = true)
        {
            _options.IncludeHidden = includeHidden;
            return this;
        }

        internal PageGetByRoleOptions Build()
        {
            return _options;
        }
    }


    /// <summary>
    ///     Builder class for creating LocatingElementAction instances with fluent API.
    /// </summary>
    public new abstract class Builder<TAction, TBuilder> : AbstractPlaywrightAction.Builder<TAction, TBuilder>
        where TAction : LocatingElementAction
        where TBuilder : Builder<TAction, TBuilder>
    {
        internal List<LocatorDefinition>? Locators { get; set; }

        /// <summary>
        ///     Sets the element locator using role-based selection.
        /// </summary>
        public TBuilder WithRole(AriaRole role, string? name = null, bool? exact = null)
        {
            var options = new LocatorOptions
            {
                GetByRoleOptions = new PageGetByRoleOptions { Name = name, Exact = exact }
            };
            AddLocator(LocatorStrategy.ROLE, role.ToString(), options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using role-based selection with options.
        /// </summary>
        public TBuilder WithRole(AriaRole role, PageGetByRoleOptions roleOptions)
        {
            var options = new LocatorOptions { GetByRoleOptions = roleOptions };
            AddLocator(LocatorStrategy.ROLE, role.ToString(), options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using role-based selection with configuration.
        /// </summary>
        public TBuilder WithRole(AriaRole role, Action<RoleOptionsBuilder> configureOptions)
        {
            var optionsBuilder = new RoleOptionsBuilder();
            configureOptions(optionsBuilder);
            var options = new LocatorOptions { GetByRoleOptions = optionsBuilder.Build() };
            AddLocator(LocatorStrategy.ROLE, role.ToString(), options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using text content.
        /// </summary>
        public TBuilder WithText(string text, bool? exact = null)
        {
            var options = new LocatorOptions { GetByTextOptions = new PageGetByTextOptions { Exact = exact } };
            AddLocator(LocatorStrategy.TEXT, text, options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using label text.
        /// </summary>
        public TBuilder WithLabel(string label, bool? exact = null)
        {
            var options = new LocatorOptions { GetByLabelOptions = new PageGetByLabelOptions { Exact = exact } };
            AddLocator(LocatorStrategy.LABEL, label, options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using placeholder text.
        /// </summary>
        public TBuilder WithPlaceholder(string placeholder, bool? exact = null)
        {
            var options = new LocatorOptions
            {
                GetByPlaceholderOptions = new PageGetByPlaceholderOptions { Exact = exact }
            };
            AddLocator(LocatorStrategy.PLACEHOLDER, placeholder, options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using alt text.
        /// </summary>
        public TBuilder WithAltText(string altText, bool? exact = null)
        {
            var options = new LocatorOptions { GetByAltTextOptions = new PageGetByAltTextOptions { Exact = exact } };
            AddLocator(LocatorStrategy.ALT_TEXT, altText, options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using the title attribute.
        /// </summary>
        public TBuilder WithTitle(string title, bool? exact = null)
        {
            var options = new LocatorOptions { GetByTitleOptions = new PageGetByTitleOptions { Exact = exact } };
            AddLocator(LocatorStrategy.TITLE, title, options);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using test ID.
        /// </summary>
        public TBuilder WithTestId(string testId)
        {
            AddLocator(LocatorStrategy.TEST_ID, testId);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using CSS selector.
        /// </summary>
        public TBuilder WithCss(string cssSelector)
        {
            AddLocator(LocatorStrategy.CSS, cssSelector);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using XPath expression.
        /// </summary>
        public TBuilder WithXPath(string xpathExpression)
        {
            AddLocator(LocatorStrategy.XPATH, xpathExpression);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using ID attribute.
        /// </summary>
        public TBuilder WithId(string id)
        {
            AddLocator(LocatorStrategy.ID, id);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using class name.
        /// </summary>
        public TBuilder WithClassName(string className)
        {
            AddLocator(LocatorStrategy.CLASS_NAME, className);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using name attribute.
        /// </summary>
        public TBuilder WithName(string name)
        {
            AddLocator(LocatorStrategy.NAME, name);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using tag name.
        /// </summary>
        public TBuilder WithTagName(string tagName)
        {
            AddLocator(LocatorStrategy.TAG_NAME, tagName);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using link text.
        /// </summary>
        public TBuilder WithLinkText(string linkText)
        {
            AddLocator(LocatorStrategy.LINK_TEXT, linkText);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Sets the element locator using partial link text.
        /// </summary>
        public TBuilder WithPartialLinkText(string partialLinkText)
        {
            AddLocator(LocatorStrategy.PARTIAL_LINK_TEXT, partialLinkText);
            return (TBuilder)this;
        }

        /// <summary>
        ///     Adds another locator to the chain.
        /// </summary>
        public TBuilder And()
        {
            return (TBuilder)this;
        }

        /// <summary>
        ///     Adds a locator definition to the list.
        /// </summary>
        protected void AddLocator(LocatorStrategy strategy, string value, LocatorOptions? options = null)
        {
            Locators ??= [];
            Locators.Add(new LocatorDefinition { Strategy = strategy, Value = value, Options = options });
        }

        /// <summary>
        ///     Builds the action instance. Must be implemented by concrete builders.
        /// </summary>
        public abstract override TAction Build();
    }

    /// <summary>
    ///     Non-generic builder for backward compatibility.
    /// </summary>
    public class Builder : Builder<LocatingElementAction, Builder>
    {
        /// <summary>
        ///     Builds an instance of the <see cref="LocatingElementAction" /> using the current configuration of the builder.
        /// </summary>
        /// <returns>An instance of <see cref="LocatingElementAction" />.</returns>
        public override LocatingElementAction Build()
        {
            return new LocatingElementAction(this);
        }
    }
}
