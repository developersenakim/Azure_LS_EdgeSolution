using System;
//using System.Collections.Generic;
//using System.Text;
//using Newtonsoft.Json;
using System.Data.SqlClient;
//using Microsoft.Azure.Devices.Client;
//using System.Threading.Tasks;
using System.Data;

//dotnet add package System.Data.SqlClient
//#r "Microsoft.Azure.Devices.Client"
//#r "Newtonsoft.Json"
//#r "System.Data.SqlClient"
namespace PreProcessorModule
{

    public class SQLClass
    {
        public string m_connectionstring;
        private string m_dbname;
        private string m_tablename;
        public SQLClass(string connectionstring)
        {
            m_connectionstring = connectionstring;
        }
        public string GetSqlDbName()
        {
            return m_dbname;
        }
        public string GetSqlTableName()
        {
            return m_tablename;
        }
        public void SetSqlNameAndTable(string p_dbname, string p_tablename)
        {
            m_dbname = p_dbname;
            m_tablename = p_tablename;
        }

        public string CheckSqlConnection()
        {
            string tempState = "failed";
            try
            {
                Console.WriteLine("DB Connection State: {0}", OpenSqlConnection());
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Error See log for detail.");
                Console.WriteLine("ConnectionString: {0}", exception);
            }
            return tempState;
        }
        ///<summary>
        ///* Function: private function to Opening SQL CONNECTION.!-- This is used by many other public functions SetSqlNameAndTable. First.!--  
        ///* @author: Sena.kim
        ///* @parameter: NONE
        ///* @return: ConnectionState
        ///</summary>
        private ConnectionState OpenSqlConnection()
        {
            ConnectionState temp_connectionState = ConnectionState.Closed;
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = m_connectionstring;
                connection.Open();
                temp_connectionState = connection.State;
                // Console.WriteLine("ConnectionString: {0}", connection.ConnectionString);
            }
            return temp_connectionState;
        }

       /////
        public bool CheckTableNameInSQL(string tablename)
        {
            string temp_CheckTableNameInSQLstring = string.Empty;
            string temp_errormessageString = string.Empty;
            bool temp_isProcessSucceeded = false;
            try
            {
                temp_CheckTableNameInSQLstring = "select NAME FROM sysobjects where name = '" + tablename + "'";
                temp_errormessageString = "";
                temp_isProcessSucceeded = ProcessSQL(temp_CheckTableNameInSQLstring, temp_errormessageString);
            }
            catch
            {

            }
            return temp_isProcessSucceeded;

        }

        public bool CheckDBInSQL(string p_dbname)
        {
            string temp_CheckTableNameInSQLstring = $"USE {p_dbname}";
            string temp_errormessageString = "";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CheckTableNameInSQLstring, temp_errormessageString);

            return temp_isProcessSucceeded;
        }
        public void CreateDBInSQL(string p_dbname, string p_filepath)
        {
            string temp_CreateDBNameInSQLstring = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'{p_dbname}') BEGIN CREATE DATABASE {p_dbname} ON (NAME = {p_dbname}, FILENAME = {p_filepath}) END;";
            string temp_errormessageString = "Failed creating table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CreateDBNameInSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "The db'" + p_dbname + "' created;");
            }
        }
        public void CreateTableInSQL(string p_dbname, string p_tablename, string p_keyoptions)
        {
            string temp_CreateTableNameInSQLstring = $"IF  NOT EXISTS (SELECT * FROM {p_dbname}.dbo.sysobjects WHERE name = N'{p_tablename}') BEGIN CREATE TABLE {p_dbname}.dbo.{p_tablename} {p_keyoptions}; END;";
            string temp_errormessageString = "Failed creating table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CreateTableNameInSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "The table'" + p_tablename + "' created;");
            }
        }
        public void CreateTableInSQL(string tablename, string keyoptions)
        {
            string temp_CreateTableNameInSQLstring = $"create table {tablename} {keyoptions};";
            string temp_errormessageString = "Failed creating table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CreateTableNameInSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "The table'" + tablename + "' created");
            }
        }
        public void TruncateTable(string tablename)
        {
            //Empty table 
            string temp_CreateTableNameInSQLstring = $"TRUNCATE TABLE {tablename}";
            string temp_errormessageString = "Failed TRUNCATING table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CreateTableNameInSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "The table'" + tablename + "' TRUNCATED");
            }
        }
        public void InsertRawDataInSQL(string insertingvalues, string tablename)
        {
            //string temp_InsertRawDataToSQLstring = $"INSERT table {tablename}(X1 DOUBLE PRECISION, X2 DOUBLE PRECISION, X3 DOUBLE PRECISION, X4 DOUBLE PRECISION);";
            string temp_InsertRawDataToSQLstring = $"INSERT INTO {tablename} VALUES({insertingvalues});";

            string temp_errormessageString = "Failed inserting into the table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_InsertRawDataToSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                // LogBuilder.WriteMessage($"({insertingvalues})");
            }
        }

        public void CloseSQL()
        {
            using (SqlConnection connection = new SqlConnection(m_connectionstring))
            {
                connection.Open();
                connection.Close();
            }
        }
        private static void ReadSingleRow(IDataRecord record)
        {
            Console.WriteLine(String.Format(record[0].ToString()));
        }

        public string ReadSQL(string commandstring)
        {
            string tempresult = "0";
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = m_connectionstring;
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandstring, connection))
                    {
                        command.ExecuteNonQuery();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tempresult = String.Format(reader[0].ToString());
                            }
                            reader.Close();
                        }
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, "RedingSQL : " + exception);
            }
            return tempresult;
        }
        public bool ProcessSQL(string commandstring, string errormessage)
        {
            bool temp_processComplete = false;
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = m_connectionstring;
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandstring, connection))
                    {
                        command.ExecuteNonQuery();
                        if (commandstring.StartsWith("select") == true)
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    //  Console.WriteLine(reader.GetString(0));
                                    temp_processComplete = true;
                                }
                                else
                                {
                                    LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, errormessage);
                                    temp_processComplete = false;
                                }
                            }
                        }
                        else
                        {
                            temp_processComplete = true;
                        }

                    }
                }
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "Error See log for detail.");
                Console.WriteLine("ConnectionString: {0}", exception);
                temp_processComplete = false;
            }
            return temp_processComplete;
        }
        public void InsertTableInSQL(string p_Line, string p_TESTDATE, string p_Model, string p_Barcode, string p_Result, string p_RawLocation, string p_CepLocation, string p_ApsLocation)
        {
            string dbname = GetSqlDbName();
            string tbname = GetSqlTableName();
            string temp_InsertRawDataToSQLstring = $"INSERT INTO {dbname}.dbo.{tbname} ([LINE] ,[시험일자] ,[Model] ,[BarCode] ,[CREATEDT] ,[RAWLocation] ,[CEPLocation] ,[APSLocation]) VALUES( '{p_Line}', '{p_TESTDATE}' , '{p_Model}' , '{p_Barcode}' , GETDATE() , '{p_RawLocation}'  , '{p_CepLocation}'  , '{p_ApsLocation}' );";
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = m_connectionstring;
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(temp_InsertRawDataToSQLstring, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, $"{exception}");
            }
        }
        public void InsertSQL(string commandstring)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = m_connectionstring;
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandstring, connection))
                    {

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (System.Data.SqlClient.SqlException exception)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Error, "Error See log for detail.");
                Console.WriteLine("ConnectionString: {0}", exception);
            }
        }
    }// end of class
}// end of namespace


