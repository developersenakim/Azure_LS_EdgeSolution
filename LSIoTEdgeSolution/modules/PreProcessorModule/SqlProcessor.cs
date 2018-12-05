using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Azure.Devices.Client;
using System.Threading.Tasks;
using System.Data;

//dotnet add package Newtonsoft.Json
//#r "Microsoft.Azure.Devices.Client"
//#r "Newtonsoft.Json"
//#r "System.Data.SqlClient"
namespace PreProcessorModule
{

    public class SQLClass
    {
        private string s_connectionstring;
        public bool SQLTableExist;

        public SQLClass(string connectionstring)
        {
            s_connectionstring = connectionstring;
            SQLTableExist = false;
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

        private ConnectionState OpenSqlConnection()
        {
            ConnectionState temp_connectionState = ConnectionState.Closed;
            using (SqlConnection connection = new SqlConnection())
            {

                connection.ConnectionString = s_connectionstring;
                connection.Open();
                temp_connectionState = connection.State;
                // Console.WriteLine("ConnectionString: {0}", connection.ConnectionString);
            }
            return temp_connectionState;
        }
        public bool CheckTableNameInSQL(string tablename)
        {
            string temp_CheckTableNameInSQLstring = "select name from sysobjects where name = '" + tablename + "'";
            string temp_errormessageString = "";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CheckTableNameInSQLstring, temp_errormessageString);

            return temp_isProcessSucceeded;
        }
        public void CreateTableInSQL(string tablename)
        {
            string temp_CreateTableNameInSQLstring = $"create table {tablename}(X1 DOUBLE PRECISION, X2 DOUBLE PRECISION, X3 DOUBLE PRECISION, X4 DOUBLE PRECISION);";
            string temp_errormessageString = "Failed creating table";
            bool temp_isProcessSucceeded = ProcessSQL(temp_CreateTableNameInSQLstring, temp_errormessageString);
            if (temp_isProcessSucceeded)
            {
                LogBuilder.LogWrite(LogBuilder.MessageStatus.Usual, "The table'" + tablename + "' created");
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
        public bool ProcessSQL(string commandstring, string errormessage)
        {
            bool temp_processComplete = false;
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = s_connectionstring;
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
        public void InsertSQL(string connectionstring, string commandstring)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    connection.ConnectionString = connectionstring;
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(commandstring, connection))
                        command.ExecuteNonQuery();
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


