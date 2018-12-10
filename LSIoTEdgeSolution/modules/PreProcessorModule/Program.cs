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

//#pragma warning disable CS4014
namespace PreProcessorModule
{
    class Program
    {
        private static Stopwatch stopwatch;

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();

            ModuleManager moduleManager = null;
            ModuleClient moduleclient = null;
            SQLClass sqlclass;
            Environment currentEnvironmet;


            int count = 0;
            string logmessage = string.Empty;
            currentEnvironmet = Environment.productionOnlinux;
            moduleManager = new ModuleManager("");
            string configfile = "";
            bool repeat = true;

            if (currentEnvironmet == Environment.productionOnlinux)
            {
              
                configfile = "/app/documents/config.txt";
                moduleclient = ConnectionManager.Init().Result;
                moduleManager = new ModuleManager("/app/documents/config.txt");


            }
            else if (currentEnvironmet == Environment.testOnWindow)
            {
                configfile = "C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\Azure_LS_EdgeSolution\\LSIoTEdgeSolution\\config\\config.txt";
                moduleManager = new ModuleManager("C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\Azure_LS_EdgeSolution\\LSIoTEdgeSolution\\config\\config.txt");
            }
            moduleManager.Init();
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, configfile);

            sqlclass = new SQLClass(moduleManager.GetsqlConnectionString());
            sqlclass.CheckSqlConnection();
            CreateDBAndNGTable(sqlclass, currentEnvironmet);


            ////////////////////////////////////////// Initialization Complete ////////////////////////////
            logmessage = "Initialization complete : Time elapsed: {" + stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\:ff") + "}"; //local test : {00:00:00:38}
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, logmessage);

            while (repeat == true)//count < 1)//true
            {
                count++;
                Process(moduleManager, currentEnvironmet, sqlclass, moduleclient);

                ////////////////////////////////////////// Process Complete ////////////////////////////
                moduleManager.Clear();
                sqlclass.CloseSQL();// only print messages that contains raw cep aps
                stopwatch.Stop();  // Stop
                if (count == 1)
                {// Write hours, minutes , seconds , milliseconds/.
                    logmessage = "All Process Complete For the First Time : Time elapsed: {" + stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\:ff") + "}";  // Write hours, minutes , seconds , milliseconds/.
                }
                else
                {
                    logmessage = "Process Complete : Time elapsed: {" + stopwatch.Elapsed.ToString("hh\\:mm\\:ss\\:ff") + "}";  // Write hours, minutes , seconds , milliseconds/.
                }
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, logmessage);
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Resetting watch...");
                stopwatch.Reset();
            }
            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            ConnectionManager.WhenCancelled(cts.Token).Wait();

        }

        static void CreateDBAndNGTable(SQLClass p_sqlclass, Environment p_environment)
        {
            string dbname = "LS_IoTEDGE";
            string tablename = "T_NG"; // NG_TABLE          

            string dbfilepath = string.Empty;
            if (p_environment == Environment.productionOnlinux)
            {
                dbfilepath = "'/var/opt/mssql/lsiotedge.mdf'";
            }
            else if (p_environment == Environment.testOnWindow)
            {
                dbfilepath = @"'C:\LSIoTEdgeSolution\config\lsiotedge.mdf'";
            }
            p_sqlclass.CreateDBInSQL(dbname, dbfilepath);
            p_sqlclass.CreateTableInSQL(dbname, tablename, "(LINE varchar(50), 시험일자 varchar(50), Model varchar(50), BarCode varchar(50), 재판정결과 varchar(50), CREATEDT datetime, RAWLocation varchar(250), CEPLocation varchar(250), APSLocation varchar(250))");
            p_sqlclass.SetSqlNameAndTable(dbname, tablename);
        }

        static void Process(ModuleManager p_moduleManager, Environment p_currentEnvironment, SQLClass p_sqlclass, ModuleClient p_moduleclient)
        {            //this is being looped this

            string tempmessage = "Start Processing Line reports.";
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, tempmessage);

            p_moduleManager.ProcessToAssignModuleMessageBody(p_sqlclass, p_currentEnvironment);
            p_moduleManager.m_totalMessageBodiesOfAllLines.TrimExcess();
            int tempMax = p_moduleManager.m_totalMessageBodiesOfAllLines.Count;
            int count = 0;
            foreach (var temp in p_moduleManager.m_totalMessageBodiesOfAllLines)
            {
                count++;
                var messageBody = LogBuilder.AssignTempMessageBody(temp.LineName, temp.Raw, temp.Cep);
                var messageString = JsonConvert.SerializeObject(messageBody);
                tempmessage = $"This ID : {count} Line Name {temp.LineName}, Barcode {temp.BadProductInfo.BarCode}";

                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, tempmessage);


                if (p_sqlclass.ReadSQL($"SELECT COUNT([BarCode]) AS ISEIXSTS FROM [LS_IoTEDGE].[dbo].[T_NG]	WHERE [BarCode] = '{ temp.BadProductInfo.BarCode}'	;") == "0")
                {
                    Random _r = new Random();
                    int a = _r.Next(2);
                    if (a == 0)
                    {
                        temp.BadProductInfo.Result = "NG";
                    }
                    else if (a == 1)
                    {
                        temp.BadProductInfo.Result = "OK";
                    }
                    //Chek if the data already exist 
                    p_sqlclass.InsertTableInSQL(temp.LineName, temp.BadProductInfo.Date, temp.BadProductInfo.Model, temp.BadProductInfo.BarCode, temp.BadProductInfo.Result, temp.Raw, temp.Cep, temp.Aps);

                    if (p_currentEnvironment == Environment.productionOnlinux)
                    {
                        ConnectionManager.SendData(p_moduleclient, messageString).Wait();
                    }
                    else if (p_currentEnvironment == Environment.testOnWindow)
                    {
                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, messageString);
                    }
                }
            }

        }// end of Process void
    }
}
