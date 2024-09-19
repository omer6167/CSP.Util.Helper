using Bimser.Synergy.Entities.Configuration.Business.DTOs.Requests;
using Bimser.Synergy.Entities.Shared.Business.Objects;
using Bimser.Synergy.ServiceAPI;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;


namespace CSP.Util.Helper
{
    public static class ConfigurationManagerHelper
    {
        public static string GetConfigValue(Context context, string key)
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

        public static string GetConfigValue(ServiceAPI serviceAPi, string key)
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

        public static Dictionary<string, string> GetConfigValues(ServiceAPI serviceAPi, IEnumerable configKeys)
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
}
