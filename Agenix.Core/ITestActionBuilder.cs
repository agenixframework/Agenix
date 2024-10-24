using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agenix.Core
{
    /// <summary>
    /// Test action builder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITestActionBuilder<T> where T : ITestAction
    {
        /// <summary>
        /// Builds new test action instance.
        /// </summary>
        /// <returns>the built test action.</returns>
        T Build();


        public interface IDelegatingTestActionBuilder<U> : ITestActionBuilder<U> where U : ITestAction
        {
            /// <summary>
            /// Obtains the delegate test action builder.
            /// </summary>
            ITestActionBuilder<U> Delegate { get; }
        }
    }
}
