using Bimser.Synergy.Entities.Shared.Business.Objects;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSP.Util.Helper
{
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

            return new CreateFileResponse { IsOk = isOk, PathId = response.PathId, SecretKey = response.SecretKey,Link = link };


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
}
