using Bimser.Synergy.ServiceAPI.Models.Authentication;
using Bimser.Synergy.ServiceAPI;
using Newtonsoft.Json;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.Entities.WebInterface.Business.DTOs.Requests;
using Newtonsoft.Json.Linq;
using Bimser.Synergy.ServiceAPI.Models.Workflow;
using Bimser.Synergy.ServiceAPI.Models.Form;
using Bimser.CSP.FormControls.Controls;
using Bimser.Synergy.Entities.Workflow.Models.Properties;
using Bimser.Framework.Web.Models;
using Microsoft.AspNetCore.StaticFiles;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Requests;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Responses;
using Bimser.Synergy.Entities.DocumentManagement.Business.Objects;
using Bimser.CSP.Runtime.Common.Extensions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Bimser.CSP.FormControls.Extensions;

namespace CSP.Util.Helper
{


    public static class ServiceApiHelper
    {
        private static string _webInterfaceUrl;

        internal static string WebInterfaceUrl
        {
            get
            {
                if (_webInterfaceUrl == null)
                {
                    string httpClientOptions = Environment.GetEnvironmentVariable("HTTP_CLIENT_OPTIONS");
                    _webInterfaceUrl = JsonConvert.DeserializeObject<HttpClientOptions>(httpClientOptions).WebInterfaceUrl;
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

        public static List<T> GetData<T>(Context context, string connectionName, string queryName, object parameters)
        {
            var serviceApi = GetServiceApiInstance(context) ?? GetServiceApiInstance(context);
            return serviceApi.DataSourceManager.ExecuteQuery<T>(connectionName, queryName, parameters).Result;
        }

        internal static ServiceAPI GetServiceApiInstance(Context context, string webInterfaceUrl = null)
        {
            var credentials = GetTokenCredential(context);
            return new ServiceAPI(credentials, webInterfaceUrl ?? WebInterfaceUrl);
        }

        //public static ServiceAPI GetServiceApiInstance()
        //{
        //    return new ServiceAPI(BasicCredentials, WebInterfaceUrl);
        //}           

        public static FormInstance GetFormInstance(Context context, string processName, string formName, long documentId = 0)
        {
            var serviceApi = GetServiceApiInstance(context) ?? GetServiceApiInstance(context);
            return serviceApi.FormManager.CreateWithoutView(processName, formName, documentId).Result;
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
            await process.SetStarterUserByUserId(userID.ToInt64());

            if (AnAkisId != "")
                process.ParentProcessId = AnAkisId.ToInt64();

            await process.SaveAndContinue();
        }
        public static long StartFlow(Context context, string processName, Dictionary<string, object> values, string userID, string flowName = "Flow1", string AnAkisId = "", Event id = null)
        {
            id ??= new Event() { Id = 4 };

            WorkflowInstance process = CreateProcess(context, processName, flowName, 0).Result;
            foreach (var item in values)
            {
                process.Variables[item.Key] = item.Value;
            }

            process.StartingEvent = id;
            process.SetStarterUserByUserId(userID.ToInt64());

            if (AnAkisId != "")
                process.ParentProcessId = Convert.ToInt64(AnAkisId);

            process.SaveAndContinue();

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
                LogExtension.Log(controlName + " İsimli RelatedDocuments nesnesi bulunamadı.", context);
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

        internal static byte[] Download(Context context, string path)
        {
            var serviceApi = GetServiceApiInstance(context) ?? GetServiceApiInstance(context);
            return serviceApi.DocumentManagement.DownloadAsync(path).Result;
        }

        internal static long Upload(Context context, string dmPath, byte[] data, long major = 1, long minor = 0)
        {
            var serviceApi = GetServiceApiInstance(context);
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

                        return createFileResponse.Result.Id;
                    }
                }
            }
            return 0;
        }

        internal static string GetMimeTypeForFileExtension(string filePath)
        {
            const string DefaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = DefaultContentType;
            }
            return contentType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDMDownloadUrl(Context context, string path)
        {
            Config config = GetHelper.GetConfig(context);


            WrapResponse<GetDMObjectsResponse> file = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(path)).Result;
            GetDownloadUrlResponse getDownloadUrlResponse = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDownloadUrl(file.Result.Items[0].SecretKey, file.Result.Items[0].Name[file.Result.Items[0].Name.Keys.First()]).Result;

            string downloadUrl = config._CSPApiUrl + getDownloadUrlResponse.DownloadUrl;

            return downloadUrl;

        }
        public static void Bind_Related_Documents(List<RelatedDocumentFile> docs, FormInstance bindForm)
        {
            Control rdControl = bindForm.Controls["rdEkDosya"];

            if (!((JToken)rdControl.Value).HasValues)
            {
                rdControl.Value = new List<RelatedDocumentFile>();
            }

            List<RelatedDocumentFile> rdFiles1 = ((JArray)rdControl.Value)?.ToObject<List<RelatedDocumentFile>>();

            List<RelatedDocumentCategory> rdCategories1 = ((JArray)rdControl.Categories)?.ToObject<List<RelatedDocumentCategory>>();

            foreach (RelatedDocumentFile file in docs)
            {
                file.Category = rdCategories1.Find(cat => cat.Name["tr-TR"].Equals("Varsayılan"));//Varsayilan Kategorideki dosyaları alır
                rdFiles1.Add(file);
            }

            bindForm.Controls["rdEkDosya"].Value = rdFiles1;
            var resp = bindForm.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="project"></param>
        /// <param name="query"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static List<T> ExecQuery<T>(Context context, string project, string query, object prm = null)
        {
            var resp = GetServiceApiInstance(context).DataSourceManager.ExecuteQuery<T>(project, query, prm);

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
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="project"></param>
        /// <param name="query"></param>
        /// <param name="prm"></param>
        /// <returns></returns>
        public async static Task<List<T>> ExecQueryAsync<T>(Context context, string project, string query, object prm = null)
        {
            var resp = await GetServiceApiInstance(context).DataSourceManager.ExecuteQuery<T>(project, query, prm);
            return resp;
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
        public static async Task AkisDevamEttirAsync(Context context, long processId, string flowPauserName, string projectName, Event id)
        {
            var flowRequests = GetServiceApiInstance(context).WorkflowManager.GetWaitingProcessRequests(processId).Result;
            var flowPauser = flowRequests.Result.Where(x => x.StepName == flowPauserName);


            if (flowPauser.Any() == false)
                throw new Exception("Üst akışı devam ettirmek için durdurucuda beklemesi lazım;" + processId + "-" + flowPauserName);

            else
            {
                var mainProcess = GetServiceApiInstance(context).WorkflowManager.Create(projectName, "Flow1", processId, flowPauser.FirstOrDefault().RequestId).Result;
                mainProcess.StartingEvent = id;

                var continueResponse = await mainProcess.Continue();
            }
        }
    }
}


