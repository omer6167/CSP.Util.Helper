using Bimser.CSP.DataSource.Api.Models;
using Bimser.Framework.Collection.Extensions;
using Bimser.Framework.Dependency;
using Bimser.Framework.Domain.Option.Pagination;
using Bimser.Synergy.Entities.DataSource.Providers;
using Bimser.Synergy.Entities.DataSource.Providers.WebService.Rest;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.ServiceAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CSP.Util.Helper
{
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

        public static object CreateRestParameter(Dictionary<string,object> keyValuePairs, RestWebServiceParameterType type= RestWebServiceParameterType.Body)
        {
            
            var parameters = new List<RestWebServiceParameterItem>();
            foreach (var item in keyValuePairs)
            {
                parameters.Add(new RestWebServiceParameterItem
                {
                    Type = type,
                    Key = item.Key, Value = item.Value,
                    
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
}
