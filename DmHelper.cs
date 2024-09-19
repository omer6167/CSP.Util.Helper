using Bimser.Framework.Web.Models;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Requests;
using Bimser.Synergy.Entities.DocumentManagement.Business.DTOs.Responses;
using Bimser.Synergy.Entities.DocumentManagement.Business.Objects;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.ServiceAPI;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public static string GetDMDownloadUrl(Context context,string cspApiUrl, string path)
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

            Bimser.Framework.Web.Models.WrapResponse<GetDMObjectsResponse> dmRootFolder = serviceApi.DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;

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

            Bimser.Framework.Web.Models.WrapResponse<GetDMObjectsResponse> dmRootFolder = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.GetDMObjectsFromPath(new GetDMObjectsFromPathRequest(root)).Result;

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
                        return new FileResponse { PathId = res.Id ,SecretKey = res.SecretKey };
                    }
                }
            }
            return new FileResponse { PathId = 0 ,SecretKey = ""};
        }

        public class FileResponse
        {
            public long PathId { get; set; }
            public string SecretKey { get; set; } = "";
        }
    }
}
