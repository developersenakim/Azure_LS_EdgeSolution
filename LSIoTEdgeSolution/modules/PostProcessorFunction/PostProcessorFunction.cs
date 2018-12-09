using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EdgeHub;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Functions.Samples
{
    public static class PostProcessorFunction
    {
        private static Stopwatch stopwatch;

        [FunctionName("PostProcessorFunction")]
        public static async Task FilterMessageAndSendMessage([EdgeHubTrigger("input1")] Message messageReceived, [EdgeHub(OutputName = "output1")] IAsyncCollector<Message> output, ILogger logger)
        {

            stopwatch = new Stopwatch();
            stopwatch.Start();

            SQLClass sqlclass;
            Environment currentEnvironment;
            LogBuilder logBuilder;

            string logmessage = string.Empty;
            currentEnvironment = Environment.productionOnlinux;
            string configfile = "";
            string logpath = "/app/documents/";
            string dbname = "LS_IoTEDGE";
            string tablename = "T_NG"; // NG_TABLE  
            string sqlconnectionstring = "Data Source=tcp:sql,1433;User Id=SA;Password=Strong!Passw0rd;TrustServerCertificate=False;Connection Timeout=30;";

            if (currentEnvironment == Environment.productionOnlinux)
            {
                logpath = "/app/documents/";
                configfile = "/app/documents/config.txt";
                sqlconnectionstring = "Data Source=tcp:sql,1433;User Id=SA;Password=Strong!Passw0rd;TrustServerCertificate=False;Connection Timeout=30;";
            }
            else if (currentEnvironment == Environment.testOnWindow)
            {
                logpath = @"C:\Users\sena.kim\Documents\Projects\LS산전\Azure_LS_EdgeSolution\LSIoTEdgeSolution\config\";
                configfile = "C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\Azure_LS_EdgeSolution\\LSIoTEdgeSolution\\config\\config.txt";
            }
            logBuilder = new LogBuilder(logpath);
            sqlclass = new SQLClass(configfile, logBuilder, sqlconnectionstring);
            sqlclass.SetSqlNameAndTable(dbname, tablename);
            /////////////////////////////////////////////////////////////////////////


            byte[] messageBytes = messageReceived.GetBytes();
            var messageString = System.Text.Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                logger.LogInformation("Info: Received one non-empty message");
                var messageBody = JsonConvert.DeserializeObject<MessageBody>(messageString);

                if (messageBody != null && !string.IsNullOrEmpty(messageBody.Cep))
                {
                    sqlclass.CheckSqlConnection();
                    sqlclass.UpdateTableInSQL(messageBody.Cep, messageBody.Predicted); // this checks if there is same cep filename . 
                    // Send the message to the output as the temperature value is greater than the threashold.
                    var pipeMessage = new Message(messageBytes);
                    // Copy the properties of the original message into the new Message object.
                    foreach (KeyValuePair<string, string> prop in messageReceived.Properties)
                    { pipeMessage.Properties.Add(prop.Key, prop.Value); }
                    // Send the message.       
                    await output.AddAsync(pipeMessage);
                    logger.LogInformation("Info: Received from ML");
                }
            }
        }
    }
}