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
        private static string? _webInterfaceUrl;

        public static FormInstance? FormInstance { get; set; }

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
            if(frm.Controls[controlName] is null)
                throw new ArgumentNullException(controlName +" is null");

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

        

        
       
        
        public static void Bind_Related_Documents(List<RelatedDocumentFile> docs, FormInstance bindForm,string controlName = "rdEkDosya", string categoryName="Varsayılan",string culture = "tr-TR")
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
                mainProcess.StartingEvent = new Event() { Id = eventId};
                
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
}


