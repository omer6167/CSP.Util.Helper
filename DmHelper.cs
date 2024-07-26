using Bimser.Framework.Web.Models;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Requests;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Responses;
using Bimser.Synergy.Entities.DocumentManagement.Business.Objects;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.ServiceAPI;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSP.Util.Helper
{
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

                throw new Exception("Link Getirilemedi; "+ ex.Message);
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
                
                throw new Exception("Link Getirilemedi; "+ ex.Message);
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
        public static string GetDMDownloadUrl(Context context,string cspApiUrl, string path)
        {
            ServiceAPI serviceApi = ServiceApiHelper.GetServiceApiInstance(context);

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

        public static WrapResponse<bool> DeleteFile(Context context, string secretKey)
        {
            var serviceApi = ServiceApiHelper.GetServiceApiInstance(context) ?? ServiceApiHelper.GetServiceApiInstance(context);
            return serviceApi.DocumentManagement.DeleteFile(new DeleteFileRequest(secretKey, " ")).Result;
        }
       

        public static (long, string) Upload(Context context, string dmPath, byte[] data, long major = 1, long minor = 0)
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
                        return (res.Id, res.SecretKey);
                    }
                }
            }
            return (0, "");
        }
    }
}
