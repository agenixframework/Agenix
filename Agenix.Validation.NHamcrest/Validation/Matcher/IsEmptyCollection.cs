using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
/// Represents a matcher that evaluates whether a given collection of type <typeparamref name="T"/> is empty.
/// </summary>
/// <typeparam name="T">
/// The type of elements contained within the collection being evaluated.
/// </typeparam>
/// <remarks>
/// This matcher is used to ensure that a collection does not contain any elements.
/// </remarks>
public class IsEmptyCollection<T> : IMatcher<IEnumerable<T>>
{
    public void DescribeTo(IDescription description)
    {
        description.AppendText("an empty collection");
    }

    public bool Matches(IEnumerable<T> collection)
    {
        return collection is not null && !collection.Any();

    }

    public void DescribeMismatch(IEnumerable<T> collection, IDescription mismatchDescription)
    {
        if (collection == null)
        {
            mismatchDescription.AppendText("was null");
        }
        else
        {
            mismatchDescription.AppendText($"was a collection with {collection.Count()} items: ");
            mismatchDescription.AppendValue(collection);
        }

    }
}