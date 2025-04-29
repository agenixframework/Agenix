using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
/// A wrapper class that adapts an <see cref="IMatcher{string}"/> instance
/// to become an <see cref="IMatcher{object}"/> by handling type casting internally.
/// </summary>
/// <remarks>
/// This class is used to bridge the gap between matchers expecting string input
/// and cases where the input type is object. It ensures that the encapsulated
/// string matcher can still be used in broader matching scenarios involving object types.
/// </remarks>
public class StringToObjectMatcherWrapper : IMatcher<object>
{
    private readonly IMatcher<string> _innerMatcher;

    public StringToObjectMatcherWrapper(IMatcher<string> innerMatcher)
    {
        _innerMatcher = innerMatcher;
    }

    public bool Matches(object item)
    {
        // Cast the input to string and pass it to the inner matcher's Matches method.
        return item is string str && _innerMatcher.Matches(str);
    }

    public void DescribeMismatch(object item, IDescription mismatchDescription)
    {
        
    }

    public void DescribeTo(IDescription description)
    {
        // Delegate to the inner matcher’s DescribeTo.
        _innerMatcher.DescribeTo(description);
    }

}
