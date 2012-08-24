using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus.QClient
{
    class Program
    {
        private const string _queueName = "ProcessingQueue";
        private const string _connectionString = "Endpoint=sb://ckdemo.servicebus.windows.net;SharedSecretIssuer=owner;SharedSecretValue=sygYuji+u55NZLPBJg/LhZ+Eur13I1LanEjrxhcTMqI=";
        
        static void Main(string[] args)
        {

            try
            {
                var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
                if (!namespaceManager.QueueExists(_queueName))
                    namespaceManager.CreateQueue(_queueName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // this is caused by race conditions, so just ignore it
            }

            var client = QueueClient.CreateFromConnectionString(_connectionString, _queueName);

            for (int i = 0; i < 5; i++)
            {
                var messageBody = String.Format("Message {0} >>> {1}", i, DateTime.Now.ToShortTimeString());
                Console.WriteLine("Sending " + messageBody);
                var message = new BrokeredMessage(messageBody);
                client.Send(message);
            }

            client.Close();

        }
    }
}
