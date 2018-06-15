using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAppServiceBus.Models
{
    public class ServiceBusMessageInfo
    {
        public string MessageToSend { get; set; }
        public string MessageReceived { get; set; }
    }
}