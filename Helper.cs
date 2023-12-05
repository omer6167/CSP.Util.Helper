using Bimser.Framework.Collection.Extensions;
using Bimser.Framework.Dependency;
using Bimser.Synergy.Entities.DataSource.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSP.Util.Helper
{

    internal static class Helper
    {
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
    }

}
