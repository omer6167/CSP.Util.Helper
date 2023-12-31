using DevExpress.Data.Linq.Helpers;
using System;
using System.Collections.Generic;
using System.Data;

namespace CSP.Util.Helper
{
    public class LogClass
    {
        public LogClass(long id, string message, string error = "undefined", string method = "", string project = "")
        {
            Id = id;
            Message = message;
            Error = error;
            Method = method;
            Project = project;
        }

        public long Id { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string Method { get; set; } = "";
        public string Project { get; set; } = "";
        public string Date { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        public DataTable GetDataTable()
        {
            DataTable dt = new("TBL_LogClass");

            dt.Columns.Add("Id");
            dt.Columns.Add("Message");
            dt.Columns.Add("Error");
            dt.Columns.Add("Method");
            dt.Columns.Add("Project");
            dt.Columns.Add("Date");

            return dt; 
        }

        /// <summary>
        /// if OBJECT_ID('TBL_LogClass') is null CREATE TABLE [dbo].[TBL_LogClass]( [Id][bigint] NULL,[Message] [varchar] (max) NULL,[Error] [varchar] max) NULL, [Method] [varchar] (max) NULL, [Project] [varchar] (250) NULL, [Date][varchar] (250) NULL )
        /// </summary>
        /// <param name="connString"></param>
        public void CrateTableOnDB(string connString)
        {
            string query = @"if OBJECT_ID('TBL_LogClass') is null 
                            CREATE TABLE [dbo].[TBL_LogClass](
	                        [Id] [bigint] NULL,
	                        [Message] [varchar](max) NULL,
	                        [Error] [varchar](max) NULL,
	                        [Method] [varchar](max) NULL,
	                        [Project] [varchar](250) NULL,
	                        [Date] [varchar](250) NULL
                        )";

            GetHelper.SQLExecute(queryString: query, ConStr: connString);

        }


        public LogClass GetLastLogById(string connString)
        {
            string query = "select top 1 * from TBL_LogClass order by Id desc;";

            DataTable dt = GetHelper.GetSQLData(queryString: query, conStr: connString);

            return dt.DataTableToClass<LogClass>();
        }

        public LogClass GetLogForId(string connString,long Id)
        {
            string query = "select top 1 * from TBL_LogClass where Id = @Id ;";

            DataTable dt = GetHelper.GetSQLData(queryString: query, conStr: connString,new Dictionary<string, string>()
            {
                { "Id",Id.ToString() }
            });

            return dt.DataTableToClass<LogClass>();
        }


    }
}
