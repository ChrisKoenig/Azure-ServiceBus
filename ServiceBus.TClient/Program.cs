using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus.TClient
{
    internal class Program
    {
        private const string _topicOneName = "TopicOne";
        private const string _topicTwoName = "TopicTwo";
        private const string _connectionString = "Endpoint=sb://ckdemo.servicebus.windows.net;SharedSecretIssuer=owner;SharedSecretValue=sygYuji+u55NZLPBJg/LhZ+Eur13I1LanEjrxhcTMqI=";

        private static void Main(string[] args)
        {
            if (args.Contains("send"))
            {
                var clientOne = TopicClient.CreateFromConnectionString(_connectionString, _topicOneName);
                var clientTwo = TopicClient.CreateFromConnectionString(_connectionString, _topicTwoName);
                for (int i = 0; i < 10; i++)
                {
                    if (i % 2 == 0)
                    {
                        //send TopicTwo
                        clientTwo.Send(new BrokeredMessage("TopicTwo: " + i));
                    }
                    else
                    {
                        //send TopicOne
                        clientOne.Send(new BrokeredMessage("TopicOne: " + i));
                    }
                }
                clientOne.Close();
                clientTwo.Close();
            }

            if (args.Contains("show"))
            {
                BrokeredMessage message;
                var clientOne = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicOneName, _topicOneName);
                var clientTwo = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicTwoName, _topicTwoName);

                Console.WriteLine("Trying TopicOne Messages...");
                message = clientOne.Receive(TimeSpan.FromSeconds(5));
                while (message != null)
                {
                    var sequenceNumber = message.SequenceNumber;
                    var messageBody = message.GetBody<string>();
                    Console.WriteLine(String.Format("Displaying TopicOne: {0} = {1}", sequenceNumber, messageBody));
                    message = clientOne.Receive(TimeSpan.FromSeconds(5));
                }
                clientOne.Close();

                Console.WriteLine("Trying TopicTwo Messages...");
                message = clientTwo.Receive(TimeSpan.FromSeconds(5));
                while (message != null)
                {
                    var sequenceNumber = message.SequenceNumber;
                    var messageBody = message.GetBody<string>();
                    Console.WriteLine(String.Format("Displaying TopicTwo: {0} = {1}", sequenceNumber, messageBody));
                    message = clientTwo.Receive(TimeSpan.FromSeconds(5));
                }
                clientTwo.Close();
            }
        }
    }
}