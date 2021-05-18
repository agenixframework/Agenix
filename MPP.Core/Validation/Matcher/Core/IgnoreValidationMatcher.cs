using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP.Core.Validation.Matcher.Core
{
    public class IgnoreValidationMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            Console.WriteLine($"Ignoring value for field '{fieldName}'");
        }
    }
}
