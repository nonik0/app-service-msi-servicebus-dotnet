using System;
using Microsoft.ServiceBus.Messaging;
using System.Web.Mvc;
using Microsoft.ServiceBus;
using System.Text;
using WebAppServiceBus.Models;
using System.Web.Configuration;

namespace WebAppServiceBus.Controllers
{
    public class HomeController : Controller
    {
        private static string ServiceBusNamespace => WebConfigurationManager.AppSettings["ServiceBusNamespace"];
        private static string ServiceBusQueue => WebConfigurationManager.AppSettings["ServiceBusQueue"];

        private static MessagingFactory CreateMessagingFactoryWithMsiTokenProvider()
        {
            // create a parameter object for the messaging factory that configures
            // the MSI token provider for Service Bus and use of the AMQP protocol:
            MessagingFactorySettings messagingFactorySettings = new MessagingFactorySettings
            {
                TokenProvider = TokenProvider.CreateManagedServiceIdentityTokenProvider(ServiceAudience.ServiceBusAudience),
                TransportType = TransportType.Amqp
            };

            // create the messaging factory using the namespace endpoint name supplied by web.config
            MessagingFactory messagingFactory = MessagingFactory.Create($"sb://{ServiceBusNamespace}.servicebus.windows.net/",
                messagingFactorySettings);

            return messagingFactory;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Send(ServiceBusMessageInfo messageInfo)
        {
            if (string.IsNullOrEmpty(messageInfo.MessageToSend))
            {
                // TODO: show error message
                return RedirectToAction("Index");
            }

            // create a messaging factory configured to use a ManagedServiceIdentityTokenProvider
            MessagingFactory messagingFactory = CreateMessagingFactoryWithMsiTokenProvider();

            // create a queue client using the queue name supplied by the web.config
            QueueClient queueClient = messagingFactory.CreateQueueClient(ServiceBusQueue);

            // send a message using the input text
            queueClient.Send(new BrokeredMessage(Encoding.UTF8.GetBytes(messageInfo.MessageToSend)));

            queueClient.Close();
            messagingFactory.Close();

            return RedirectToAction("Index");
        }


        public ActionResult Receive()
        {
            // create a messaging factory configured to use a ManagedServiceIdentityTokenProvider
            MessagingFactory messagingFactory = CreateMessagingFactoryWithMsiTokenProvider();

            // create a queue client using the queue name supplied by the web.config
            QueueClient queueClient = messagingFactory.CreateQueueClient(ServiceBusQueue);

            // request a readily available message (with a very short wait) 
            BrokeredMessage msg = queueClient.Receive(TimeSpan.FromSeconds(1));

            var messageInfo = new ServiceBusMessageInfo();
            if (msg != null)
            {
                messageInfo.MessageReceived = $"Seq#:{msg.SequenceNumber} data:{Encoding.UTF8.GetString(msg.GetBody<byte[]>())}{Environment.NewLine}";
            }
            else
            {
                messageInfo.MessageReceived = "<no messages in queue>";
            }

            queueClient.Close();
            messagingFactory.Close();

            return View("Index", messageInfo);
        }
    }
}