using System.IO;
using System.Collections.Generic;
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
        public LineStatus[] m_Linestatus { get; set; }

        public List<ModuleMessageBody> m_totalMessageBodiesOfAllLines { get; set; }

        public ModuleManager(string configPath)
        {
            m_configPath = configPath;//"C:\\Users\\sena.kim\\Documents\\Projects\\LS산전\\LsisIotPoC_IoTEdgeSolution\\EdgeSolution\\modules\\processorModule\\obj\\config.txt";
            m_sqlConnectionString = "";
            m_shareFolderLocation = "";
            m_logPath = "";
            m_numberOfLines = 0;
            m_totalMessageBodiesOfAllLines = new List<ModuleMessageBody>();

        }

        public void Clear()
        {
            m_totalMessageBodiesOfAllLines.Clear();
            for (int i = 0; i < m_numberOfLines; i++)// Access Each line folder. 
            {
                m_Linestatus[i].Clear();
            }// end of for 
             m_totalMessageBodiesOfAllLines = new List<ModuleMessageBody>();
        }


        /// <summary>
        /// Initialize, and Assign some variables.!-- 
        /// </summary>
        public void Init()
        {
            string reportfolderName = string.Empty;
            string aifolderName = string.Empty;
            string apsfolderName = string.Empty;
            string cepfolderName = string.Empty;
            string rawfolderName = string.Empty;

            // Assigns some variables.           
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "SQLconnectionString:", ref m_sqlConnectionString);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "SharedFolderPath:", ref m_shareFolderLocation);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "LogPath:", ref m_logPath);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "ReportFolderName:", ref reportfolderName);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "AiFolderName:", ref aifolderName);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "APSFolderName:", ref apsfolderName);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "CepFolderName:", ref cepfolderName);
            DirectoryReader.ReadContentfromConfigAndReturnStringReference(m_configPath, "RawFolderName:", ref rawfolderName);

            bool isApplicationSafeToContinue = DirectoryReader.IsDirectoryExistInThefolder(m_shareFolderLocation);

            if (isApplicationSafeToContinue == true)
            {
                DirectoryInfo[] sharedFolderDirectoryInfo = DirectoryReader.Readfromfolder(m_shareFolderLocation);// thisfolder reads location finding how many lines exist . 
                m_numberOfLines = sharedFolderDirectoryInfo.Length;
                m_Linestatus = new LineStatus[m_numberOfLines];

                for (int i = 0; i < m_numberOfLines; i++)// Access Each line folder. 
                {
                    m_Linestatus[i] = new LineStatus(apsfolderName, cepfolderName, rawfolderName, m_shareFolderLocation, sharedFolderDirectoryInfo[i].Name, i, reportfolderName, aifolderName);
                    m_Linestatus[i].AssignLineStatus(m_shareFolderLocation, sharedFolderDirectoryInfo[i].Name, i, reportfolderName, aifolderName);

                }// end of for 
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Module Initialization Complete. Application is safe to continue.");
            }
            else
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Module Initialization Failed.");
            }

        }// end of init

        // line raw filename// cep // cep file name. 
        public string GetsqlConnectionString()
        {
            return m_sqlConnectionString;

        }
        // line raw filename// cep // cep file name. 
        public void ProcessToAssignModuleMessageBody(SQLClass p_sqlclass, Environment p_currentEnvironment)
        {
            bool isthisNewDay = false;
            for (int i = 0; i < m_numberOfLines; i++)// Access Each line folder. 
            {
                isthisNewDay = m_Linestatus[i].ProcessSingleDateFolderInfo(p_currentEnvironment);
                if (isthisNewDay == true)
                {
                    p_sqlclass.TruncateTable("[LS_IoTEDGE].[dbo].[T_NG]");
                }

                m_Linestatus[i].m_ModuleMessageBody.TrimExcess();
                foreach (var messageStructure in m_Linestatus[i].m_ModuleMessageBody)
                {
                    m_totalMessageBodiesOfAllLines.Add(messageStructure);
                }
            }// end of for       
        }
    }
}