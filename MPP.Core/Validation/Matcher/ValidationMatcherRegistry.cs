using System.Collections.Generic;
using System.Linq;
using FleetPay.Core.Exceptions;

namespace FleetPay.Core.Validation.Matcher
{
    /// <summary>
    ///     ValidationMatcher registry holding all available validation matcher libraries.
    /// </summary>
    public class ValidationMatcherRegistry
    {
        /// <summary>
        ///     List of libraries providing custom validation matchers
        /// </summary>
        private List<ValidationMatcherLibrary> _validationMatcherLibraries = new();

        /// <summary>
        ///     List of libraries providing custom validation matchers
        /// </summary>
        public List<ValidationMatcherLibrary> ValidationMatcherLibraries
        {
            get => _validationMatcherLibraries;
            set => _validationMatcherLibraries = value;
        }

        public ValidationMatcherLibrary GetLibraryForPrefix(string validationMatcherPrefix)
        {
            if (_validationMatcherLibraries == null)
                throw new NoSuchValidationMatcherLibraryException(
                    $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");

            foreach (var validationMatcherLibrary in _validationMatcherLibraries.Where(validationMatcherLibrary =>
                validationMatcherLibrary.Prefix.Equals(validationMatcherPrefix))) return validationMatcherLibrary;

            throw new NoSuchValidationMatcherLibraryException(
                $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");
        }

        /// <summary>
        ///     Adds given validation matcher library to this registry.
        /// </summary>
        /// <param name="validationMatcherLibrary"></param>
        public void AddValidationMatcherLibrary(ValidationMatcherLibrary validationMatcherLibrary)
        {
            var prefixAlreadyUsed =
                _validationMatcherLibraries.Any(lib => lib.Prefix.Equals(validationMatcherLibrary.Prefix));

            if (prefixAlreadyUsed)
                throw new CoreSystemException(
                    $"Validation matcher library prefix '{validationMatcherLibrary.Prefix} is already bound to another instance. " +
                    "Please choose another prefix.");

            _validationMatcherLibraries.Add(validationMatcherLibrary);
        }
    }
}