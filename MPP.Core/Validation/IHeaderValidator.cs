namespace FleetPay.Core.Validation
{
    public interface IHeaderValidator
    {
        void ValidateHeader(string name, object received, object control, TestContext context);
    }
}
