using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FleetPay.Core.Message;

namespace FleetPay.Core.Messaging
{
    public interface IProducer
    {
        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">the message object to send.</param>
        /// <param name="context">the internal context.</param>
        void Send(IMessage message, TestContext context);

        /// <summary>
        /// Gets the producer name.
        /// </summary>
        string Name { get; }
    }
}
