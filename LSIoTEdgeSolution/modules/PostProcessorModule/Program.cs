namespace PostProcessorModule
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
    using Sql = System.Data.SqlClient;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;

    class Program
    {
        static int counter;

        static void Main(string[] args)
        {
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
            Console.WriteLine("IoT Hub module client initialized.");

            bool repeat = true;
            while (repeat)
            {                // Register callback to be called when a message is received by the module
                await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
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
            char[] Mychar = { '[', ']' };
            char[] Mychar1 = { '"' };
            // string  newstring = messageString.TrimStart(Mychar1);
            // newstring = newstring.TrimEnd(Mychar1);
            string newstring = messageString.Replace("\\", "");
            newstring = newstring.Replace("[\"", "[");
            newstring = newstring.Replace("\"]", "]");
            newstring = newstring.Trim('\\');
            newstring = newstring.Trim(Mychar);

            Console.WriteLine($"Received message: {counterValue}, Body: {newstring}");
            var messageBody = JsonConvert.DeserializeObject<MessageBody>(newstring);

            //////////////////DO HERE
            // if condition to check if cep exist and if exist 
            // -- await
            //UpdateSQLTable(messageBody.Cep, messageBody.Predicted);
            ///////////////////////////////

            MessageBody new_messageBody = new MessageBody
            {
                LineName = messageBody.LineName,
                Raw = messageBody.Raw,
                Cep = messageBody.Cep,
                Predicted = messageBody.Predicted
            };

            messageString = JsonConvert.SerializeObject(new_messageBody);
            if (!string.IsNullOrEmpty(messageString))
            {
                messageBytes = Encoding.UTF8.GetBytes(messageString);

                var pipeMessage = new Message(messageBytes);
                pipeMessage.ContentEncoding = "utf-8";
                pipeMessage.ContentType = "application/json";
                foreach (var prop in message.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await moduleClient.SendEventAsync("output1", pipeMessage);
                Console.WriteLine("Received message sent");
            }
            return MessageResponse.Completed;
        }


        static public void UpdateSQLTable(string p_ceplocation, string p_resultValue)
        {

            string m_dbname = "LS_IoTEDGE";
            string m_tablename = "T_NG"; // NG_TABLE           
            string m_connectionstring = "Data Source=tcp:sql,1433;User Id=SA;Password=Strong!Passw0rd;TrustServerCertificate=False;Connection Timeout=30;";
            string m_resultValue = p_resultValue;
            string m_ceplocation = p_ceplocation;

            string temp_UpdateTableInSQLstring = $"UPDATE [{m_dbname}].[dbo].[{m_tablename}] SET [재판정결과]='{m_resultValue}' WHERE [CEPLocation] = '{m_ceplocation}';";
            try
            {
                using (Sql.SqlConnection connection = new Sql.SqlConnection())
                {
                    connection.ConnectionString = m_connectionstring;
                    connection.Open();
                    using (Sql.SqlCommand command = new Sql.SqlCommand(temp_UpdateTableInSQLstring, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Sql.SqlException exception)
            {
                Console.WriteLine("ConnectionString: {0}", exception);
            }
        }

        static public void ReadContentfromConfigAndReturnStringReference(string p_filePath, string splitstring, ref string var)
        {
            if (System.IO.File.Exists(p_filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(p_filePath);
                // System.Console.WriteLine("Contents of WriteLines2.txt = ");
                foreach (string line in lines)
                {
                    if (line.StartsWith(splitstring))
                    {
                        string tempstring = line.Split(splitstring)[1];
                        tempstring = tempstring.TrimStart();
                        tempstring = tempstring.TrimEnd();
                        var = tempstring;
                    }
                }
            }
        }// end of  public void ReadContentfromConfig(string filepath, string splitstring, ref string var)



        public class MessageBody
        {
            [JsonProperty("line")]
            public string LineName { get; set; }
            [JsonProperty("raw")]
            public string Raw { get; set; }
            [JsonProperty("cep")]
            public string Cep { get; set; }//True False Unkown

            [JsonProperty("predicted")]
            public string Predicted { get; set; }//True False Unkown

        }
    }
}
