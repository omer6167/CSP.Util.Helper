using System;
using System.Data;

namespace CSP.Util.Helper
{
    public class LogClass
    {
        public LogClass(int id, string message, string error = "undefined", string method = "", string project = "")
        {
            Id = id;
            Message = message;
            Error = error;
            Method = method;
            Project = project;
        }

        public int Id { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string Method { get; set; } = "";
        public string Project { get; set; } = "";
        public string Date { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

        public DataTable GetDataTable()
        {
            DataTable dt = new DataTable("Log");

            dt.Columns.Add("Id");
            dt.Columns.Add("Message");
            dt.Columns.Add("Error");
            dt.Columns.Add("Project");
            dt.Columns.Add("Date");

            return dt; 
        }

    }
}
