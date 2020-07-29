using MobileApp.Interfaces;
using MobileApp.Models;
using MobileApp.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileApp.Factories
{
    public static class MessageHandlerFactory
    {
        public static IMessageHandler GetMessageHandler => MessageHandler.Instance; // for now, we use the MessageHandler singleton instance
    }
}
