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

namespace AzureIoTHubSample
{
    class Program
    {

        static void Main(string[] args)
        {
            new AzureIoTHubDeviceSimulator().ProcessIoTHubSimulatorAsync().Wait();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
