using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using SharedObjects;

namespace RuleWorkerRoleLow
{
    public class WorkerRole : RoleEntryPoint
    {
        private const string TopicName = "ruleprocessingtopic";
        private const string SubscriptionName = "ruleprocessingsubscriptionlow";

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
                        Trace.WriteLine(String.Format("Processing Low rule: {0} = {1} {2}", sequenceNumber, messageBody.CurrentAge, messageBody.HighOrLow), "Information");
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

            //var retryStrategy = new Incremental(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
            //var retryPolicy = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(retryStrategy);

            try
            {
                if (!namespaceManager.TopicExists(TopicName))
                    namespaceManager.CreateTopic(TopicName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // eat and/or log this one as it's usually caused by a race condition
            }

            try
            {
                if (!namespaceManager.SubscriptionExists(TopicName, SubscriptionName))
                    namespaceManager.CreateSubscription(TopicName, SubscriptionName, new SqlFilter("Age < 60"));
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // eat and/or log this one as it's usually caused by a race condition
            }

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