using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using SharedObjects;

namespace RuleWorkerRoleHigh
{
    public class WorkerRole : RoleEntryPoint
    {
        private const string TopicName = "ruleprocessingtopic";
        private const string SubscriptionName = "ruleprocessingsubscriptionhigh";

        private bool IsStopped;
        private SubscriptionClient Client;
        private string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

        public override void Run()
        {
            while (!IsStopped)
            {
                try
                {
                    // Receive the message
                    var receivedMessage = Client.Receive(TimeSpan.FromSeconds(5));
                    if (receivedMessage != null)
                    {
                        // Process the message
                        var messageBody = receivedMessage.GetBody<HighLowObject>();
                        var sequenceNumber = receivedMessage.SequenceNumber;
                        Trace.WriteLine(String.Format("Processing High rule: {0} = {1} {2}", sequenceNumber, messageBody.CurrentAge, messageBody.HighOrLow), "Information");
                        receivedMessage.Complete();
                    }
                }
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }

                    Thread.Sleep(10000);
                }
                catch (OperationCanceledException e)
                {
                    if (!IsStopped)
                    {
                        Trace.WriteLine(e.Message);
                        throw;
                    }
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // Create the queue if it does not exist already
            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists(TopicName))
                namespaceManager.CreateTopic(TopicName);

            if (!namespaceManager.SubscriptionExists(TopicName, SubscriptionName))
                namespaceManager.CreateSubscription(TopicName, SubscriptionName, new SqlFilter("Age > 60"));

            // Initialize the connection to Service Bus Queue
            Client = SubscriptionClient.CreateFromConnectionString(connectionString, TopicName, SubscriptionName);
            IsStopped = false;
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            IsStopped = true;
            Client.Close();
            base.OnStop();
        }
    }
}