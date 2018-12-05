// Copyright (c) Bespinglobal Corporation. All rights reserved.

using Newtonsoft.Json;


using System;
using System.Threading;
using System.Diagnostics;




using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Shared;

using System.Net;
using System.Globalization;
using System.Linq;
namespace PreProcessorModule
{
    public class ModuleManager
    {
        private string m_configPath { get; set; }
        private string m_sqlConnectionString;
        public string m_shareFolderLocation;
        private string m_logPath;
        private int m_numberOfLines;
        LineStatus[] m_Linestatus;
        bool m_IsProcessComplete;
        public Queue<ModuleMessageBody> m_ModuleMessageBody { get; set; }

        public ModuleManager()
        {
            m_configPath = "/app/documents/config.txt";//"C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\LsisIotPoC_IoTEdgeSolution\\EdgeSolution\\modules\\processorModule\\obj\\config.txt";
            m_sqlConnectionString = "";
            m_shareFolderLocation = "";
            m_logPath = "";
            m_numberOfLines = 0;
            m_IsProcessComplete = false;
            m_ModuleMessageBody = new Queue<ModuleMessageBody>();

        }


        /// <summary>
        /// Initialize, and Assign some variables.!-- 
        /// </summary>
        public bool Init()
        {
            // Assigns some variables.           
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "SQLconnectionString:", ref m_sqlConnectionString);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "SharedFolderPath:", ref m_shareFolderLocation);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "LogPath:", ref m_logPath);
           
            bool isApplicationSafeToContinue = DirectoryReader.IsDirectoryExistInThefolder(m_shareFolderLocation);

            if (isApplicationSafeToContinue == true)
            {
                DirectoryInfo[] sharedFolderDirectoryInfo = DirectoryReader.Readfromfolder(m_shareFolderLocation);// thisfolder reads location finding how many lines exist . 
                m_numberOfLines = sharedFolderDirectoryInfo.Length;
                m_Linestatus = new LineStatus[m_numberOfLines];

                for (int i = 0; i < m_numberOfLines; i++)// Access Each line folder. 
                {
                    m_Linestatus[i] = new LineStatus();
                    m_Linestatus[i].AssignLineStatus(m_shareFolderLocation, sharedFolderDirectoryInfo[i].Name, i);
                }// end of for 
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Module Initialization Complete. Application is safe to continue.");
            }
            else
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Module Initialization Failed.");
            }
            return isApplicationSafeToContinue;
        }// end of init
         // line raw filename// cep // cep file name. 

        public void Process()
        {
            for (int i = 0; i < m_numberOfLines; i++)// Access Each line folder. 
            {
                Queue<ModuleMessageBody> tempMessageBody = m_Linestatus[i].ProcessMultipleDateFolderInfo();
                tempMessageBody.TrimExcess();
                if (tempMessageBody.Count() > 0)
                {
                    foreach (var messageStructure in tempMessageBody)
                    {
                        if (messageStructure.LineName != string.Empty && messageStructure.Raw != string.Empty && messageStructure.Cep != string.Empty)
                        {
                            m_ModuleMessageBody.Enqueue(new ModuleMessageBody() { LineName = messageStructure.LineName, Raw = messageStructure.Raw, Cep = messageStructure.Cep, Aps = messageStructure.Aps });
                        }
                    }
                }
            }// end of for       


            m_IsProcessComplete = true;
            while (m_IsProcessComplete == false)
            {
                // This while loop waits for Threadpool event for all  thread to complete
            }
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Process Complete");



        }
    }
}