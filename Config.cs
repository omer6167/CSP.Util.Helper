namespace CSP.Util.Helper
{
    public class Configuration
    {
        public Configuration()
        {
              
        }
        public string _ConnStr { get; set; } = "";
        public string _CSPApiUrl { get; set; } = "";
        public string _CSPUrl { get; set; } = "";
        public string _SAPUserName { get; set; } = "";
        public string _SAPPassword { get; set; } = "";
        public string _SAPHost { get; set; } = "";
        public string _SAPPort { get; set; } = "";
        public string _SAPClient { get; set; } = "";
        public string _SAPLanguage { get; set; } = "";

    }

    public static class Config
    {
        public static string _ConnStr { get; set; } = "";
        public static string _CSPApiUrl { get; set; } = "";
        public static string _CSPUrl { get; set; } = "";
        public static string _SAPUserName { get; set; } = "";
        public static string _SAPPassword { get; set; } = "";
        public static string _SAPHost { get; set; } = "";
        public static string _SAPPort { get; set; } = "";
        public static string _SAPClient { get; set; } = "";
        public static string _SAPLanguage { get; set; } = "";
    }

}
