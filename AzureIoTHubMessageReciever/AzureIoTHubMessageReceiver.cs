//|---------------------------------------------------------------|
//|                         AZURE IoT HUB                         |
//|---------------------------------------------------------------|
//|                       Developed by Wonde Tadesse              |
//|                             Copyright ©2017 - Present         |
//|---------------------------------------------------------------|
//|                         AZURE IoT HUB                         |
//|---------------------------------------------------------------|

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ServiceBus.Messaging;
using System.Threading;

namespace AzureIoTHubMessageReciever
{
    /// <summary>
    /// Azure IoT Hub message receiver
    /// </summary>
    public class AzureIoTHubMessageReceiver
    {
        #region Public Methods 

        /// <summary>
        /// Receive IoT Hub messages
        /// </summary>
        public void RecieveIoTHubMessages()
        {
            EventHubClient eventHubClient = null;

            if (TryCreatingEventHubClient(out eventHubClient))
            {
                ReceiveMessagesFromDeviceAsync(eventHubClient);
            }
            else
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        #endregion

        #region Private Methods 
                
        /// <summary>
        /// Try creating Event Hub Client object
        /// </summary>
        /// <param name="eventHubClient">EventHubClient object value</param>
        /// <returns>true/false</returns>
        private bool TryCreatingEventHubClient(out EventHubClient eventHubClient)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                eventHubClient = EventHubClient.CreateFromConnectionString(ConfigurationManager.AppSettings.Get("IoTHubConnectionString"), ConfigurationManager.AppSettings.Get("IoTHubMessageEndpoint"));
                Console.WriteLine("Event Hub Client successfully created !\n");
            }
            catch (Exception exception)
            {
                eventHubClient = null;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred !");
                Console.WriteLine(exception);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Recieve message from IoT Device
        /// </summary>
        /// <param name="eventHubClient">EventHubClient object value</param>
        /// <param name="partitionID">PartitionID value</param>
        /// <param name="cancellationToken">CancellationToken value</param>
        /// <returns>Task object</returns>
        private async Task ReceiveMessagesFromDeviceAsync(EventHubClient eventHubClient, string partitionID, CancellationToken cancellationToken)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partitionID, DateTime.UtcNow);
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    EventData eventData = await eventHubReceiver.ReceiveAsync();

                    if (eventData == null) continue;

                    string offset = eventData.Offset;
                    string content = Encoding.UTF8.GetString(eventData.GetBytes());
                    if (!string.IsNullOrWhiteSpace(offset) &&
                        !string.IsNullOrWhiteSpace(content))
                    {
                        Console.WriteLine($"Message received. Partition - [{partitionID}]");
                        Console.WriteLine($"Received message offset - [{offset}]");
                        Console.WriteLine($"Received message content\n");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{content}\n");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Event data has no message content !");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred !");
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Recieve message from IoT Device
        /// </summary>
        /// <param name="eventHubClient">EventHubClient object value</param>
        private void ReceiveMessagesFromDeviceAsync(EventHubClient eventHubClient)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            try
            {
                Console.WriteLine("About to receive messages. Press Ctrl-C to exit.\n");

                var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

                CancellationTokenSource cts = new CancellationTokenSource();

                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cts.Cancel();
                    Console.WriteLine("Exiting...");
                    return;
                };

                var tasks = new List<Task>();
                foreach (string partition in d2cPartitions)
                {
                    tasks.Add(ReceiveMessagesFromDeviceAsync(eventHubClient, partition, cts.Token));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred !");
                Console.WriteLine(exception);
            }
        }

        #endregion
    }
}
