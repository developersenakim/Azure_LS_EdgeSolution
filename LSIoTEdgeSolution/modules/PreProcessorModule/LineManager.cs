// compile with: -doc:DocFileName.xml 

using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

// using System.Data;
// using Microsoft.VisualBasic.FileIO;

namespace PreProcessorModule
{


    /// text for class LineStatus
    public class LineStatus
    {
        public LineStatus(string p_apsfolderName, string p_cepfolderName, string p_rawfolderName, string p_shareFolderLocation, string p_directoryName, int i, string p_reportfolderName, string p_aifolderName)
        {
            m_LineName = "";
            m_IsThereLineFolder = false;
            m_IsThereReportFolder = false;
            m_IsThereCurrentDirectoryDateFolder = false;
            m_IsThereAIFolder = false;
            m_apsfolderName = p_apsfolderName;
            m_cepfolderName = p_cepfolderName;
            m_rawfolderName = p_rawfolderName;
            m_previousworkingDate = new DateTime(1000, 8, 1, 12, 0, 0);
        }
        public string m_LineName { get; set; }
        public string m_apsfolderName { get; set; }
        public string m_cepfolderName { get; set; }
        public string m_rawfolderName { get; set; }
        public bool m_IsThereLineFolder { get; set; }
        public bool m_IsThereReportFolder { get; set; }
        public bool m_IsThereCurrentDirectoryDateFolder { get; set; }
        public bool m_IsThereAIFolder { get; set; }
        public string m_linefolderLocation { get; set; } // line folder location
        public string m_reportfolderlocation { get; set; } // report folder location
        public string m_aidatafolderlocation { get; set; } // AI folder location
        public DateTime m_previousworkingDate { get; set; } // AI folder location
        public DateFolderInfo m_currentdateFolderInfo { get; set; }
        //  public Queue<BadProductInfo> m_badProductsInfo { get; set; }
        public Queue<ModuleMessageBody> m_ModuleMessageBody { get; set; }

        public Environment m_currentEnvironment;
        public enum RestultFileType
        {
            APS,
            CEP,
            RAW,
        }

        public void Clear()
        {
            m_currentdateFolderInfo.CepFolderLocation = "";
            m_currentdateFolderInfo.BadProductsToPass.Clear();
            m_ModuleMessageBody.Clear();

        }




        ///<summary>
        ///* Function: Initialze and assign the Specified share folder location, line name, report and aidata folderlocation 
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter:string Sharefolderlocation, directory name as line name eg line2/3/4
        ///* @return: None 
        ///</summary>
        public int AssignLineStatus(string p_shareFolderLocation, string p_directoryName, int i, string p_reportfolderName, string p_aifolderName)
        {
            int Linenumber = i + 2;
            m_LineName = "MC" + Linenumber + "LINE";
            // m_LineName = p_directoryName;
            //reset variales, 
            m_linefolderLocation = p_shareFolderLocation + p_directoryName;

            m_reportfolderlocation = m_linefolderLocation + p_reportfolderName;//"/report";
            m_aidatafolderlocation = m_linefolderLocation + p_aifolderName;//'"/aidata";
            m_ModuleMessageBody = new Queue<ModuleMessageBody>();
            m_currentdateFolderInfo = new DateFolderInfo();
            m_IsThereLineFolder = DirectoryReader.IsDirectoryExistInThefolder(m_linefolderLocation);
            if (m_IsThereLineFolder == true)
            {
                m_IsThereReportFolder = DirectoryReader.IsDirectoryExistInThefolder(m_reportfolderlocation);
                m_IsThereAIFolder = DirectoryReader.IsDirectoryExistInThefolder(m_aidatafolderlocation);

                if (m_IsThereReportFolder == true && m_IsThereAIFolder == true)
                {
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, m_LineName + ": ReportFolder & AIFolder Detected and Successfully Assigned.");
                }
                else if (m_IsThereReportFolder == false && m_IsThereAIFolder == false) { }
                else if (m_IsThereReportFolder == false || m_IsThereAIFolder == false)
                {
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, m_LineName + ": ReportFolder or AIFolder Is Not Detected.");
                }
            }
            return Linenumber;
        }
        ///<summary>
        ///* Function: Initialze and assign the Specified share folder location, line name, report and aidata folderlocation 
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter:string Sharefolderlocation, directory name as line name eg line2/3/4
        ///* @return: None 
        ///</summary>
        public bool ProcessSingleDateFolderInfo(Environment p_currentEnvironment)
        {
            bool isThisNewDay = false;
          //  var comparingDates = 1;
            m_currentdateFolderInfo = SetSingleDateFolderInfo(p_currentEnvironment);
    
            if ( m_currentdateFolderInfo.WorkingDate.Date > m_previousworkingDate.Date)//  relationship = "is earlier than";
            {
                m_previousworkingDate = m_currentdateFolderInfo.WorkingDate;
                isThisNewDay = true;
            }

            else if (m_currentdateFolderInfo.WorkingDate.Date == m_previousworkingDate.Date) //relationship = "is the same time as";
            {
                isThisNewDay = false;
                m_previousworkingDate = m_currentdateFolderInfo.WorkingDate;
                SetbadProductsInfoUnderDateFolder(m_currentdateFolderInfo);

                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Accessing : " + m_currentdateFolderInfo.DateFolderLocationUnderReport);
                ProcessBadReportsUnderSingleDates(m_currentdateFolderInfo);
            }
            else
            {


            }
            return isThisNewDay;
            //relationship = "is later than";
        }


        ///<summary>
        ///* Function: Setting today's date folder.!--  
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter:string Sharefolderlocation, directory name as line name eg line2/3/4
        ///* @return: None 
        ///</summary>

        private DateFolderInfo SetSingleDateFolderInfo(Environment p_currentEnvironment)
        {
            m_currentEnvironment = p_currentEnvironment;
            string dateFolderLocation = "";
            DateTime currentworkingDate = LogBuilder.GetKoreanFormatTime();//GetToday's date

            dateFolderLocation = DirectoryReader.ParseDatetimeToDirectoryStyle(m_reportfolderlocation, currentworkingDate, "yyyyMMdd", m_currentEnvironment);
            DateFolderInfo todayDateFolderInfo = (new DateFolderInfo()
            {
                WorkingDate = currentworkingDate,
                DateFolderLocationUnderReport = dateFolderLocation,
                APSFolderLocation = this.GetApsCepRawLocation(m_apsfolderName, RestultFileType.APS, currentworkingDate),
                CepFolderLocation = this.GetApsCepRawLocation(m_cepfolderName, RestultFileType.CEP, currentworkingDate),
                RawDataFolderLocation = this.GetApsCepRawLocation(m_rawfolderName, RestultFileType.RAW, currentworkingDate),
                //BadProductsWithErrors = new Queue<BadProductInfo>(),
                BadProductsToPass = new Queue<BadProductInfo>(),
                isProcessingComplete = false,
            });
            return todayDateFolderInfo;
        }
        ///<summary>
        ///* Function: Initialze and assign the Specified share folder location, line name, report and aidata folderlocation 
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter: fileType ( "*.csv") folder path
        ///* @return: None 
        ///</summary>
        public void SetbadProductsInfoUnderDateFolderUsingSQL(DateFolderInfo p_datefolderInfo)
        {
            // if (m_IsThereReportFolder == true && m_IsThereAIFolder == true)
            // {
            //     FileInfo[] di = DirectoryReader.Readfromfolder("*.csv", p_datefolderInfo.DateFolderLocationUnderReport);//does directory have csv files? save csv files
            //     for (int i = 0; i < di.Length; i++)
            //     {
            //         if (di[i] != null)
            //         {
            //             DataTable csvData = new DataTable();
            //             try
            //             {
            //                 using (TextFieldParser csvReader = new TextFieldParser(csv_file_path))
            //                 {
            //                     csvReader.SetDelimiters(new string[] { "," });
            //                     csvReader.HasFieldsEnclosedInQuotes = true;
            //                     string[] colFields = csvReader.ReadFields();
            //                     foreach (string column in colFields)
            //                     {
            //                         DataColumn datecolumn = new DataColumn(column);
            //                         datecolumn.AllowDBNull = true;
            //                         csvData.Columns.Add(datecolumn);
            //                     }
            //                     while (!csvReader.EndOfData)
            //                     {
            //                         string[] fieldData = csvReader.ReadFields();
            //                         //Making empty value as null
            //                         for (int i = 0; i < fieldData.Length; i++)
            //                         {
            //                             if (fieldData[i] == "")
            //                             {
            //                                 fieldData[i] = null;
            //                             }
            //                         }
            //                         csvData.Rows.Add(fieldData);
            //                     }
            //                 }
            //             }
            //             catch (Exception ex)
            //             {
            //             }
            //             return csvData;
            //         }
            //     }
            // }

        }
        ///<summary>
        ///* Function: Initialze and assign the Specified share folder location, line name, report and aidata folderlocation 
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter: fileType ( "*.csv") folder path
        ///* @return: None 
        ///</summary>
        public void SetbadProductsInfoUnderDateFolder(DateFolderInfo p_datefolderInfo)
        {
            int dataError = 0;


            if (m_IsThereReportFolder == true && m_IsThereAIFolder == true)
            {

                //does directory have csv files? save csv files
                FileInfo[] di = DirectoryReader.Readfromfolder("*.csv", p_datefolderInfo.DateFolderLocationUnderReport);


                for (int i = 0; i < di.Length; i++)
                {
                    if (di[i] != null)
                    {
                        int badcount = 0;
                        int barcodeError = 0;
                        string[] allLines = DirectoryReader.ReadAllLinesOfAFile(di[i].Name, p_datefolderInfo.DateFolderLocationUnderReport);//File.ReadAllLines(fileLocation);
                        var csvLinesData = allLines.Skip(1);
                        var varbadProductsInfo = (from line in csvLinesData
                                                  let data = line.Split(",")
                                                  select new
                                                  {
                                                      Date = data[0],
                                                      Model = data[1],
                                                      BarCode = data[2],
                                                      Result = data[3]
                                                  })
                                         .Where(data => data.Result == "NG").ToList(); // end of linq
                        foreach (var s in varbadProductsInfo)
                        {
                            bool IsNotError = DirectoryReader.checkStringForContainingAlphabet(s.BarCode);
                            if (IsNotError == true)
                            {
                                badcount++;
                                p_datefolderInfo.BadProductsToPass.Enqueue(new BadProductInfo() { Date = s.Date, Model = s.Model, BarCode = s.BarCode, Result = s.Result });
                            }
                            else
                            {
                                barcodeError++;
                                dataError++;
                            }
                        }
                        // m_badProductsInfo.TrimExcess();
                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Reading " + p_datefolderInfo.WorkingDate + " " + di[i].Name + "....." + "No. of BadProducts #" + badcount);
                    }

                }// end of for loop
            } // end of  if (IsThereReportFolder == true && IsThereAIFolder == true)Z         

            string dateformat = LogBuilder.ParseDateTimeToString(p_datefolderInfo.WorkingDate, "yyyy-MM-dd");
            LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Folder :" + dateformat + " Total bad products :" + p_datefolderInfo.BadProductsToPass.Count + " DataError :" + dataError.ToString());

        }// end of  public SetDecisionResultUnderReportFolder(string p_dateFolderLocation)



        public Queue<ModuleMessageBody> ProcessBadReportsUnderSingleDates(DateFolderInfo p_datefolderInfo)
        {
            if (p_datefolderInfo.BadProductsToPass != null)
            {
                p_datefolderInfo.BadProductsToPass.TrimExcess();
                int tempmax = p_datefolderInfo.BadProductsToPass.Count;
                for (int i = 0; i < tempmax; i++)
                {
                    BadProductInfo singlebadInfo = p_datefolderInfo.BadProductsToPass.Dequeue();
                    p_datefolderInfo.BadProductsToPass.TrimExcess();
                    string fileApsstring = "", fileCepstring = "", fileRawstring = "";
                    FileInfo[] apsFi = Readfromfolder(singlebadInfo.BarCode, "*.csv", p_datefolderInfo.APSFolderLocation);
                    FileInfo[] cepFi = Readfromfolder(singlebadInfo.BarCode, "*.csv", p_datefolderInfo.CepFolderLocation);
                    FileInfo[] rawFi = Readfromfolder(singlebadInfo.BarCode, "*.csv", p_datefolderInfo.RawDataFolderLocation);

                    if (apsFi.Count() > 0)
                    {
                        fileApsstring = apsFi[0].ToString();
                    }
                    if (cepFi.Count() > 0)
                    {
                        fileCepstring = cepFi[0].ToString();
                    }
                    if (rawFi.Count() > 0)
                    {
                        fileRawstring = rawFi[0].ToString();
                    }

                    m_ModuleMessageBody.Enqueue(new ModuleMessageBody()
                    {
                        LineName = m_LineName,
                        BadProductInfo = (new BadProductInfo()
                        {
                            Date = singlebadInfo.Date,
                            Model = singlebadInfo.Model,
                            BarCode = singlebadInfo.BarCode,
                            Result = singlebadInfo.Result,
                        }),
                        Raw = fileRawstring,
                        Cep = fileCepstring,
                        Aps = fileApsstring
                    });
                }
            }
            return m_ModuleMessageBody;
        }
        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: containing folder can be name or date fileType ( "*.csv") folder path
        ///* @return: FileInfo[] Info. assigns the string 
        ///</summary>
        static public FileInfo[] Readfromfolder(string p_containingpartial, string p_fileType, string p_folderPath)
        {
            FileInfo[] fi;
            fi = new FileInfo[1];

            if (DirectoryReader.IsDirectoryExistInThefolder(p_folderPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(p_folderPath);
                fi = di.GetFiles("*" + p_containingpartial + p_fileType);
                if (fi.Length == 0)
                {
                    string message = "There is no existing " + p_containingpartial + " files in @ " + p_folderPath;
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, message);
                }
                else
                {
                    string message = "@" + p_folderPath + " folder." + "Detecting " + fi.Length + " " + p_containingpartial + " files.";
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, message);
                }
            }
            return fi;
        }



        ///<summary>
        ///* Function: Initialze and assign the Specified share folder location, line name, report and aidata folderlocation 
        /// This function proves that those specific folders exist.!-- 
        ///* @author: Sena.kim
        ///* @parameter: fileType ( "*.csv") folder path
        ///* @return: None 
        ///</summary>
        private string GetApsCepRawLocation(string p_fileType, RestultFileType p_resultType, DateTime p_workingdate)
        {
            string workingfolder = string.Empty;
            if (m_currentdateFolderInfo != null)
            {
                workingfolder = DirectoryReader.ParseDatetimeToDirectoryStyle(m_aidatafolderlocation, p_workingdate, "yyyy-MM-dd", m_currentEnvironment);
                workingfolder = workingfolder + p_fileType;
                bool isdirectoryExist = false;
                if (DirectoryReader.IsDirectoryEmpty(workingfolder) == false)
                {
                    isdirectoryExist = true;
                }

                switch (p_resultType)
                {
                    case RestultFileType.APS:
                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Is There APS Folder " + isdirectoryExist + " @ " + workingfolder);
                        break;

                    case RestultFileType.CEP:

                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Is There CEP Folder " + isdirectoryExist + " @ " + workingfolder);
                        break;
                    case RestultFileType.RAW:

                        LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Is There RAW Folder " + isdirectoryExist + " @ " + workingfolder);
                        break;
                    default:
                        break;

                }

            }

            return workingfolder;

        }
    }
}