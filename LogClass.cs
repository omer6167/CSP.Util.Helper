using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CSP.Util.Helper
{
    public class LogClass
    {
        public LogClass(int id, string message, string error, string methodName, string project, string date)
        {
            Id = id;
            Message = message;
            Error = error;
            MethodName = methodName;
            Project = project;
            Date = date;
        } 
       
        public int Id { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public string MethodName { get; set; } = "";
        public string Project { get; set; } = "";
        public string Date { get; set; } = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

    }
}
