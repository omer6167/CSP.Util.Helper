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
        
        public static (bool, string) CreatePdfWithSpireDoc(Context context, string templateDMPath, string uploadDMPath, Dictionary<object, object> values)
        {
            string link = string.Empty;
            bool isOk = false;
            string templateUrl = DmHelper.GetDMDownloadUrl(context, templateDMPath);
            byte[] templateBytes = ServiceApiHelper.GetServiceApiInstance(context).DocumentManagement.DownloadAsync(templateUrl).Result;

            using MemoryStream ms = new MemoryStream();

            Document doc = new Document();

            doc.LoadFromStream(new MemoryStream(templateBytes), FileFormat.Docx2019);

            foreach (var value in values)
            {
                doc.Replace(matchString: value.Key.ToString(), newValue: value.Value.ToString(), caseSensitive: true, wholeWord: true);
            }


            doc.SaveToFile(ms, FileFormat.PDF);
            ms.Seek(0, SeekOrigin.Begin);

            (long,string) item = DmHelper.Upload(context, uploadDMPath + ".pdf", ms.ToArray());
            if (item.Item1 > 0) //isPathId 
            {
                link = DmHelper.GetDMLink(context, uploadDMPath + ".pdf");
                isOk = true;
            }

            return (isOk, link);

        }

        public static (bool, string) CreatePdfWithSpireDoc(Context context, string uploadDMPath, string stringInBase64)
        {
            string link = string.Empty;
            bool isOk = false;


            byte[] bytes = Convert.FromBase64String(stringInBase64);


            using MemoryStream ms = new MemoryStream();

            Document doc = new();

            doc.LoadFromStream(new MemoryStream(bytes), FileFormat.PDF);



            doc.SaveToFile(ms, FileFormat.PDF);
            ms.Seek(0, SeekOrigin.Begin);

            (long, string) item = DmHelper.Upload(context, uploadDMPath + ".pdf", ms.ToArray());
            if (item.Item1 > 0) //isPathId 
            {
                link = DmHelper.GetDMLink(context, uploadDMPath + ".pdf");
                isOk = true;
            }

            return (isOk, link);

        }

        public static (bool, string, string) CreateFileFromBase64(Context context, string uploadDMPath, string stringInBase64, string extension = "pdf")
        {
            try
            {
                string pathId = string.Empty;
                string secretKey = string.Empty;
                bool isOk = false;
                

                byte[] bytes = Convert.FromBase64String(stringInBase64);

                using (MemoryStream ms = new(bytes))
                {
                    ms.Seek(0, SeekOrigin.Begin);

                    (long, string) item = DmHelper.Upload(context, uploadDMPath + "." + extension, ms.ToArray());

                    if (item.Item1 > 0)
                    {
                        pathId = item.Item1.ToString();
                        secretKey = item.Item2;
                        isOk = true;
                    }
                }
                return (isOk, pathId, secretKey);
            }
            catch (Exception ex)
            {
                throw new Exception("CreateFileFromBase64 Hata; " + ex.Message);
            }
        }
    }
}
