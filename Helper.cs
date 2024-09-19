using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Bimser.Framework.Dependency;
using Bimser.Framework.Domain.Option.Pagination;
using Bimser.Framework.Web.Models;
using Bimser.Framework.Collection.Extensions;
using Bimser.Synergy.Entities.DataSource.Providers.WebService.Rest;
using Bimser.Synergy.Entities.DataSource.Providers;
using Bimser.Synergy.ServiceAPI;
using Bimser.Synergy.ServiceAPI.Models.Form;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Requests;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Responses;
using Bimser.Synergy.Entities.DocumentManagement.Business.Objects;
using Bimser.Synergy.Entities.WebInterface.Business.DTOs.Requests;
using Bimser.Synergy.Entities.Configuration.Business.DTOs.Requests;
using Bimser.Synergy.ServiceAPI.Models.Authentication;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.ServiceAPI.Models.Workflow;
using Bimser.Synergy.Entities.Workflow.Models.Properties;
using Bimser.CSP.Runtime.Common.Extensions;
using Bimser.CSP.FormControls.Extensions;
using Bimser.CSP.FormControls.Controls;
using Bimser.CSP.DataSource.Api.Models;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using Spire.Doc;



namespace Helper
{
	//For All of them

	#region Config
	public class Configuration
	{
		public Configuration()
		{

		}
		public string _ConnStr { get; set; } = "";
		public string _CSPApiUrl { get; set; } = "";
		public string _CSPUrl { get; set; } = "";
		public string _SAPUserName { get; set; } = "";
		public string _SAPPassword { get; set; } = "";
		public string _SAPHost { get; set; } = "";
		public string _SAPPort { get; set; } = "";
		public string _SAPClient { get; set; } = "";
		public string _SAPLanguage { get; set; } = "";

	}

	public static class Config
	{
		public static string _ConnStr { get; set; } = "";
		public static string _CSPApiUrl { get; set; } = "";
		public static string _CSPUrl { get; set; } = "";
		public static string _SAPUserName { get; set; } = "";
		public static string _SAPPassword { get; set; } = "";
		public static string _SAPHost { get; set; } = "";
		public static string _SAPPort { get; set; } = "";
		public static string _SAPClient { get; set; } = "";
		public static string _SAPLanguage { get; set; } = "";
	}
	#endregion

	#region ConfigHelper

	public static class ConfigHelper
	{
		public static Configuration GetConfig(Context context)
		{
			var prm = new { };
			try
			{
				string configJson = DataSourceHelper.ExecuteLocalQuery<string>(context, "Get_Config", new object[] { prm }).Result[0];

				return JObject.Parse(configJson)["Config"].ToObject<Configuration>();
			}
			catch (Exception)
			{

				throw new ArgumentNullException("Config Bilgileri Getirilemedi");
			}
		}

	}

	#endregion

	#region LogClass
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

			SqlHelper.Execute(queryString: query, ConStr: connString);

		}


		public LogClass GetLastLogById(string connString)
		{
			string query = "select top 1 * from TBL_LogClass order by Id desc;";

			DataTable dt = SqlHelper.GetData(queryString: query, conStr: connString);

			return dt.ToClass<LogClass>();
		}

		public LogClass GetLogForId(string connString, long Id)
		{
			string query = "select top 1 * from TBL_LogClass where Id = @Id ;";

			DataTable dt = SqlHelper.GetData(queryString: query, conStr: connString, prm: new Dictionary<string, string>()
			{
				{ "Id",Id.ToString() }
			});

			return dt.ToClass<LogClass>();
		}


	}
	#endregion

	#region LogHelper

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

		#region LogHelperExtensions

		/// <summary>
		///  
		/// </summary>
		/// <param name="bulkData">Bulk insert with DataTableName</param>
		/// <param name="connString"></param>
		public static void LogToTable(this DataTable bulkData, string connString)
		{
			LogHelper.LogToTable(connString: connString, bulkData: bulkData, tableName: bulkData.TableName);
		}

		public static LogClass ToLogClass(string logClassJson)
		{
			return JsonConvert.DeserializeObject<LogClass>(logClassJson);
		}


		public static void InsertLog(this LogClass logClass, string connString)
		{
			LogHelper.Log(logJsonValue: logClass.ToJson(), connString: connString);
		}
		#endregion


	}

	#endregion

	#region IbanHelper
	public static class IbanHelper
	{
		private static string[] DonusturmeTablosu_String = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", };
		private static string[] DonusturmeTablosu_Number = { "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "32", "33", "34", "35" };


		//private static bool IsAlpha(string strAlpha)
		//{
		//    System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^A-Z]");
		//    return !MyObj.IsMatch(strAlpha);
		//}

		private static bool IsTR(string strIlkIki)
		{
			return strIlkIki == "TR";
		}

		private static bool IsNumber(string strNumber)
		{
			System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^0-9]");
			return !MyObj.IsMatch(strNumber);
		}

		private static bool IsAlphaNumeric(string strAlphaNumeric)
		{
			System.Text.RegularExpressions.Regex MyObj = new System.Text.RegularExpressions.Regex("[^A-Z0-9]");
			return !MyObj.IsMatch(strAlphaNumeric);
		}

		/// <summary>
		/// Check TR Iban with  http://www.tcmb.gov.tr/iban/teblig.htm
		/// </summary>
		/// <param name="iban"></param>
		/// <returns></returns>
		public static bool CheckIbanIfIsTR(string iban)
		{
			string strIBAN = iban.Trim().ToUpper();

			//Boşsa Retunrn False
			if (string.IsNullOrEmpty(strIBAN.Trim())) return false;

			// uzunluk en fazla 34 karakter olabilir 
			//if (strIBAN.Length > 34) return false;

			//TR Iban
			if (strIBAN.Length != 26) return false;

			// ilk iki karakteri alalım
			string str1_2 = strIBAN.Substring(0, 2).Trim();

			// kontrol karakterlerini alalım 
			string str3_4 = strIBAN.Substring(2, 2).Trim();

			string strRezerv = strIBAN.Substring(10, 1).Trim();

			// ilk iki karakter yalnızca harf olabilir 
			// if (!IsAlpha(str1_2)) return false;

			//TR Kontrolü İçin
			if (!IsTR(str1_2)) return false;

			// kontrol karakterleri yalnızca sayı olabilir 
			if (!IsNumber(str3_4)) return false;

			// Rezerv Alanı Tüm hesaplarda 0 olmalıdır. 
			if (strRezerv != "0") return false;

			// IBAN numarası yalnızca harf ve rakam olabilir 
			if (!IsAlphaNumeric(strIBAN)) return false;

			// özel olan ilk 4 karakteri alalım 
			string temp1 = strIBAN.Substring(0, 4).Trim();

			// geri kalan karakterleri alalım 
			string temp2 = strIBAN.Substring(4).Trim();

			// ilk 4 karakteri sona atalım 
			string temp3 = temp2 + temp1;

			// harfleri sayı karşılıklarına çevirelim
			string temp4 = temp3;
			for (int i = 0; i < DonusturmeTablosu_String.Length; i++)
				temp4 = temp4.Replace(DonusturmeTablosu_String[i], DonusturmeTablosu_Number[i]);

			// sayıya çevirelim 
			decimal num = Convert.ToDecimal(temp4);

			// 97'ye göre mod alalım 
			decimal mod = num % 97;

			// eğer mod bölümünden kalan 1 ise uygun bir IBAN       
			// değilse uygun olmayan bir IBAN numarası demektir.    
			if (mod != 1) return false;

			return true;
		}


	}
	#endregion

	#region DataSourceHelper
	public static class DataSourceHelper
	{

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
			Configuration config = ConfigHelper.GetConfig(context);

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

		public static T GetSAPData<T>(Configuration config, string functName, dynamic input)
		{

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

		#region ExecuteDataSource



		public static List<T> ExecQuery<T>(Context context, string project, string query, object prm = null)
		{
			ServiceAPI service = ServiceApiHelper.GetServiceApiInstance(context);

			var resp = service.DataSourceManager.ExecuteQuery<T>(project, query, prm);

			if (resp.Result != null)
			{
				return resp.Result;
			}
			else if (resp.Exception != null)
			{
				throw new Exception("Sorgu Çalıştırılırken Hata Oluştu:" + query + ";" + resp.Exception.Message);
			}
			else
			{
				return new List<T> { };
			}
		}
		public async static Task<List<T>> ExecQueryAsync<T>(Context context, string project, string query, object prm = null)
		{
			var resp = await ServiceApiHelper.GetServiceApiInstance(context).DataSourceManager.ExecuteQuery<T>(project, query, prm);
			return resp;
		}

		public static object CreateRestParameter(Dictionary<string, object> keyValuePairs, RestWebServiceParameterType type = RestWebServiceParameterType.Body)
		{

			var parameters = new List<RestWebServiceParameterItem>();
			foreach (var item in keyValuePairs)
			{
				parameters.Add(new RestWebServiceParameterItem
				{
					Type = type,
					Key = item.Key,
					Value = item.Value,

				});
			}

			var input = new BaseDataSourceRestWebServiceRequest
			{
				Parameters = parameters,
				LoadOptions = new DataSourceLoadOptions(null, null, new Pagination(0, 100)),
			};

			return input;
		}



		#endregion


		#region ExecLocalQuery

		internal const string DATA_SOURCE_CONTROLLER_BASE_TYPE_NAME = "Bimser.CSP.DataSource.Api.Base.BaseDataSourceController, Bimser.CSP.DataSource";

		public static async Task<List<T>> ExecuteLocalQuery<T>(Context context, string queryName, object[] queryArgs)
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
			// cast source object to JArray

			if (source is JArray jsonArray)
			{

				// get 
				result = jsonArray.ToObject<List<T>>();
			}

			return result;
		}


		#endregion
	}
	#endregion

	#region DmHelper
	public static class DmHelper
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path">Such as Documents/Uploads/MyUpload.extension</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static string GetDMLink(Context context, string path)
		{
			Configuration config = ConfigHelper.GetConfig(context);

			try
			{
				ServiceAPI serviceApi = ServiceApiHelper.GetServiceApiInstance(context);

				WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
				GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

				string downloadUrl = config._CSPApiUrl + getDownloadUrlResponse.DownloadUrl;

				return downloadUrl;

			}
			catch (Exception ex)
			{
				//LogHelper.Log(config:config, JsonConvert.SerializeObject(ex));

				throw new Exception("Link Getirilemedi; " + ex.Message);
			}
		}

		public static string GetDMLink(Context context, string cspApiUrl, string path)
		{
			try
			{
				ServiceAPI serviceApi = ServiceApiHelper.GetServiceApiInstance(context);

				WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
				GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

				string downloadUrl = cspApiUrl + getDownloadUrlResponse.DownloadUrl;

				return downloadUrl;
			}
			catch (Exception ex)
			{

				throw new Exception("Link Getirilemedi; " + ex.Message);
			}
		}


		public static string GetDMLink(this ServiceAPI serviceApi, string cspApiUrl, string path)
		{
			try
			{

				WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
				GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

				string downloadUrl = cspApiUrl + getDownloadUrlResponse.DownloadUrl;

				return downloadUrl;
			}
			catch (Exception ex)
			{

				throw new Exception("Link Getirilemedi; " + ex.Message);
			}
		}

		public static string GetDMDownloadUrl(Context context, string path)
		{
			Configuration config = ConfigHelper.GetConfig(context);
			ServiceAPI serviceApi = ServiceApiHelper.GetServiceApiInstance(context);

			WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
			GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

			string downloadUrl = config._CSPApiUrl + getDownloadUrlResponse.DownloadUrl;

			return downloadUrl;
		}
		public static string GetDMDownloadUrl(Context context, string cspApiUrl, string path)
		{
			ServiceAPI serviceApi = ServiceApiHelper.GetServiceApiInstance(context);

			WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
			GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

			string downloadUrl = cspApiUrl + getDownloadUrlResponse.DownloadUrl;

			return downloadUrl;
		}

		public static string GetDMDownloadUrl(this ServiceAPI serviceApi, string cspApiUrl, string path)
		{

			WrapResponse<GetDMObjectsResponse> file = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
			GetDownloadUrlResponse getDownloadUrlResponse = serviceApi.DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

			string downloadUrl = cspApiUrl + getDownloadUrlResponse.DownloadUrl;

			return downloadUrl;
		}

		public static string GetMimeTypeForFileExtension(string filePath)
		{
			const string DefaultContentType = "application/octet-stream";

			var provider = new FileExtensionContentTypeProvider();

			if (!provider.TryGetContentType(filePath, out string contentType))
			{
				contentType = DefaultContentType;
			}
			return contentType;
		}

		public static byte[] Download(Context context, string path)
		{
			var serviceApi = ServiceApiHelper.GetServiceApiInstance(context);
			return serviceApi.DocumentManagement.DownloadAsync(path).Result;
		}

		public static byte[] Download(this ServiceAPI serviceApi, string path)
		{
			return serviceApi.DocumentManagement.DownloadAsync(path).Result;
		}

		public static WrapResponse<bool> DeleteFile(Context context, string secretKey)
		{
			var serviceApi = ServiceApiHelper.GetServiceApiInstance(context);
			return serviceApi.DocumentManagement.DeleteFile(new DeleteFileRequest(secretKey, " ")).Result;
		}

		public static WrapResponse<bool> DeleteFile(this ServiceAPI serviceApi, string secretKey)
		{

			return serviceApi.DocumentManagement.DeleteFile(new DeleteFileRequest(secretKey, " ")).Result;
		}

		public static string GetFolderSecretKey(this ServiceAPI serviceApi, string path)
		{
			string root = path.Replace(@"\", "/");
			WrapResponse<GetDMObjectsResponse> dmRootFolder = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;
			if (dmRootFolder.Success && dmRootFolder.Result.Items.Count > 0)
			{
				GetDMObjectResponse dmObject = dmRootFolder.Result.Items.First();
				return dmObject.SecretKey;
			}
			return "";
		}
		public static string GetFolderSecretKey(Context context, string path)
		{
			string root = path.Replace(@"\", "/");
			WrapResponse<GetDMObjectsResponse> dmRootFolder = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;
			if (dmRootFolder.Success && dmRootFolder.Result.Items.Count > 0)
			{
				GetDMObjectResponse dmObject = dmRootFolder.Result.Items.First();
				return dmObject.SecretKey;
			}
			return "";
		}

		public static async Task<GetDMObjectResponse> CreateFolder(this ServiceAPI serviceApi, string name, string parentFolderSecretKey, string description = null)
		{
			return await serviceApi.DocumentManagement.CreateFolderAsync(name.GetNameByText(), name.GetNameByText(), parentFolderSecretKey);
		}

		public static async Task<GetDMObjectResponse> CreateFolder(Context context, string name, string parentFolderSecretKey, string description = null)
		{
			return await ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.CreateFolderAsync(name.GetNameByText(), name.GetNameByText(), parentFolderSecretKey);
		}



		public static async Task<bool> MoveObjects(this ServiceAPI serviceApi, List<string> sourceSecretKeys, string destinationSecretKey)
		{
			if (sourceSecretKeys == null || string.IsNullOrWhiteSpace(destinationSecretKey))
				return false;

			var response = await serviceApi.DocumentManagement.MoveObjects(new MoveObjectsRequest(sourceSecretKeys, destinationSecretKey, string.Empty));
			if (response != null && response.Success)
			{
				return response.Result;
			}
			else
			{
				//Console.WriteLine(response.ErrorDetail);
				return false;
			}

		}
		public static async Task<bool> MoveObjects(Context context, List<string> sourceSecretKeys, string destinationSecretKey)
		{
			if (sourceSecretKeys == null || string.IsNullOrWhiteSpace(destinationSecretKey))
				return false;

			var response = await ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.MoveObjects(new MoveObjectsRequest(sourceSecretKeys, destinationSecretKey, string.Empty));
			if (response != null && response.Success)
			{
				return response.Result;
			}
			else
			{
				//Console.WriteLine(response.ErrorDetail);
				return false;
			}

		}
		private static GetDMObjectResponse GetDMObject(this ServiceAPI serviceApi, string path)
		{

			string root = path.Replace(@"\", "/");

			WrapResponse<GetDMObjectsResponse> dmRootFolder = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;

			if (dmRootFolder.Success && dmRootFolder.Result.Items.Count > 0)

			{
				GetDMObjectResponse dmObject = dmRootFolder.Result.Items.First();
				return dmObject;
			}

			return null;

		}
		private static GetDMObjectResponse GetDMObject(Context context, string path)
		{

			string root = path.Replace(@"\", "/");

			WrapResponse<GetDMObjectsResponse> dmRootFolder = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;

			if (dmRootFolder.Success && dmRootFolder.Result.Items.Count > 0)

			{
				GetDMObjectResponse dmObject = dmRootFolder.Result.Items.First();
				return dmObject;
			}

			return null;

		}

		public static async Task<string> GetDownloadUrlBySecretKey(this ServiceAPI serviceApi, GetDMObjectResponse file)
		{
			var result = await serviceApi.DocumentManagement.GetDownloadUrl(file.SecretKey, file.Name[file.Name.Keys.First()]);
			return $"{ServiceApiHelper.WebInterfaceUrl}/{result.DownloadUrl}";
		}

		public static async Task<string> GetDownloadUrlByNameAndSecretKey(this ServiceAPI serviceApi, string fileName, string secretKey)
		{
			var result = await serviceApi.DocumentManagement.GetDownloadUrl(secretKey, fileName);
			return $"{ServiceApiHelper.WebInterfaceUrl}/{result.DownloadUrl}";
		}

		public static async Task<string> GetDownloadUrlByNameAndSecretKey(Context context, string fileName, string secretKey)
		{
			var result = await ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDownloadUrl(secretKey, fileName);
			return $"{ServiceApiHelper.WebInterfaceUrl}/{result.DownloadUrl}";
		}

		public static async Task<byte[]> GetFileContent(this ServiceAPI serviceApi, string downloadUrl)
		{
			return await serviceApi.DocumentManagement.DownloadAsync(downloadUrl);
		}

		public static async Task<byte[]> GetFileContent(Context context, string downloadUrl)
		{
			return await ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.DownloadAsync(downloadUrl);
		}

		public static bool IsFolderExist(Context context, string fullPath)
		{
			try
			{
				var folder = ServiceApiHelper.GetServiceApiInstance(context).GetDMObject(fullPath);
				if (folder == null)
				{
					//LogExtension.Log("IsFolderExist-" + false, Session);
					return false;
				}
			}
			catch (System.Exception)
			{
				//LogExtension.Log("IsFolderExist-false", Session);
				return false;
			}
			return true;
		}

		public static bool IsFolderExist(this ServiceAPI serviceApi, string fullPath)
		{
			try
			{
				var folder = serviceApi.GetDMObject(fullPath);
				if (folder == null)
				{
					//LogExtension.Log("IsFolderExist-" + false, Session);
					return false;
				}
			}
			catch (System.Exception)
			{
				//LogExtension.Log("IsFolderExist-false", Session);
				return false;
			}
			return true;
		}

		public static FileResponse Upload(Context context, string dmPath, byte[] data, long major = 1, long minor = 0)
		{
			var serviceApi = ServiceApiHelper.GetServiceApiInstance(context);
			string[] pathParts = dmPath.Split("/");
			string folderPat = string.Join("/", pathParts.SkipLast(1));
			var folders = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(folderPat)).Result;
			string contentType = GetMimeTypeForFileExtension(pathParts.Last());

			if (folders.Success)
			{
				GetDMObjectsResponse getDMObjectResponse = folders.Result;
				if (getDMObjectResponse.Items?.Count > 0)
				{
					string secretkey = getDMObjectResponse.Items[0].SecretKey;
					Dictionary<string, string> fileName = new Dictionary<string, string>
						{
							{"tr-TR",pathParts.Last()},
							{"en-US",pathParts.Last()}

						};
					FileContentInfo fileContentInfo = new FileContentInfo(contentType, data.Length);
					CreateFileRequest createFileRequest = new CreateFileRequest(secretkey, fileContentInfo, fileName, new Dictionary<string, string>(), null, new FileVersion() { Major = major, Minor = minor, VersionDate = DateTime.Now });

					var createFileResponse = serviceApi.DocumentManagement.CreateFile(createFileRequest).Result;
					if (createFileResponse.Success)
					{
						string fileSecretkey = createFileResponse.Result.SecretKey;
						var uploadParts = serviceApi.DocumentManagement.GetUploadParts(new GetUploadPartsRequest(fileSecretkey, null, data.Length)).Result;
						if (uploadParts.Success)
						{
							serviceApi.DocumentManagement.Upload(data, uploadParts.Result.UploadParts);
						}
						var res = createFileResponse.Result;
						return new FileResponse { PathId = res.Id, SecretKey = res.SecretKey };
					}
				}
			}
			return new FileResponse { PathId = 0, SecretKey = "" };
		}

		public static FileResponse Upload(this ServiceAPI serviceApi, string dmPath, byte[] data, long major = 1, long minor = 0)
		{
			string[] pathParts = dmPath.Split("/");
			string folderPat = string.Join("/", pathParts.SkipLast(1));
			var folders = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(folderPat)).Result;
			string contentType = GetMimeTypeForFileExtension(pathParts.Last());

			if (folders.Success)
			{
				GetDMObjectsResponse getDMObjectResponse = folders.Result;
				if (getDMObjectResponse.Items?.Count > 0)
				{
					string secretkey = getDMObjectResponse.Items[0].SecretKey;
					Dictionary<string, string> fileName = new Dictionary<string, string>
						{
							{"tr-TR",pathParts.Last()},
							{"en-US",pathParts.Last()}

						};
					FileContentInfo fileContentInfo = new FileContentInfo(contentType, data.Length);
					CreateFileRequest createFileRequest = new CreateFileRequest(secretkey, fileContentInfo, fileName, new Dictionary<string, string>(), null, new FileVersion() { Major = major, Minor = minor, VersionDate = DateTime.Now });

					var createFileResponse = serviceApi.DocumentManagement.CreateFile(createFileRequest).Result;
					if (createFileResponse.Success)
					{
						string fileSecretkey = createFileResponse.Result.SecretKey;
						var uploadParts = serviceApi.DocumentManagement.GetUploadParts(new GetUploadPartsRequest(fileSecretkey, null, data.Length)).Result;
						if (uploadParts.Success)
						{
							serviceApi.DocumentManagement.Upload(data, uploadParts.Result.UploadParts);
						}
						var res = createFileResponse.Result;
						return new FileResponse { PathId = res.Id, SecretKey = res.SecretKey };
					}
				}
			}
			return new FileResponse { PathId = 0, SecretKey = "" };
		}

		public class FileResponse
		{
			public long PathId { get; set; }
			public string SecretKey { get; set; } = "";
		}
	
	}
	#endregion

	#region PdfHelper
	public static class PdfHelper
	{

		public static CreateFileResponse CreatePdfWithSpireDoc(Context context, string templateDMPath, string uploadDMPath, Dictionary<object, object> values)
		{
			string link = string.Empty;
			bool isOk = false;
			string templateUrl = DmHelper.GetDMDownloadUrl(context, templateDMPath);
			byte[] templateBytes = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.DownloadAsync(templateUrl).Result;

			using MemoryStream ms = new MemoryStream();

			Spire.Doc.Document doc = new Spire.Doc.Document();

			doc.LoadFromStream(new MemoryStream(templateBytes), FileFormat.Docx2019);

			foreach (var value in values)
			{
				doc.Replace(matchString: value.Key.ToString(), newValue: value.Value.ToString(), caseSensitive: true, wholeWord: true);
			}


			doc.SaveToFile(ms, FileFormat.PDF);
			ms.Seek(0, SeekOrigin.Begin);

			DmHelper.FileResponse response = DmHelper.Upload(context, uploadDMPath + ".pdf", ms.ToArray());
			if (response.PathId > 0) //isPathId 
			{
				link = DmHelper.GetDMLink(context, uploadDMPath + ".pdf");
				isOk = true;
			}

			return new CreateFileResponse { IsOk = isOk, PathId = response.PathId, SecretKey = response.SecretKey, Link = link };

		}

		public static CreateFileResponse CreatePdfWithSpireDoc(Context context, string uploadDMPath, string stringInBase64)
		{
			string link = string.Empty;
			bool isOk = false;


			byte[] bytes = Convert.FromBase64String(stringInBase64);


			using MemoryStream ms = new MemoryStream();

			Spire.Doc.Document doc = new();

			doc.LoadFromStream(new MemoryStream(bytes), FileFormat.PDF);



			doc.SaveToFile(ms, FileFormat.PDF);
			ms.Seek(0, SeekOrigin.Begin);

			DmHelper.FileResponse response = DmHelper.Upload(context, uploadDMPath + ".pdf", ms.ToArray());
			if (response.PathId > 0) //isPathId 
			{
				link = DmHelper.GetDMLink(context, uploadDMPath + ".pdf");
				isOk = true;
			}

			return new CreateFileResponse { IsOk = isOk, PathId = response.PathId, SecretKey = response.SecretKey, Link = link };


		}

		public static CreateFileResponse CreateFileFromBase64(Context context, string uploadDMPath, string stringInBase64, string extension = "pdf")
		{
			try
			{
				long pathId = 0;
				string secretKey = string.Empty;
				bool isOk = false;


				byte[] bytes = Convert.FromBase64String(stringInBase64);

				using (MemoryStream ms = new(bytes))
				{
					ms.Seek(0, SeekOrigin.Begin);

					DmHelper.FileResponse response = DmHelper.Upload(context, uploadDMPath + "." + extension, ms.ToArray());

					if (response.PathId > 0)
					{
						pathId = response.PathId;
						secretKey = response.SecretKey;
						isOk = true;
					}
				}
				return new CreateFileResponse { IsOk = isOk, PathId = pathId, SecretKey = secretKey };
			}
			catch (Exception ex)
			{
				throw new Exception("CreateFileFromBase64 Hata; " + ex.Message);
			}
		}

		public class CreateFileResponse
		{
			public bool IsOk { get; set; }
			public long PathId { get; set; }
			public string SecretKey { get; set; } = string.Empty;
			public string Link { get; set; } = string.Empty;
		}



	}
	#endregion

	#region SqlHelper
	public static class SqlHelper
	{

		#region Sql


		public static DataTable GetData(string queryString, string conStr, Dictionary<string, string>? prm = null)
		{
			using SqlConnection connection = new(conStr);
			DataTable dt = new();
			connection.Open();

			using (SqlCommand command = new(queryString, connection))
			{
				if (prm != null)
				{
					foreach (var item in prm)
					{
						command.Parameters.AddWithValue("@" + item.Key, item.Value);
					}
				}

				SqlDataAdapter adp = new()
				{
					SelectCommand = command
				};

				adp.Fill(dt);
			}


			return dt;



		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="queryString"></param>
		/// <param name="prm"></param>
		/// <returns></returns>
		public static DataTable GetData(Context context, string queryString, Dictionary<string, string> prm = null)
		{
			Configuration config = ConfigHelper.GetConfig(context);

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


		public static object ExecuteScalar(string queryString, string conStr, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
		{
			using SqlConnection connection = new SqlConnection(conStr);
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

		/// <summary>
		/// For Scalar Value
		/// </summary>
		/// <param name="context"></param>
		/// <param name="queryString"></param>
		/// <param name="prm"></param>
		/// <param name="sqltyp"></param>
		/// <returns></returns>
		public static object ExecuteScalar(Context context, string queryString, Dictionary<string, string> prm = null, CommandType sqltyp = CommandType.Text)
		{
			Configuration config = ConfigHelper.GetConfig(context);

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
		public static int Execute(string queryString, string ConStr, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
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
		public static int Execute(Context context, string queryString, Dictionary<string, string> prm = null) //,   CommandType sqltyp=CommandType.Text)
		{
			Configuration config = ConfigHelper.GetConfig(context);

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
	}
	#endregion

	#region ServiceApiHelper
	public static class ServiceApiHelper
	{
		private static string? _webInterfaceUrl;

		public static FormInstance? FormInstance { get; set; }

		internal static string WebInterfaceUrl
		{
			get
			{
				if (_webInterfaceUrl == null)
				{
					string httpClientOptions = Environment.GetEnvironmentVariable("HTTP_CLIENT_OPTIONS");
					_webInterfaceUrl = JsonConvert.DeserializeObject<Bimser.Synergy.Entities.Shared.Business.Objects.HttpClientOptions>(httpClientOptions).WebInterfaceUrl;
				}
				return _webInterfaceUrl;
			}
		}

		internal static LoginWithTokenAuthenticationParameters GetTokenCredential(Context context)
		{
			return new LoginWithTokenAuthenticationParameters()
			{
				DomainAddress = WebInterfaceUrl,
				Token = context.Token,
				EncryptedData = context.EncryptedData,
				Language = context.Language,
				FootPrint = context.FootPrint
			};
		}



		public static ServiceAPI GetServiceApiInstance(Context context, string webInterfaceUrl = null)
		{
			var credentials = GetTokenCredential(context);
			return new ServiceAPI(credentials, webInterfaceUrl ?? WebInterfaceUrl);
		}


		public static FormInstance GetFormInstance(Context context, string processName, string formName, long documentId = 0)
		{
			var serviceApi = GetServiceApiInstance(context) ?? GetServiceApiInstance(context);
			return serviceApi.FormManager.CreateWithoutView(processName, formName, documentId).Result;
		}
		public static string GetFormControlValue(FormInstance frm, string controlName)
		{
			if (frm.Controls[controlName] is null)
				throw new ArgumentNullException(controlName + " is null");

			var controlValue = frm.Controls[controlName].Value;
			if (controlValue is JValue jValue && jValue.Type == JTokenType.Boolean)
			{
				return jValue.Value<bool>().ToString();
			}
			else
			{
				return controlValue.ToString();
			}
		}

		public static async Task<WorkflowInstance> CreateProcess(Context context, string projectName, string flowName, long processId = 0)
		{
			ServiceAPI ServiceApi = GetServiceApiInstance(context);
			return await ServiceApi.WorkflowManager.Create(projectName, flowName, processId);
		}

		public async static Task StartFlowAsync(Context context, string processName, Dictionary<string, object> values, string userID, string flowName = "Flow1", string AnAkisId = "", Event id = null)
		{
			id ??= new Event() { Id = 4 };

			WorkflowInstance process = CreateProcess(context, processName, flowName, 0).Result;
			foreach (var item in values)
			{
				process.Variables[item.Key] = item.Value;
			}

			process.StartingEvent = id;
			process.SetStarterUserByUserId(userID.ToInt64()).Wait();

			if (AnAkisId != "")
				process.ParentProcessId = AnAkisId.ToInt64();

			await process.SaveAndContinue();
		}

		public static long StartFlow(Context context, string processName, Dictionary<string, object> values, string userID, string flowName = "Flow1", string AnAkisId = "", Event? id = null)
		{
			id ??= new Event() { Id = 4 };

			WorkflowInstance process = CreateProcess(context, processName, flowName, 0).Result;
			foreach (var item in values)
			{
				process.Variables[item.Key] = item.Value;
			}

			process.StartingEvent = id;
			process.SetStarterUserByUserId(userID.ToInt64()).Wait();

			if (AnAkisId != "")
				process.ParentProcessId = Convert.ToInt64(AnAkisId);

			process.SaveAndContinue().Wait();


			return process.ProcessId;
		}

		public static string GetLink(Context context, long processID, long requestID)
		{
			var serviceApi = GetServiceApiInstance(context);
			CreateLinkRequest data = new CreateLinkRequest();
			data.EmbeddedView = false;
			data.LinkType = 0;
			data.Scope = new List<string> {"sysfullaccess",
											"idefullaccess",
											"dmfullaccess",
											"mobilefullaccess",
											"appsfullaccess",
											"menufullaccess",
											"procmanfullaccess",
											"sysaccess",
											"app.",
											"web.announcement",
											"web.announcement.read",
											"webfullaccess",
											"webaccess"};
			data.UserId = 1;
			data.Status = true;
			data.Payload = "{ \"ProcessId\" : " + processID + ", \"RequestId\" : " + requestID + ", \"RequestType\" : 2}";
			//data.RequestLimit = 99;
			//data.ExpireDate = DateTimeOffset.Now.AddDays(25);

			var result = serviceApi.Shared.CreateLink(data).Result;

			//string returnString = Newtonsoft.Json.JsonConvert.SerializeObject(result);

			return result.Result.LinkId;
		}

		internal static List<RelatedDocumentFile> GetRelatedDocuments(Context context, string processName, string formName, long documentId, string controlName)
		{
			var serviceApi = GetServiceApiInstance(context) ?? GetServiceApiInstance(context);

			List<RelatedDocumentFile> files = new List<RelatedDocumentFile>();

			FormInstance mainForm = serviceApi.FormManager.Create(processName, formName, documentId).Result;

			var control = mainForm.Controls[controlName];

			if (control == null)
			{
				LogExtension.Error(controlName + " İsimli RelatedDocuments nesnesi bulunamadı.", context);
			}
			else
			{
				if (string.IsNullOrEmpty(mainForm.Controls[controlName].Value.ToString()) == false)
				{
					files = JsonConvert.DeserializeObject<List<RelatedDocumentFile>>(mainForm.Controls[controlName].Value.ToString());
				}
			}

			return files;
		}






		public static void Bind_Related_Documents(List<RelatedDocumentFile> docs, FormInstance bindForm, string controlName = "rdEkDosya", string categoryName = "Varsayılan", string culture = "tr-TR")
		{
			Control rdControl = bindForm.Controls[controlName];

			if (!((JToken)rdControl.Value).HasValues)
			{
				rdControl.Value = new List<RelatedDocumentFile>();
			}

			List<RelatedDocumentFile> rdFiles1 = ((JArray)rdControl.Value)?.ToObject<List<RelatedDocumentFile>>();

			List<RelatedDocumentCategory> rdCategories1 = ((JArray)rdControl.Categories)?.ToObject<List<RelatedDocumentCategory>>();

			foreach (RelatedDocumentFile file in docs)
			{
				file.Category = rdCategories1.Find(cat => cat.Name[culture].Equals(categoryName));//Varsayilan Kategorideki dosyaları alır
				rdFiles1.Add(file);
			}

			bindForm.Controls[controlName].Value = rdFiles1;
			var resp = bindForm.Save();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="processId"></param>
		/// <param name="flowPauserName"></param>
		/// <param name="projectName"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		public static async Task ContinueFlowAsync(Context context, long processId, string flowPauserName, string projectName, int eventId)
		{
			ServiceAPI serviceAPI = GetServiceApiInstance(context);

			var flowRequests = serviceAPI.WorkflowManager.GetWaitingProcessRequests(processId).Result;
			var flowPauser = flowRequests.Result.Where(x => x.StepName == flowPauserName);


			if (flowPauser.Any() == false)
				throw new Exception("Üst akışı devam ettirmek için durdurucuda beklemesi lazım;" + processId + "-" + flowPauserName);

			else
			{
				var mainProcess = serviceAPI.WorkflowManager.Create(projectName, "Flow1", processId, flowPauser.FirstOrDefault().RequestId).Result;
				mainProcess.StartingEvent = new Event() { Id = eventId };

				var continueResponse = await mainProcess.Continue();
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="processId"></param>
		/// <param name="flowPauserName"></param>
		/// <param name="projectName"></param>
		/// <param name="id"></param>
		/// <param name="variables"></param>
		/// <exception cref="Exception"></exception> 
		public static void ContinueFlow(Context context, long processId, string flowPauserName, string projectName, int eventId, Dictionary<string, string> variables = null)
		{
			ServiceAPI serviceAPI = GetServiceApiInstance(context);

			var flowRequests = serviceAPI.WorkflowManager.GetWaitingProcessRequests(processId).Result;
			var flowPauser = flowRequests.Result.Where(x => x.StepName == flowPauserName);


			//LogExtension.Warning(flowRequests.Result,context);
			if (flowPauser.Any() == false)
				throw new Exception("Üst akışı devam ettirmek için durdurucuda beklemesi lazım;" + processId + "-" + flowPauserName);


			else
			{
				var mainProcess = serviceAPI.WorkflowManager.Create(projectName, "Flow1", processId, flowPauser.FirstOrDefault().RequestId).Result;
				if (variables != null)
				{
					foreach (var variable in variables)
					{
						mainProcess.Variables[variable.Key] = variable.Value;
					}
				}
				mainProcess.StartingEvent = new Event() { Id = eventId };

				var continueResponse = mainProcess.Continue().Result;

			}
		}
		public static bool CheckFlowPauser(Context context, long processId, string flowPauserName)
		{
			ServiceAPI serviceAPI = GetServiceApiInstance(context);

			var flowRequests = serviceAPI.WorkflowManager.GetWaitingProcessRequests(processId).Result;
			var flowPauser = flowRequests.Result.Where(x => x.StepName == flowPauserName);

			if (flowPauser.Any() == true)
				return true;
			else
				return false;

		}

		#region Extensions

		public static GridDataRowCell GetCellByName(this GridDataRow gridDataRow, string name)
		{
			var cell = gridDataRow.Cells.FirstOrDefault(x => x.Name == name);

			return cell is null ? throw new ArgumentNullException($"GridDataRowExtension.Error, A cell named {name} could not be found!") : cell;
		}

		#endregion
	}
	#endregion

	#region Extensions

	public static class Extension
	{
		public static DataTable ToDataTable<T>(this IList<T> data)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

			DataTable table = new DataTable();

			foreach (PropertyDescriptor prop in properties)
				table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
			foreach (T item in data)
			{
				DataRow row = table.NewRow();
				foreach (PropertyDescriptor prop in properties)
					row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
				table.Rows.Add(row);
			}
			return table;
		}

		public static DataTable ToDataTableWithJson<T>(this IList<T> list)
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
			if (string.IsNullOrWhiteSpace(str))
			{
				return false;
			}
			str = str.Trim();
			if ((str.StartsWith("{") && str.EndsWith("}")) || (str.StartsWith("[") && str.EndsWith("]")))
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


		public static string ToJson(this LogClass logClass) { return JsonConvert.SerializeObject(logClass); }


		public static string ToJson<T>(this T classObject) { return JsonConvert.SerializeObject(classObject); }

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


		public static void SetFormControlValue(
			this FormInstance sourceForm,
			FormInstance targetForm,
			string controlName)
		{
			try
			{
				targetForm.SetControlValue(controlName, GetFormControlValue(sourceForm, controlName));
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException(controlName + " için değer atama işlemi başarısız; " + ex.Message);
			}
		}

		public static void SetFormControlValues(
			this FormInstance sourceForm,
			FormInstance targetForm,
			Dictionary<string, string> controlNames)
		{
			try
			{
				foreach (var item in controlNames)
				{
					targetForm.SetControlValue(item.Key, GetFormControlValue(sourceForm, item.Value));
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException("Değer atama işlemi başarısız; " + ex.Message);
			}
		}
	}

	#endregion

	#region ConfigurationManagerHelper

	public class ConfigurationManagerHelper
	{

		string GetConfigValue(Context context, string key)
		{
			ServiceAPI serviceAPi = ServiceApiHelper.GetServiceApiInstance(context);

			var result = serviceAPi.ConfigurationAPI.GetConfiguration(new GetConfigurationRequest
			{
				ConfigurationKey = key
			});


			if (result.Exception is not null)
			{
				throw new Exception("Config Değeri getirilirken hata oluştu; " + JsonConvert.SerializeObject(result.Exception));
			}

			return result.Result.Result.ConfigurationData;
		}

		string GetConfigValue(ServiceAPI serviceAPi, string key)
		{
			var result = serviceAPi.ConfigurationAPI.GetConfiguration(new GetConfigurationRequest
			{
				ConfigurationKey = key
			});


			if (result.Exception is not null)
			{
				throw new Exception("Config Değeri getirilirken hata oluştu; " + JsonConvert.SerializeObject(result.Exception));
			}

			return result.Result.Result.ConfigurationData;
		}

		Dictionary<string, string> GetConfigValues(ServiceAPI serviceAPi, IEnumerable configKeys)
		{
			Dictionary<string, string> configList = new Dictionary<string, string>();
			foreach (var configKey in configKeys)
			{
				var key = configKey.ToString();
				var value = GetConfigValue(serviceAPi, configKey.ToString());

				configList.Add(key, value);
			}

			return configList;
		}


	}

	#endregion


}
