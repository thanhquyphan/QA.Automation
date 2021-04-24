namespace TQP.QA.Automation.Shared
{
    public class AppSettings
    {
        public HostsConfiguration Hosts { get; set; }
        public BrowserConfiguration Browser { get; set; }
        public DeveloperSettingsConfiguration DeveloperSettings { get; set; }

        public class HostsConfiguration
        {
            public string MainApp { get; set; }
        }

        public class BrowserConfiguration
        {
            public BrowserTypeEnum BrowserType { get; set; }

            public ChromeConfiguration Chrome { get; set; }

            public class ChromeConfiguration
            {
                public bool Headless { get; set; }
                public bool RemoteDriver { get; set; }
                public string Host { get; set; }
                public string DriverDirectory { get; set; }
                public ProxyConfiguration ProxySettings { get; set; }

                public class ProxyConfiguration
                {
                    public string Proxy { get; set; }
                }
            }

            public enum BrowserTypeEnum
            {
                Chrome
            }
        }

        public class DeveloperSettingsConfiguration
        {
            public bool AutoOpenReportOnFinish { get; set; }
        }
    }
}
