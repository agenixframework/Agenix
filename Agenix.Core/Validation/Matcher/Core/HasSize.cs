using System.Collections.Generic;
using NHamcrest;
using NHamcrest.Core;

namespace Agenix.Core.Validation.Matcher.Core;

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