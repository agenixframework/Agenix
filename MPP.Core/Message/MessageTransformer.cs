using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPP.Core.Message
{
    interface IMessageTransformer
    {

        interface IBuilder<out T, TB> where T: IMessageTransformer where TB : IBuilder<T, TB> {

            /// <summary>
            /// Builds new message processor instance.
            /// </summary>
            /// <returns></returns>
            T Build();
        }
    }
}
