using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP.Core.Validation
{
    public interface IHeaderValidator
    {
        void ValidateHeader(string name, object received, object control, TestContext context);
    }
}
