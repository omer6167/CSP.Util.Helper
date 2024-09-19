using Bimser.Synergy.Entities.Shared.Business.Objects;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;

namespace CSP.Util.Helper
{
    public static class LogHelper
    {
        #region Log

        /// <summary>
        /// Log To AI_LOG Table
        /// CREATE TABLE dbo.AI_LOG ( ID INT IDENTITY(1,1), LOGTEXT NVARCHAR(MAX), CONSTRAINT PK_TBL_LOG_ID PRIMARY KEY(ID) ) GO
        /// </summary>
        /// <param name="logJsonValue"></param>
        public static void Log(Context context, string logJsonValue)
        {
            Configuration config = ConfigHelper.GetConfig(context);

            SqlHelper.Execute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", config._ConnStr);
        }

        public static void Log(Configuration config, string logJsonValue)
        {
            SqlHelper.Execute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", config._ConnStr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logJsonValue"></param>
        /// <param name="connString"></param>
        public static void Log(string logJsonValue, string connString)
        {

            SqlHelper.Execute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", connString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bulkData"></param>
        /// <param name="tableName"></param>       
        public static void LogToTable(Context context, DataTable bulkData, string tableName)
        {
            Configuration config = ConfigHelper.GetConfig(context);

            BulkInsert(context, bulkData, tableName);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="bulkData"></param>
        /// <param name="tableName"></param>
        public static void LogToTable(string connString, DataTable bulkData, string tableName)
        {
            BulkInsert(connString, bulkData, tableName);
        }



        private static void BulkInsert(Context context, DataTable bulkData, string tableName)
        {
            Configuration config = ConfigHelper.GetConfig(context);
            SqlConnection con = new SqlConnection(config._ConnStr);

            con.Open();
            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                if (bulkData.Rows.Count > 0)
                {
                    SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, transaction);

                    bulkCopy.DestinationTableName = tableName;

                    foreach (DataColumn column in bulkData.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    bulkCopy.BulkCopyTimeout = 9999;
                    bulkCopy.WriteToServer(bulkData);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        private static void BulkInsert(string connString, DataTable bulkData, string tableName)
        {

            SqlConnection con = new SqlConnection(connString);

            con.Open();
            SqlTransaction transaction = con.BeginTransaction();

            try
            {
                if (bulkData.Rows.Count > 0)
                {
                    SqlBulkCopy bulkCopy = new SqlBulkCopy(con, SqlBulkCopyOptions.Default, transaction);

                    bulkCopy.DestinationTableName = tableName;

                    foreach (DataColumn column in bulkData.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(column.ColumnName, column.ColumnName));
                    }

                    bulkCopy.BulkCopyTimeout = 9999;
                    bulkCopy.WriteToServer(bulkData);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                con.Close();
            }
        }

        #endregion

        #region Extensions
        public static LogClass ToLogClass(string logClassJson)
        {
            return JsonConvert.DeserializeObject<LogClass>(logClassJson);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="bulkData">Bulk insert with DataTableName</param>
        /// <param name="connString"></param>
        public static void LogToTable(this DataTable bulkData, string connString)
        {
            LogHelper.LogToTable(connString: connString, bulkData: bulkData, tableName: bulkData.TableName);
        }

        


        public static void InsertLog(this LogClass logClass, string connString)
        {
            LogHelper.Log(logJsonValue: logClass.ToJson(), connString: connString);
        }
        #endregion


    }
}
