#region Imports

using System.Collections.Generic;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Simple test object used for testing generic objects.
/// </summary>
public class TestGenericObject<T, U>
{
    #region Fields

    #endregion

    #region Properties

    public int ID { get; set; }

    public string Name { get; set; } = string.Empty;

    public IList<int> SomeGenericList { get; set; } = new List<int>();

    public IDictionary<string, int> SomeGenericDictionary { get; set; } = new Dictionary<string, int>();

    #endregion

    #region Constructor (s) / Destructor

    public TestGenericObject()
    {
    }

    public TestGenericObject(int id)
    {
        ID = id;
    }

    public TestGenericObject(int id, string name)
    {
        ID = id;
        Name = name;
    }

    #endregion

    #region Methods

    public static List<V> CreateList<V>()
    {
        return new List<V>();
    }

    public TestGenericObject<V, W> CreateInstance<V, W>()
    {
        return new TestGenericObject<V, W>();
    }

    #endregion
}
