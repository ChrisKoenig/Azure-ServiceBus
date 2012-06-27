using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus.QClient
{
    class Program
    {
        const string _queueName = "ProcessingQueue";
        const string _connectionString = "Endpoint=sb://chriskoenig.servicebus.windows.net;SharedSecretIssuer=owner;SharedSecretValue=SvEwFJZmK+W4V1nRJDY1mAQNN7tZsdRByAq53BmGWS0=";
        static void Main(string[] args)
        {

            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            var client = QueueClient.CreateFromConnectionString(_connectionString, _queueName);

            if (args.Contains("send"))
            {

                for (int i = 0; i < 5; i++)
                {
                    var messageBody = String.Format("Message {0} | {1}", i, DateTime.Now.ToShortTimeString());
                    var message = new BrokeredMessage(messageBody);
                    client.Send(message);
                }
            }
            
            if (args.Contains("show"))
            {
                var message = client.Receive();
                while (message != null)
                {
                    var body = message.GetBody<string>();
                    Console.WriteLine(message.SequenceNumber + " = " + body);
                    message = client.Receive();
                }
            }

            client.Close();

        }
    }
}
