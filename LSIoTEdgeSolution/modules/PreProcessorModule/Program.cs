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
        private static Stopwatch mywatch;
        private static ModuleManager moduleManager;
        // private static volatile DesiredPropertiesData desiredPropertiesData;
        // private static volatile bool IsReset = false;

        static void Main(string[] args)
        {
            mywatch = new Stopwatch();
            mywatch.Start();

            moduleManager = new ModuleManager();
            moduleManager.Init();

            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            AmqpTransportSettings amqpSetting = new AmqpTransportSettings(TransportType.Amqp_Tcp_Only);
            ITransportSettings[] settings = { amqpSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "IoT Hub module client initialized.");

            var moduleTwin = await ioTHubModuleClient.GetTwinAsync();
            var moduleTwinCollection = moduleTwin.Properties.Desired;
            // as this runs in a loop we don't await
            SendSimulationData(ioTHubModuleClient);
        }

        private static async Task SendSimulationData(ModuleClient deviceClient)
        {
            while (true)
            {
                try
                {

                    var messageBody = LogBuilder.AssignTempMessageBody("");
                    var messageString = JsonConvert.SerializeObject(messageBody);

                    if (messageString != string.Empty)
                    {
                        var logstring = "@@@@@@@@@" + messageString + "";
                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, logstring);
                        var messageBytes = Encoding.UTF8.GetBytes(messageString);
                        var message = new Message(messageBytes);
                        message.ContentEncoding = "utf-8";
                        message.ContentType = "application/json";

                        await deviceClient.SendEventAsync("messageOutput", message);

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Unexpected Exception {ex.Message}");
                    Console.WriteLine($"\t{ex.ToString()}");
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        /// <summary>
        /// This method is called whenever the module is sent a message from the EdgeHub. 
        /// It just pipe the messages without any change.
        /// It prints all the incoming messages.
        /// </summary>
        static async Task<MessageResponse> PipeMessage(Message message, object userContext)
        {
            int counterValue = Interlocked.Increment(ref counter);

            var moduleClient = userContext as ModuleClient;
            if (moduleClient == null)
            {
                throw new InvalidOperationException("UserContext doesn't contain " + "expected values");
            }

            byte[] messageBytes = message.GetBytes();
            string messageString = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine($"Received message: {counterValue}, Body: [{messageString}]");

            if (!string.IsNullOrEmpty(messageString))
            {
                var pipeMessage = new Message(messageBytes);
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }
    }
}
