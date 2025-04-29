using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
/// A matcher that ensures a collection has the specified size.
/// </summary>
/// <typeparam name="T">The type of elements in the collection being matched.</typeparam>
public class HasSize<T>(int size) : Matcher<IEnumerable<T>>
{
    public override bool Matches(IEnumerable<T> collection)
    {
        var count = 0;
        var enumerator = collection.GetEnumerator();
        while (enumerator.MoveNext()) count++;

        return count == size;
    }

    public override void DescribeTo(IDescription description)
    {
        description.AppendText("a collection with size ").AppendValue(size);
    }
}