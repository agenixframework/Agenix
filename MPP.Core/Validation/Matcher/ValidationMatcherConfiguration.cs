namespace MPP.Core.Validation.Matcher
{
    public class ValidationMatcherConfiguration
    {
        public ValidationMatcherLibrary ValidationMatcherLibrary { get; } = new DefaultValidationMatcherLibrary();
    }
}