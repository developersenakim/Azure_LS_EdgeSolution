using System.IO;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace PreProcessorModule
{
    public class DirectoryReader
    {
        /// <summary>
        /// Checks if the folder exists
        /// Return bool parameter fileType : *.csv folderPath : /app/data/ 
        /// </summary>
        static public bool IsDirectoryEmpty(string p_path)
        {
            if (System.IO.Directory.Exists(p_path))
            {
                int fileCount = Directory.GetFiles(p_path).Length;
                if (fileCount > 0)
                {
                    return false;
                }

                string[] dirs = Directory.GetDirectories(p_path);
                if (dirs.Length > 0)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Checks if the folder exists
        /// Return bool parameter fileType : *.csv folderPath : /app/data/ 
        /// </summary>
        static public bool IsDirectoryExistInThefolder(string p_folderpath)
        {

            bool isExist = false;

            if (System.IO.Directory.Exists(p_folderpath))
            {
                isExist = true;
            }
            else
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Warning, "Folder does not exist. [" + p_folderpath + "]");
            }
            return isExist;
        }
        /// <summary>
        /// Checks if the folder exists
        /// Return bool parameter fileType : *.csv folderPath : /app/data/ 
        /// </summary>
        static public bool DoesFileExist(string p_filePath)
        {
            bool isExist = false;
            if (System.IO.File.Exists(p_filePath))
            {
                isExist = true;
            }
            else
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Warning, "file does not exsit" + p_filePath);
            }
            return isExist;
        }

        ///<summary>
        ///* Function: Function to Parse Date Time to Directory style if this directory is under Report put reportfolderDirectory AI Data
        ///* @author: Sena.kim
        ///* @parameter: string DirectoryAboveTheFile, string folderFormat (report : "yyyyMMdd" or AIDATA :  "yyyy-MM-dd")
        ///* @return: string DateFolderLocation 
        ///</summary>
        static public string ParseDatetimeToDirectoryStyle(string p_directoryAboveTheDate, DateTime p_currentWorkingDate, string p_folderFormat, Environment p_currentEnvironment)
        {       //Parse Date Time to Directory style
            string sresultDateTime = LogBuilder.ParseDateTimeToString(p_currentWorkingDate, p_folderFormat);// Parse the DateTime To string to get folderName
            string dateFolderLocation = "";
            if (p_currentEnvironment == Environment.productionOnlinux)
            {
                dateFolderLocation = p_directoryAboveTheDate + "/" + sresultDateTime;
            }
            else if (p_currentEnvironment == Environment.testOnWindow)
            {
                dateFolderLocation = p_directoryAboveTheDate + "\\" + sresultDateTime;
            }
            dateFolderLocation = p_directoryAboveTheDate + "/" + sresultDateTime; //Assign DateFolderLocation                 

            return dateFolderLocation;
        }

        ///<summary>
        ///* Function: function to Reading specified fileTypes from folder 
        ///* @author: Sena.kim
        ///* @parameter: finename (log.txt) folderpath must not end with \
        ///* @return: FileInfo[] Info. assigns the string 
        ///</summary>
        static public string[] ReadAllLinesOfAFile(string p_filename, string p_folderlocation)
        {
            int currentRetry = 0;
            string[] allLines = new string[0];
            string s_fileName = p_filename;
            string fileLocation = p_folderlocation + "/" + s_fileName;

            // reading csvs, skips the first raw. 
            try
            {
                allLines = File.ReadAllLines(fileLocation);
            }
            catch
            {
                Thread.Sleep(20);
                currentRetry++;
                if (currentRetry > 10)
                {
                    // If this isn't a transient error or we shouldn't retry, 
                    // rethrow the exception.
                    throw;
                }

            }
            return allLines;
        }

        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: filepath the absoulte file path
        ///* @return: string[] Info. assigns the string 
        ///</summary>
        static public string[] ReadAllLinesOfAFile(string p_filepath)
        {
            string[] allLines;
            if (DoesFileExist(p_filepath))
            {
                string fileLocation = p_filepath;
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "start Reading" + p_filepath);        // reading csvs, skips the first raw. 
            }
            return allLines = File.ReadAllLines(p_filepath);


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

            if (IsDirectoryExistInThefolder(p_folderPath))
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
                    string message = "@" + p_folderPath + "folder." + "Detecting " + fi.Length + " " + p_containingpartial + " files.";
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, message);
                }
            }
            return fi;
        }


        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: fileType ( "*.csv") folder path
        ///* @return: FileInfo[] Info. assigns the string 
        ///</summary>
        static public FileInfo[] Readfromfolder(string p_fileType, string p_folderPath)
        {
            FileInfo[] fi;
            fi = new FileInfo[1];

            if (IsDirectoryExistInThefolder(p_folderPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(p_folderPath);

                //Check if the CSV files exist.                
                fi = di.GetFiles(p_fileType);

                if (fi.Length == 0)
                {
                    string message = "There is no existing " + p_fileType + " files in @ " + p_folderPath;
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, message);
                }
            }
            return fi;
        }

        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: folder path
        ///* @return: Directory Info[]. assigns the string 
        ///</summary>
        static public DirectoryInfo[] Readfromfolder(string p_folderPath)
        {
            DirectoryInfo[] fi = new DirectoryInfo[1];

            if (IsDirectoryExistInThefolder(p_folderPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(p_folderPath);
                fi = di.GetDirectories();

                if (fi.Length == 0)
                {
                    string message = "There is no existing directory";
                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, message);
                }
            }

            return fi;
        }


        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: filepath (string),  string splitstring, ref string var
        ///* @return: bool. true if the string contains alphabet
        ///</summary>
        public static bool checkStringForContainingAlphabet(String p_input)
        {
            bool iscontainingAlphabet = false;
            char[] input = p_input.ToCharArray();

            for (int id = 0; id < input.Length; id++)
            {
                if ('a' <= input[id] && input[id] <= 'z')
                {
                    iscontainingAlphabet = true;
                }
                else if ('A' <= input[id] && input[id] <= 'Z')
                {
                    iscontainingAlphabet = true;
                }
            }

            return iscontainingAlphabet;
        }



        ///<summary>
        ///* Function: function to read from content from a file using splitstring to detect the line to get a string
        ///* @author: Sena.kim
        ///* @parameter: filepath (string),  string splitstring, ref string var
        ///* @return: none. assigns the string 
        ///</summary>
        static public void ReadContentfromConfigAndReturnStringReference(string filepath, string splitstring, ref string var)
        {
            if (DoesFileExist(filepath))
            {
                string[] lines = System.IO.File.ReadAllLines(filepath);
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
    }
}