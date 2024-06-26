﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Bimser.Framework.Dependency;
using System.Reflection;
using System.Threading.Tasks;
using Bimser.Framework.Web.Models;
using Bimser.Synergy.Entities.DataSource.Providers;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Requests;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Responses;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Spire.Doc;
using Bimser.Framework.Collection.Extensions;

namespace CSP.Util.Helper
{

    public static class GetHelper
    {
        internal static Configuration GetConfig(Context context)
        {
            var prm = new { };
            try
            {
                string configJson = ExecuteLocalQuery<string>(context, "Get_Config", new object[] { prm }).Result[0];

                return JObject.Parse(configJson)["Config"].ToObject<Configuration>();
            }
            catch (Exception)
            {

                throw new ArgumentNullException("Config Bilgileri Getirilemedi");
            }
        }

        /// <summary>
        /// sample Url = CSPApiUrl+"/api/sapintegration/invokeRfc"
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="functName"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static T GetSAPData<T>(Context context, string functName, dynamic input)
        {
            Configuration config = GetConfig(context);

            try
            {
                RestClient restClient = new RestClient("");

                var request = new RestRequest("/api/sapintegration/invokeRfc");

                var payload = new
                {
                    rfcName = functName,
                    payload = input,
                    abapSystem = new
                    {
                        user = config._SAPUserName,
                        passwd = config._SAPPassword,
                        ashost = config._SAPHost,
                        sysnr = config._SAPPort,
                        client = config._SAPClient,
                        lang = config._SAPLanguage
                    }
                };

                request.AddJsonBody(JsonConvert.SerializeObject(payload), ContentType.Json);
                

                var response = restClient.Execute(request, Method.Post);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception("Hatalı Request Veya Veri Getirilemedi");
                }

                return JsonConvert.DeserializeObject<T>(response.Content.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
                //return default(T);
            }
        }

        #region Sql

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="conStr"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        public static DataTable GetSQLData(string queryString, string conStr, Dictionary<string, string> prm = null)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                DataSet ds = new DataSet();
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                SqlDataAdapter adp = new SqlDataAdapter();

                adp.SelectCommand = command;

                command.Connection.Open();

                adp.Fill(ds);

                command.Connection.Close();

                return ds.Tables[0];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        public static DataTable GetSQLData(Context context, string queryString, Dictionary<string, string> prm = null)
        {
            Configuration config = GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                DataSet ds = new DataSet();
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                SqlDataAdapter adp = new SqlDataAdapter();

                adp.SelectCommand = command;

                command.Connection.Open();

                adp.Fill(ds);

                command.Connection.Close();

                return ds.Tables[0];
            }
        }


        /// <summary>
        /// For Scalar Value
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="conStr"></param>
        /// <param name="prm"></param>
        /// <param name="sqltyp"></param>
        /// <returns></returns>
        public static object SQLExecuteScalar(string queryString, string conStr, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
        {
            using (SqlConnection connection = new SqlConnection(conStr))
            {
                connection.Open();

                using SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.CommandType = sqltyp;

                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// For Scalar Value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        /// <param name="sqltyp"></param>
        /// <returns></returns>
        public static object SQLExecuteScalar(Context context, string queryString, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
        {
            Configuration config = GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                connection.Open();

                using SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.CommandType = sqltyp;

                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// For ExecuteNonQuery
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="ConStr"></param>
        /// <param name="prm"></param>
        public static int SQLExecute(string queryString, string ConStr, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
        {
            using (SqlConnection connection = new SqlConnection(ConStr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.Connection.Open();
                int effective = command.ExecuteNonQuery();
                command.Connection.Close();

                return effective;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="queryString"></param>
        /// <param name="prm"></param>
        public static int SQLExecute(Context context, string queryString, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
        {
            Configuration config = GetConfig(context);

            using (SqlConnection connection = new SqlConnection(config._ConnStr))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                if (prm != null)
                {
                    foreach (var item in prm)
                    {
                        command.Parameters.AddWithValue("@" + item.Key, item.Value);
                    }
                }

                command.Connection.Open();
                int effective = command.ExecuteNonQuery();
                command.Connection.Close();

                return effective;
            }
        }

        #endregion

        #region Log

        /// <summary>
        /// Log To AI_LOG Table
        /// CREATE TABLE dbo.AI_LOG ( ID INT IDENTITY(1,1), LOGTEXT NVARCHAR(MAX), CONSTRAINT PK_TBL_LOG_ID PRIMARY KEY(ID) ) GO
        /// </summary>
        /// <param name="logJsonValue"></param>
        public static void Log(Context context, string logJsonValue)
        {
            Configuration config = GetConfig(context);

            SQLExecute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", config._ConnStr);
        }

        private static void Log(Configuration config, string logJsonValue)
        {
            SQLExecute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", config._ConnStr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logJsonValue"></param>
        /// <param name="connString"></param>
        public static void Log(string logJsonValue, string connString)
        {

            SQLExecute(@"INSERT INTO AI_LOG (LOGTEXT) VALUES(N'" + logJsonValue + "')", connString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bulkData"></param>
        /// <param name="tableName"></param>       
        public static void LogToTable(Context context, DataTable bulkData, string tableName)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentNullException("Detay Tablo İçin Tablo İsmi Giriniz");
            
            Configuration config = GetConfig(context);

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
            if(string.IsNullOrEmpty(tableName)) 
                    throw new ArgumentNullException("Detay Tablo İçin Tablo İsmi Giriniz");
            
            BulkInsert(connString, bulkData, tableName);
        }



        private static void BulkInsert(Context context, DataTable bulkData, string tableName)
        {
            Configuration config = GetConfig(context);
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



        //public static List<MemberComparisonResult> CompareObjectsAndGetDifferences<T>(T firstObj, T secondObj)
        //{
        //    var differenceInfoList = new List<MemberComparisonResult>();

        //    foreach (var member in typeof(T).GetMembers())
        //    {
        //        if (member.MemberType == MemberTypes.Property)
        //        {
        //            var property = (PropertyInfo)member;
        //            if (property.CanRead && property.GetGetMethod().GetParameters().Length == 0)
        //            {
        //                var xValue = property.GetValue(firstObj, null);
        //                var yValue = property.GetValue(secondObj, null);
        //                if (!object.Equals(xValue, yValue))
        //                    differenceInfoList.Add(new MemberComparisonResult(property.Name, xValue, yValue));
        //            }
        //            else
        //                continue;
        //        }
        //    }
        //    return differenceInfoList;
        //}

        public static List<MemberComparisonResult> CompareJsonDifferencess(string firstJson, string secondJson)
        {
            var differenceInfoList = new List<MemberComparisonResult>();

            JObject sourceJObject = JsonConvert.DeserializeObject<JObject>(firstJson);
            JObject targetJObject = JsonConvert.DeserializeObject<JObject>(secondJson);

            if (!JToken.DeepEquals(sourceJObject, targetJObject))
            {
                foreach (KeyValuePair<string, JToken> sourceProperty in sourceJObject)
                {
                    JProperty targetProp = targetJObject.Property(sourceProperty.Key);

                    if (!JToken.DeepEquals(sourceProperty.Value, targetProp.Value))
                    {
                        differenceInfoList.Add(new MemberComparisonResult(sourceProperty.Key, sourceProperty.Value, targetProp.Value));
                    }
                }
            }

            return differenceInfoList;
        }

        #region DM

        /// <summary>
        /// Get Word Template From DM Path, and create PDF
        /// </summary>
        /// <param name="context"></param>
        /// <param name="templateDMPath">Such as Documents/Templates/MyTemplate </param>
        /// <param name="uploadDMPath">Such as Documents/Uploads/MyPdfUpload </param>
        /// <param name="values">Replace Key to Values</param>
        /// <returns></returns>
        public static (bool, string) CreatePdfWithSpireDoc(Context context, string templateDMPath, string uploadDMPath, Dictionary<object, object> values)
        {
            string link = string.Empty;
            bool isOk = false;
            string templateUrl = ServiceApiHelper.GetDMDownloadUrl(context, templateDMPath);
            byte[] templateBytes = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.DownloadAsync(templateUrl).Result;

            using MemoryStream ms = new MemoryStream();

            Document doc = new Document();

            doc.LoadFromStream(new MemoryStream(templateBytes), FileFormat.Docx2019);

            foreach (var item in values)
            {
                doc.Replace(matchString: item.Key.ToString(), newValue: item.Value.ToString(), caseSensitive: true, wholeWord: true);
            }


            doc.SaveToFile(ms, FileFormat.PDF);
            ms.Seek(0, SeekOrigin.Begin);

            long isPathId = ServiceApiHelper.Upload(context, uploadDMPath + ".pdf", ms.ToArray());
            if (isPathId > 0)
            {
                link = GetDMLink(context, uploadDMPath + ".pdf");
                isOk = true;
            }

            return (isOk, link);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Such as Documents/Uploads/MyUpload.extension</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetDMLink(Context context, string path)
        {
            Configuration config = GetConfig(context);

            try
            {

                //string pdfPath = $"Dokümanlar/Uploads/TedarikciDegistirme/Uploads/{txtSurecNo.Text}.pdf";
                WrapResponse<GetDMObjectsResponse> file = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
                GetDownloadUrlResponse getDownloadUrlResponse = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

                string downloadUrl = config._CSPApiUrl + getDownloadUrlResponse.DownloadUrl;

                return downloadUrl;

            }
            catch (Exception ex)
            {
                Log(config, JsonConvert.SerializeObject(ex));

                throw new Exception("Link Getirilemedi");
            }
        }

        #endregion

        public static bool ChekIban(string iban)
        {
            return IbanHelper.CheckIban(iban);
        }

        #region ExecLocalQuery
       
        internal const string DATA_SOURCE_CONTROLLER_BASE_TYPE_NAME = "Bimser.CSP.DataSource.Api.Base.BaseDataSourceController, Bimser.CSP.DataSource";

        internal static async Task<List<T>> ExecuteLocalQuery<T>(Context context, string queryName, object[] queryArgs)
        {
            var dataSourceControllers = FindControllers();
            if (dataSourceControllers.IsNullOrEmpty()) throw new Exception("Data Source not found.");

            var query = dataSourceControllers.FindQuery(context.EncryptedData, context.Language, context.Token, queryName);
            if (query != null)
            {
                object[] queryArguments = null;

                var parameters = query.Item3;
                if (!parameters.IsNullOrEmpty())
                {
                    if (queryArgs.IsNullOrEmpty()) throw new ArgumentNullException("queryArgs", "You must send query arguments.");

                    queryArguments = new object[parameters.Length];

                    int indexer = 0;
                    foreach (var arg in queryArgs)
                    {
                        var parameterType = parameters[indexer].ParameterType;
                        var argJson = JsonConvert.SerializeObject(arg);
                        var argObj = JsonConvert.DeserializeObject(argJson, parameterType);
                        queryArguments[indexer] = argObj;

                        indexer++;
                    }
                }

                var queryResult = await query.Item1.InvokeMethodGetResultAsync(query.Item2, queryArguments);
                if (queryResult is DataSourceResponse<object> dataSourceResponse)
                {
                    return dataSourceResponse.Result.ToTypedResult<T>();
                }
            }

            return null;
        }

        private static List<Type> FindControllers()
        {
            Type dataSourceType = Type.GetType(DATA_SOURCE_CONTROLLER_BASE_TYPE_NAME);

            var find = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => dataSourceType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();

            return find;
        }

        private static Tuple<object, MethodInfo, ParameterInfo[]> FindQuery(this List<Type> controllers, string encryptedData, string language, string token, string queryName)
        {
            Tuple<object, MethodInfo, ParameterInfo[]> result = null;
            foreach (Type controller in controllers)
            {
                var instance = Activator.CreateInstance(controller,
                    IocManager.Instance,
                    $"Bearer {token}",
                    encryptedData,
                    language);

                var query = controller
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(x => x.Name.Equals(queryName));

                if (query == null) continue;

                result = new Tuple<object, MethodInfo, ParameterInfo[]>(instance, query, query.GetParameters());
            }

            if (result == null) throw new Exception("Data Source Query not found.");

            return result;
        }

        private static async Task<object> InvokeMethodGetResultAsync(this object source, MethodInfo targetMethod, params object[] parameters)
        {
            try
            {
                dynamic awaitable = targetMethod.Invoke(source, parameters);
                await awaitable;
                return awaitable.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    throw ex.InnerException;

                throw;
            }
        }

        private static List<T> ToTypedResult<T>(this object source)
        {
            var result = new List<T>();

            if (source != null)
            {
                // cast source object to JArray
                var jsonArray = (JArray)source;

                // get 
                result = jsonArray.ToObject<List<T>>();
            }

            return result;
        }

        #endregion

    }

    public class MemberComparisonResult
    {
        public string Name { get; }
        public object FirstValue { get; }
        public object SecondValue { get; }

        public MemberComparisonResult(string name, object firstValue, object secondValue)
        {
            Name = name;
            FirstValue = firstValue;
            SecondValue = secondValue;
        }

        public override string ToString()
        {
            return Name; //+ " : " + FirstValue.ToString() + (FirstValue.Equals(SecondValue) ? " == " : " != ") + SecondValue.ToString()
        }
    }
}
