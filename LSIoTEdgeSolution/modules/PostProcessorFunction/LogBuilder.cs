using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace  Functions.Samples
{
    public class LogBuilder
    {
        string m_logpath;
        public enum MessageStatus
        {
            Usual,
            Warning,
            Error
        }

        public LogBuilder(string p_logpath )
        {
            m_logpath = p_logpath;

        }

        public void LogWrite(MessageStatus messageStatus, string logMessage)
        {
            try
            {
                using (StreamWriter w = File.AppendText(m_logpath + "PreProcessorlog.txt"))
                {
                    WriteOnConsole(logMessage, messageStatus, w);
                }
            }
            catch (Exception)
            {
            }
        }
        //public Format of YYYYMMDD
        public DateTime ParseStringToGetDateTime(string dateString, string dateFormat)
        {
            CultureInfo koKR = new CultureInfo("ko-KR");
            DateTime dateValue = new DateTime();
            string tempmessage = "";

            if (DateTime.TryParseExact(dateString, dateFormat, koKR, DateTimeStyles.None, out dateValue))
            {
                //  tempmessage = "Converted " + dateString + " to " + dateValue + "," + dateValue.Kind.ToString();
            }
            else
            {
                tempmessage = dateString + "Converting Failed";

                LogWrite(LogBuilder.MessageStatus.Error, tempmessage);
            }
            return dateValue;
        }

        //public Format of YYYYMMDD
        public DateTime GetKoreanFormatTime()
        {
            DateTime localDate = DateTime.Now;
            String cultureName = "ko-KR";
            var culture = new CultureInfo(cultureName);

            return localDate;
        }

        public string ParseDateTimeToString(DateTime p_datetime, string p_format)
        {
            string s_date;
            s_date = p_datetime.ToString(p_format, DateTimeFormatInfo.InvariantInfo);

            return s_date;
        }
        ///<summary>
        ///* Function:  function to Comparing Two DAte Time if reservedworkingdatetime is earlier than the toda's date it will retun reserved working folder else toda'sdate.!-- 
        ///* @author: Sena.kim
        ///* @parameter: DATETIME reservedwrokingfolder, DATETIME, toda
        ///* @return: DATETIME
        ///</summary>
        public DateTime CompareTwoDateTimeToGetDateTimeToWork(DateTime p_reservedWorkingFolder, DateTime p_todaysDate)
        {
            DateTime resultTime = p_reservedWorkingFolder;
            int result = DateTime.Compare(p_reservedWorkingFolder, p_todaysDate);

            if (result < 0)               // relationship = "is earlier than";
            {
                resultTime = p_reservedWorkingFolder;
            }
            else if (result == 0)  // relationship = "is the same time as";
            {

            }
            else// relationship = "is later than";
            {

            }

            return resultTime;
        }

        ///<summary>
        ///* Function: function to Copy a file and rewrite the file after adding some txt to it.!--  
        ///* @author: Sena.kim
        ///* @parameter: 
        ///* @return: None
        ///</summary>
        void RewriteConfigFile(int Line)
        {
            int line_to_edit = 2; // Warning: 1-based indexing!
            string sourceFile = "source.txt";
            string destinationFile = "target.txt";

            // Read the appropriate line from the file.
            string lineToWrite = null;
            using (StreamReader reader = new StreamReader(sourceFile))
            {
                for (int i = 1; i <= line_to_edit; ++i)
                    lineToWrite = reader.ReadLine();
            }

            if (lineToWrite == null)
                throw new InvalidDataException("Line does not exist in " + sourceFile);

            // Read the old file.
            string[] lines = File.ReadAllLines(destinationFile);

            // Write the new file over the old file.
            using (StreamWriter writer = new StreamWriter(destinationFile))
            {
                for (int currentLine = 1; currentLine <= lines.Length; ++currentLine)
                {
                    if (currentLine == line_to_edit)
                    {
                        writer.WriteLine(lineToWrite);
                    }
                    else
                    {
                        writer.WriteLine(lines[currentLine - 1]);
                    }
                }
            }
        }


        private void Log(string logMessage, TextWriter txtWriter, string logType)
        {
            try
            {
                txtWriter.Write("\r\n" + logType);
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
                // txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception)
            {
            }
        }



        private string WriteOnConsole(string message, MessageStatus messageStatus, TextWriter w)
        {
            string messageToWrite = message;
            switch (messageStatus)
            {
                case MessageStatus.Usual:

                    WriteMessage(message);
                    Log(messageToWrite, w, "# Usual Log : ");
                    return messageToWrite;

                case MessageStatus.Error:
                    WriteErrorMessage("Warning :" + message);
                    Log(messageToWrite, w, "# Error Log : ");
                    return messageToWrite;

                case MessageStatus.Warning:
                    WriteWarningMessage("Error :" + message);
                    Log(messageToWrite, w, "# Warning Log : ");
                    return messageToWrite;

                default:
                    return messageToWrite;
            }
        }
        public MessageBody AssignTempMessageBody(string p_linename, string p_raw, string p_cep)
        {
            var messageBody = new MessageBody
            {
                LineName = p_linename,
                Raw = p_raw,
                Cep = p_cep,

            };
            return messageBody;

        }
        public MessageBody AssignTempMessageBody(string message)
        {
            var messageBody = new MessageBody
            {
                LineName = "MC3LINE",
                Raw = "/app/data/data/P0104087054960BW0301_00013440049_2018-07-20-08-28-10_Raw.csv",
                Cep = "/app/data/data/P0104087054960BW0301_00013440049_2018-07-20-08-28-10_Cep.csv",

            };
            return messageBody;
        }

        private void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

        }
        private void WriteWarningMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        private void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }



    }
}