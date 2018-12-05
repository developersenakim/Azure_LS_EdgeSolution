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

//#pragma warning disable CS4014

#pragma warning disable CS4014
namespace PreProcessorModule
{
    class Program
    {
        private static Stopwatch stopwatch;

        private enum Environment
        {
            productionOnlinux,
            testOnWindow,
        }

        static void Main(string[] args)
        {
            Environment currentEnvironmet = Environment.testOnWindow;
            ModuleManager moduleManager = new ModuleManager("");

            stopwatch = new Stopwatch();
            stopwatch.Start();

            if (currentEnvironmet == Environment.productionOnlinux)
            {
                ModuleClient moduleclient = ConnectionManager.Init().Result;
                moduleManager = new ModuleManager("/app/documents/config.txt");
            }
            else if (currentEnvironmet == Environment.testOnWindow)
            {
                moduleManager = new ModuleManager("C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\Azure_LS_EdgeSolution\\LSIoTEdgeSolution\\config\\config.txt");
            }

            moduleManager.Init();



            //loop this
            moduleManager.ProcessAssignModuleMessageBody();

            for (int i = 0; i <= moduleManager.m_ModuleMessageBody.Count; i++)
            {
                ModuleMessageBody tempModulemessageBody = moduleManager.m_ModuleMessageBody.Dequeue();
                var messageBody = LogBuilder.AssignTempMessageBody(tempModulemessageBody.LineName, tempModulemessageBody.Raw, tempModulemessageBody.Cep);
                var messageString = JsonConvert.SerializeObject(messageBody);

                if (currentEnvironmet == Environment.productionOnlinux)
                {
                   // ConnectionManager.SendData(moduleclient, messageString).Wait();
                }
                else if (currentEnvironmet == Environment.testOnWindow)
                {
                   LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual,messageString);
                }
            }

            SQLClass sqlclass = new SQLClass(moduleManager.GetsqlConnectionString());
            sqlclass.CheckSqlConnection();





            ////////////Process            
            // ConnectionManager.SendData(moduleclient).Wait();


            // // Wait until the app unloads or is cancelled
            // var cts = new CancellationTokenSource();
            // AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            // Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            // ConnectionManager.WhenCancelled(cts.Token).Wait();

        }
    }
}
