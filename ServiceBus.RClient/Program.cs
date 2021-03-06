﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ServiceBus.RClient
{
    internal class Program
    {
        private const string _topicName = "ruleprocessingtopic";
        private const string _connectionString = "Endpoint=sb://ckdemo.servicebus.windows.net;SharedSecretIssuer=owner;SharedSecretValue=sygYuji+u55NZLPBJg/LhZ+Eur13I1LanEjrxhcTMqI=";

        private static void Main(string[] args)
        {

            try
            {
                var namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
                if (!namespaceManager.TopicExists(_topicName))
                    namespaceManager.CreateTopic(_topicName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // this is caused by race conditions, so just ignore it
            }

            var client = TopicClient.CreateFromConnectionString(_connectionString, _topicName);

            for (int i = 0; i < 5; i++)
            {
                var age = 50 + (i * 5);
                var messageBody = new SharedObjects.HighLowObject()
                {
                    CurrentAge = age,
                    HighOrLow = age < 60 ? "Low" : "High"
                };
                Console.WriteLine("Sending Message >>> " + messageBody);
                var message = new BrokeredMessage(messageBody);
                message.Properties["Age"] = age;
                client.Send(message);
            }

            client.Close();
        }
    }
}