namespace MPP.Core.Validation.Matcher
{
    public interface IValueMatcher
    {
        /// <summary>
        ///     Value matcher verifies the match of given received and control values.
        /// </summary>
        /// <param name="received"></param>
        /// <param name="control"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        bool Validate(object received, object control, TestContext context);
    }
}