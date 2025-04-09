using System.Collections;

namespace Agenix.Core.NUnitTestProject.TypeResolution;

public class TestObject
{
    private IDictionary periodicTable = new Hashtable();
    public string ObjectName { get; set; }

    public int ObjectNumber { get; set; }

    /// <summary>
    ///     Funny Named method.
    /// </summary>
    public virtual void Absquatulate()
    {
    }

    public void AddPeriodicElement(string name, string element)
    {
        if (periodicTable == null) periodicTable = new Hashtable();
        periodicTable.Add(name, element);
    }
}