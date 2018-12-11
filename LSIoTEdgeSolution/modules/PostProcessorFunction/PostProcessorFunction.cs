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
using Sql = System.Data.SqlClient;
namespace Functions.Samples
{
    public static class PostProcessorFunction
    {
        [FunctionName("PostProcessorFunction")]
        public static async Task FilterMessageAndSendMessage(
                    [EdgeHubTrigger("input1")] Message messageReceived,
                    [EdgeHub(OutputName = "output1")] IAsyncCollector<Message> output,
                    ILogger logger)
        {
            ///////////////////initialize some parameters for updating sql
            // string m_dbname = "LS_IoTEDGE";
            // string m_tablename = "T_NG"; // NG_TABLE   
            // string m_connectionstring = "Data Source=tcp:sql,1433;User Id=SA;Password=Strong!Passw0rd;TrustServerCertificate=False;Connection Timeout=30;";
            // string m_resultValue = "";
            // string m_ceplocation = "";
            ///////////////////initialization Complete

            byte[] messageBytes = messageReceived.GetBytes();
            var messageString = System.Text.Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrEmpty(messageString))
            {
                // logger.LogInformation("messageString Is not null");

                // char[] Mychar = { '[', ']' };
                // char[] Mychar1 = { '"' };
                // // string  newstring = messageString.TrimStart(Mychar1);
                // // newstring = newstring.TrimEnd(Mychar1);
                // string newstring = messageString.Replace("\\", "");
                // newstring = newstring.Replace("[\"", "[");
                // newstring = newstring.Replace("\"]", "]");
                // newstring = newstring.Trim('\\');
                // newstring = newstring.Trim(Mychar);

                // logger.LogInformation($"Received message: {newstring}");
                // try
                // {
                //     var messageBody = JsonConvert.DeserializeObject<MessageBody>(newstring);
                //     string print = $" MessageBody {messageBody.Cep} Result {messageBody.Predicted}";
                //     logger.LogInformation(print);
                // }
                // catch (JsonSerializationException ex)
                // {
                //     logger.LogInformation("ConnectionString: {0}", ex);
                // }


                // //Store the data in SQL db
                // //p_resultValue OK or NG            // barcode = P0104087055046EDM001_00013470280 // update table

                // string temp_UpdateTableInSQLstring = $"UPDATE [{m_dbname}].[dbo].[{m_tablename}] SET [재판정결과]='{m_resultValue}' WHERE [CEPLocation] = '{m_ceplocation}';";
                // try
                // {
                //     using (Sql.SqlConnection connection = new Sql.SqlConnection())
                //     {
                //         connection.ConnectionString = m_connectionstring;
                //         connection.Open();
                //         using (Sql.SqlCommand command = new Sql.SqlCommand(temp_UpdateTableInSQLstring, connection))
                //         {
                //             command.ExecuteNonQuery();
                //         }
                //         connection.Close();
                //     }
                // }
                // catch (Sql.SqlException exception)
                // {
                //     logger.LogInformation("ConnectionString: {0}", exception);
                // }

                logger.LogInformation("Info: Received one non-empty message");
                var pipeMessage = new Message(messageBytes);
                foreach (KeyValuePair<string, string> prop in messageReceived.Properties)
                {
                    pipeMessage.Properties.Add(prop.Key, prop.Value);
                    Console.WriteLine(prop.Key + prop.Value);
                }
                await output.AddAsync(pipeMessage);
                logger.LogInformation("Info: Piped out the message");
            }
        }//end of public

    }

    

    class MessageBody
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