using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Interfaces
{
    public interface IMessageHandler
    {
        void HandleRabbitMessage(object sender, BasicDeliverEventArgs e); // we send both parameters that we get from the RabbitMQ service. If we need them in the future, we'll be ready.
    }
}
