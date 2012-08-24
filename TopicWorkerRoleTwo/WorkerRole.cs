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
using Microsoft.WindowsAzure.ServiceRuntime;

namespace TopicWorkerRoleTwo
{
    public class WorkerRole : RoleEntryPoint
    {
        const string TopicName = "TopicTwo";
        string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

        SubscriptionClient Client;
        bool IsStopped;

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
                        var messageBody = receivedMessage.GetBody<string>();
                        var sequenceNumber = receivedMessage.SequenceNumber;
                        Trace.WriteLine(String.Format("Processing TopicTwo: {0} = {1}", sequenceNumber, messageBody), "Information");
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
                if (!namespaceManager.SubscriptionExists(TopicName, TopicName))
                    namespaceManager.CreateSubscription(TopicName, TopicName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // eat and/or log this one as it's usually caused by a race condition
            }

            // Initialize the connection to Service Bus Queue
            Client = SubscriptionClient.CreateFromConnectionString(connectionString, TopicName, TopicName);
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
