using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
/// A matcher that checks if a given item is present in a collection of elements.
/// </summary>
/// <typeparam name="T">The type of items that this matcher handles.</typeparam>
public class IsIn<T> : IMatcher<T>
{
    private readonly ICollection<T> _collection;

    // Constructor that accepts a collection
    public IsIn(ICollection<T> collection)
    {

      // Use the original collection if no nested collection structure is detected
      _collection = collection;
    }

    // Constructor that accepts an array and converts it into a collection
    public IsIn(params T[] elements)
    {
        // Flatten nested collections into a single list
        _collection = elements.ToList();
    }

    public void DescribeTo(IDescription description)
    {
        description.AppendText("one of ");
        description.AppendValueList("{", ", ", "}", _collection);
    }

    public bool Matches(T actual)
    {
        return _collection.Contains(actual);
    }

    public void DescribeMismatch(T item, IDescription mismatchDescription)
    {
        mismatchDescription.AppendText("was ").AppendValue((object) item);
    }
}