using Bimser.Synergy.Entities.Shared.Business.Objects;
using Newtonsoft.Json.Linq;
using System;

namespace CSP.Util.Helper
{
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
}
