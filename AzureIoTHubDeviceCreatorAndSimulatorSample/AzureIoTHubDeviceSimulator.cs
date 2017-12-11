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

using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace AzureIoTHubSample
{
    /// <summary>
    /// Azure IoT Hub Device Simulator 
    /// </summary>
    public class AzureIoTHubDeviceSimulator
    {

        #region Public Methods 

        /// <summary>
        /// Process IoT Hub Simulator
        /// </summary>
        /// <returns>Taks object</returns>
        public async Task ProcessIoTHubSimulatorAsync()
        {
            RegistryManager registryManager;

            if (TryToCreateIoTDeviceRegistry(out registryManager))
            {
                Device device = null;
                if (TryToAddOrGetDevice(registryManager, out device))
                {
                    DeviceClient deviceClient = null;
                    if (TryToCreateDeviceClient(device, out deviceClient))
                    {
                        Console.WriteLine("Delay 10 seconds before sending messages. Helps IoT message reciever to be ready !\n");
                        await Task.Delay(10000);
                        await Send10MessagesToDeviceAsync(device, deviceClient);
                    }
                }
            }
        }

        #endregion

        #region Private Method 

        /// <summary>
        /// Try to create IoT Device Registry Manager
        /// </summary>
        /// <param name="registryManager">RegistryManager object value</param>
        /// <returns>true/false</returns>
        private bool TryToCreateIoTDeviceRegistry(out RegistryManager registryManager)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            registryManager = null;
            try
            {
                registryManager = RegistryManager.CreateFromConnectionString(ConfigurationManager.AppSettings.Get("IoTHubConnectionString"));
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred !");
                Console.WriteLine(exception);
                return false;
            }
            Console.WriteLine("IoT Device Registery Manager successfully created !\n");
            return true;
        }

        /// <summary>
        /// Try to create IoT Device
        /// </summary>
        /// <param name="registryManager">RegistryManager object value</param>
        /// <param name="device">Device object value</param>
        /// <returns>true/false</returns>
        private bool TryToAddOrGetDevice(RegistryManager registryManager, out Device device)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            string azureIoTDeviceID = ConfigurationManager.AppSettings.Get("DeviceID");
            device = null;
            bool checkDeviceStatus = false;
            try
            {
                device = registryManager.AddDeviceAsync(new Device(azureIoTDeviceID)).Result;
                Console.WriteLine($"IoT device name [{device.Id}] successfully added !\n");
                checkDeviceStatus = true;
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null &&
                    exception.InnerException is DeviceAlreadyExistsException)
                {
                    device = registryManager.GetDeviceAsync(azureIoTDeviceID).Result;
                    Console.WriteLine($"IoT device name [{device.Id}] added successfully retrieved !\n");
                    checkDeviceStatus = true;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error occurred !");
                    Console.WriteLine(exception);
                }
            }
            if (checkDeviceStatus)
            {
                Console.WriteLine($"Generated device key - {device.Authentication.SymmetricKey.PrimaryKey}\n");
            }
            return checkDeviceStatus;
        }

        /// <summary>
        /// Try to create IoT Device Client
        /// </summary>
        /// <param name="device">Device object value</param>
        /// <param name="deviceClient">DeviceClient object value</param>
        /// <returns>true/false</returns>
        private bool TryToCreateDeviceClient(Device device, out DeviceClient deviceClient)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            deviceClient = null;
            try
            {
                deviceClient = DeviceClient.Create(ConfigurationManager.AppSettings.Get("IoTHostName"),
                    new DeviceAuthenticationWithRegistrySymmetricKey(device.Id,
                    device.Authentication.SymmetricKey.PrimaryKey), Microsoft.Azure.Devices.Client.TransportType.Mqtt);
                deviceClient.ProductInfo = "HappyPath_Simulated-CSharp";
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred !");
                Console.WriteLine(exception);
                return false;
            }
            Console.WriteLine("IoT Device Client successfully created !");
            return true;
        }

        /// <summary>
        /// Send 10 Messages to Device
        /// </summary>
        /// <param name="device">Device object value</param>
        /// <param name="deviceClient">DeviceClient object value</param>
        /// <returns>Task object</returns>
        private async Task Send10MessagesToDeviceAsync(Device device, DeviceClient deviceClient)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();
            int maxMessagesToPublish = 10;
            int successMessageSentCounter = 0;
            Console.WriteLine("About to send messages to IoT Device !\n");
            for (var counter = 0; counter < maxMessagesToPublish; counter++)
            {

                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;

                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    deviceId = device.Id,
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint, Formatting.Indented);
                var message = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");
                try
                {
                    await deviceClient.SendEventAsync(message);
                    successMessageSentCounter++;
                    Console.WriteLine($"Sending message [{successMessageSentCounter}] to IoT Device !\n");
                    Console.WriteLine($"Sent message content\n");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{messageString}");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Delay 1 seconds for the send the next message !\n");
                    await Task.Delay(1000);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error occurred !");
                    Console.WriteLine(exception);
                }
                if (successMessageSentCounter > 0)
                {
                    Console.WriteLine($"[{successMessageSentCounter}] messages sent successfully !\n");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No message sent to IoT Device !");

                }
            }
        }

        #endregion

    }
}
