using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus.RClient
{
    internal class Program
    {
        private const string _topicName = "ruleprocessingtopic";
        private const string _connectionString = "Endpoint=sb://chriskoenig.servicebus.windows.net;SharedSecretIssuer=owner;SharedSecretValue=SvEwFJZmK+W4V1nRJDY1mAQNN7tZsdRByAq53BmGWS0=";

        private static void Main(string[] args)
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);

            if (!namespaceManager.TopicExists(_topicName))
                namespaceManager.CreateTopic(_topicName);

            var client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);
         
            for (int i = 0; i < 5; i++)
            {
                var age = 50 + (i * 5);
                var messageBody = new SharedObjects.HighLowObject() { CurrentAge = age, HighOrLow = age < 60 ? "Low" : "High" };
                var message = new BrokeredMessage(messageBody);
                message.Properties["Age"] = age;
                client.Send(message);
            }

            client.Close();
        }
    }
}