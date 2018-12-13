namespace PostProcessorModule
{
    using System;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;
    using Sql = System.Data.SqlClient;
    using System.Diagnostics;

    class Program
    {
        static int counter;
        private static Stopwatch stopwatch;


        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
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

            // Register callback to be called when a message is received by the module
            await ioTHubModuleClient.SetInputMessageHandlerAsync("input1", PipeMessage, ioTHubModuleClient);
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
            string newstring = messageString.Replace("\\", "");
            newstring = newstring.Replace("[\"", "[");
            newstring = newstring.Replace("\"]", "]");
            newstring = newstring.Trim('\\');
            newstring = newstring.Trim(Mychar);

            Console.WriteLine($"Received message: {counterValue}, Body: {newstring}");
            var messageBody = JsonConvert.DeserializeObject<MessageBody>(newstring);

            if (!string.IsNullOrEmpty(messageBody.Cep))
            {
                UpdateSQLTable(messageBody.Cep, messageBody.Predicted);
            }

            MessageBody new_messageBody = new MessageBody
            {
                LineName = messageBody.LineName,
                Raw = messageBody.Raw,
                Cep = messageBody.Cep,
                Predicted = messageBody.Predicted
            };

            messageString = JsonConvert.SerializeObject(new_messageBody);
            Console.WriteLine($"Serialzing : {new_messageBody.LineName}, {new_messageBody.Raw}, {new_messageBody.Cep}, {new_messageBody.Predicted}");
            await SendMessage(messageString, message, moduleClient);

            // StopTime
            stopwatch.Stop();
            string logmessage = " " + counter + " Process Complete : Time elapsed: {" + stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\:ffff") + "}";
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, logmessage);
            Console.WriteLine(logmessage);

            stopwatch.Reset();

            return MessageResponse.Completed;
        }
        static async Task<bool> SendMessage(string p_messageString, Message message, ModuleClient p_moduleclient)
        {
            bool resultbool = false;
            if (!string.IsNullOrEmpty(p_messageString))
            {
                var messageBytes = Encoding.UTF8.GetBytes(p_messageString);

                var pipeMessage = new Message(messageBytes);
                pipeMessage.ContentEncoding = "utf-8";
                pipeMessage.ContentType = "application/json";
                foreach (var prop in pipeMessage.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                }
                await p_moduleclient.SendEventAsync("output1", pipeMessage);
                resultbool = true;
            }
            return resultbool;
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
