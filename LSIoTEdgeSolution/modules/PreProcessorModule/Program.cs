namespace PreProcessorModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using System.Collections.Generic;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    using Microsoft.Azure.Devices.Shared;
    using Newtonsoft.Json;
    using System.Net;
    using System.Diagnostics;

    // disabling async warning as the SendSimulationData is an async method
    // but we don't wait for it
#pragma warning disable CS4014
    class Program
    {
        static int counter;
        private static Stopwatch stopwatch;
        private static ModuleManager moduleManager;
        // private static volatile DesiredPropertiesData desiredPropertiesData;
        // private static volatile bool IsReset = false;

        static void Main(string[] args)
        {
            string logmessage = string.Empty;
            stopwatch = new Stopwatch();
            stopwatch.Start();

            moduleManager = new ModuleManager();
            moduleManager.Init();

            ModuleClient moduleclient = ConnectionManager.Init().Result;
            ConnectionManager.SendData(moduleclient).Wait();

            stopwatch.Stop();
            logmessage = "Time elapsed: " + stopwatch.Elapsed;
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, logmessage);

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            ConnectionManager.WhenCancelled(cts.Token).Wait();
        }
    }
}
