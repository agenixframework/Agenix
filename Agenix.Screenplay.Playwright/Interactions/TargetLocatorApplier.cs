using Agenix.Playwright.Actions;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Playwright;

namespace Agenix.Screenplay.Playwright.Interactions;

/// <summary>
///     Simple utility that applies Target locators using dynamic dispatch.
///     Works with any builder that has the standard WithXXX methods.
/// </summary>
public static class TargetApplier
{
    private static readonly
        Dictionary<LocatingElementAction.LocatorStrategy, Action<dynamic, LocatingElementAction.LocatorDefinition>>
        LocatorAppliers =
            new()
            {
                [LocatingElementAction.LocatorStrategy.CSS] = (builder, locator) => builder.WithCss(locator.Value),
                [LocatingElementAction.LocatorStrategy.XPATH] =
                    (builder, locator) => builder.WithXPath(locator.Value),
                [LocatingElementAction.LocatorStrategy.TEST_ID] =
                    (builder, locator) => builder.WithTestId(locator.Value),
                [LocatingElementAction.LocatorStrategy.ID] = (builder, locator) => builder.WithId(locator.Value),
                [LocatingElementAction.LocatorStrategy.CLASS_NAME] =
                    (builder, locator) => builder.WithClassName(locator.Value),
                [LocatingElementAction.LocatorStrategy.NAME] =
                    (builder, locator) => builder.WithName(locator.Value),
                [LocatingElementAction.LocatorStrategy.TAG_NAME] =
                    (builder, locator) => builder.WithTagName(locator.Value),
                [LocatingElementAction.LocatorStrategy.LINK_TEXT] =
                    (builder, locator) => builder.WithLinkText(locator.Value),
                [LocatingElementAction.LocatorStrategy.PARTIAL_LINK_TEXT] =
                    (builder, locator) => builder.WithPartialLinkText(locator.Value),
                [LocatingElementAction.LocatorStrategy.ROLE] = ApplyRoleLocator,
                [LocatingElementAction.LocatorStrategy.TEXT] = ApplyTextLocator,
                [LocatingElementAction.LocatorStrategy.LABEL] = ApplyLabelLocator,
                [LocatingElementAction.LocatorStrategy.PLACEHOLDER] = ApplyPlaceholderLocator,
                [LocatingElementAction.LocatorStrategy.ALT_TEXT] = ApplyAltTextLocator,
                [LocatingElementAction.LocatorStrategy.TITLE] = ApplyTitleLocator
            };

    /// <summary>
    ///     Applies Target locators to any builder using dynamic dispatch.
    /// </summary>
    public static T ApplyTarget<T>(T builder, Target target) where T : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        ArgumentNullException.ThrowIfNull(target);

        foreach (var locator in target.GetLocatorDefinitions())
        {
            ApplyLocator(builder, locator);
        }

        return builder;
    }

    private static void ApplyLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (!LocatorAppliers.TryGetValue(locator.Strategy, out var applier))
        {
            throw new NotSupportedException($"Locator strategy {locator.Strategy} is not supported");
        }

        try
        {
            applier(builder, locator);
        }
        catch (RuntimeBinderException)
        {

        }
    }

    private static void ApplyRoleLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (Enum.TryParse<AriaRole>(locator.Value, out var role))
        {
            if (locator.Options?.GetByRoleOptions != null)
            {
                builder.WithRole(role, locator.Options.GetByRoleOptions);
            }
            else
            {
                builder.WithRole(role);
            }
        }
    }

    private static void ApplyTextLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (locator.Options?.GetByTextOptions != null)
        {
            builder.WithText(locator.Value, locator.Options.GetByTextOptions);
        }
        else
        {
            builder.WithText(locator.Value);
        }
    }

    private static void ApplyLabelLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (locator.Options?.GetByLabelOptions != null)
        {
            builder.WithLabel(locator.Value, locator.Options.GetByLabelOptions);
        }
        else
        {
            builder.WithLabel(locator.Value);
        }
    }

    private static void ApplyPlaceholderLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (locator.Options?.GetByPlaceholderOptions != null)
        {
            builder.WithPlaceholder(locator.Value, locator.Options.GetByPlaceholderOptions);
        }
        else
        {
            builder.WithPlaceholder(locator.Value);
        }
    }

    private static void ApplyAltTextLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (locator.Options?.GetByAltTextOptions != null)
        {
            builder.WithAltText(locator.Value, locator.Options.GetByAltTextOptions);
        }
        else
        {
            builder.WithAltText(locator.Value);
        }
    }

    private static void ApplyTitleLocator(dynamic builder, LocatingElementAction.LocatorDefinition locator)
    {
        if (locator.Options?.GetByTitleOptions != null)
        {
            builder.WithTitle(locator.Value, locator.Options.GetByTitleOptions);
        }
        else
        {
            builder.WithTitle(locator.Value);
        }
    }
}
