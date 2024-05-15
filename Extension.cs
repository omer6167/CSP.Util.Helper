﻿using Bimser.Synergy.ServiceAPI.Models.Form;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace CSP.Util.Helper
{
    public static class Extension
    {
        //public static DataTable ToDataTable<T>(this IList<T> data)
        //{
        //    PropertyDescriptorCollection properties =
        //        TypeDescriptor.GetProperties(typeof(T));
        //    DataTable table = new DataTable();
        //    foreach (PropertyDescriptor prop in properties)
        //        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        //    foreach (T item in data)
        //    {
        //        DataRow row = table.NewRow();
        //        foreach (PropertyDescriptor prop in properties)
        //            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
        //        table.Rows.Add(row);
        //    }
        //    return table;
        //}

        public static DataTable ToDataTable<T>(this IList<T> list)
        {
            string json = JsonConvert.SerializeObject(list);
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);

            return dt;
        }

        public static T ToClass<T>(this DataTable dataTable)
        {
            string json = JsonConvert.SerializeObject(dataTable);
            

            T myClass = JsonConvert.DeserializeObject<T>(json);

            return myClass;
        }

        public static bool IsJsonString(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) { return false; }
            str = str.Trim();
            if ((str.StartsWith("{") && str.EndsWith("}")) ||
                (str.StartsWith("[") && str.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(str);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="bulkData">Bulk insert with DataTableName</param>
        /// <param name="connString"></param>
        public static void LogToTable(this DataTable bulkData, string connString)
        {
            GetHelper.LogToTable(connString:connString,bulkData: bulkData, tableName: bulkData.TableName);
        }

        public static string ToJson(this LogClass logClass)
        {
            return JsonConvert.SerializeObject(logClass);
        }

        public static LogClass ToLogClass(string logClassJson)
        {
            return JsonConvert.DeserializeObject<LogClass>(logClassJson);
        }

        public static void InsertLog(this LogClass logClass,string connString)
        {
            GetHelper.Log(logJsonValue: logClass.ToJson(), connString: connString);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="frm"></param>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public static object GetFormControlValue(this FormInstance frm, string controlName)
        {
            var controlValue = frm.Controls[controlName].Value;
            if (controlValue is JValue jValue && jValue.Type == JTokenType.Boolean)
            {
                return jValue.Value<bool>().ToString();
            }
            else
            {
                return controlValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceForm"></param>
        /// <param name="targetForm"></param>
        /// <param name="controlName"></param>
        public static void SetFormControlValue(this FormInstance sourceForm, FormInstance targetForm,  string controlName)
        {
            try
            {
                targetForm.SetControlValue(controlName,GetFormControlValue(sourceForm,controlName));
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static void SetFormControlValues(this FormInstance sourceForm, FormInstance targetForm, Dictionary<string, string> controlNames)
        {
            try
            {
                
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
