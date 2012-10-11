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

namespace WorkerRoleWithSBQueue
{
    public class WorkerRole : RoleEntryPoint
    {
        // The name of your queue
        const string QueueName = "ProcessingQueue";
        string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

        // QueueClient is thread-safe. Recommended that you cache 
        // rather than recreating it on every request
        QueueClient Client;
        bool IsStopped;

        public override void Run()
        {
            while (!IsStopped)
            {
                try
                {
                    // Receive the message
                    var receivedMessage = Client.Receive();

                    if (receivedMessage != null)
                    {
                        // Process the message
                        var messageBody = receivedMessage.GetBody<string>();
                        var sequenceNumber = receivedMessage.SequenceNumber;

                        Trace.WriteLine(String.Format("Processing: {0} = {1}", sequenceNumber, messageBody), "Information");
                        receivedMessage.Complete();
                    }
                }
                catch (MessagingException e)
                {
                    if (!e.IsTransient)
                    {
                        Trace.WriteLine(String.Format("*** Error: {0}", e.Message), "Error");
                    }

                    Thread.Sleep(10000);
                }
                catch (OperationCanceledException e)
                {
                    if (!IsStopped)
                    {
                        Trace.WriteLine(String.Format("*** ERROR: ", e.Message), "Error");
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
                if (!namespaceManager.QueueExists(QueueName))
                    namespaceManager.CreateQueue(QueueName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // eat and/or log this one as it's usually caused by a race condition
            }


            // Initialize the connection to Service Bus Queue
            Client = QueueClient.CreateFromConnectionString(connectionString, QueueName, ReceiveMode.PeekLock);
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
