namespace Agenix.Core.Common;

public interface IDescribed
{
    /**
     * Description of this test action
     * @return description as String
     */
    public string GetDescription()
    {
        return "";
    }

    /**
     * Description setter.
     * @param description
     */
    ITestAction SetDescription(string description);
}