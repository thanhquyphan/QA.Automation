using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using System;
using TQP.QA.Automation.Shared;
using TQP.QA.Automation.UI.BrowserDrivers.Chrome;

namespace TQP.QA.Automation.UI.Core
{
    public class BrowserDriverFactorySelenium
    {
        private readonly AppSettings _appSettings;

        public BrowserDriverFactorySelenium(IOptions<AppSettings> configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Value == null) throw new ArgumentNullException(nameof(configuration.Value));
            if (configuration.Value.Browser == null) throw new ArgumentNullException(nameof(configuration.Value.Browser));

            _appSettings = configuration.Value;
        }

        public IWebDriver GetWebDriver(string hostUrl)
        {
            switch (_appSettings.Browser.BrowserType)
            {
                case AppSettings.BrowserConfiguration.BrowserTypeEnum.Chrome:
                    return GetChromeDriver(hostUrl);
                default:
                    throw new NotImplementedException($"Support for BrowserType '{_appSettings.Browser.BrowserType}' is not implemented");
            }
        }

        private IWebDriver GetChromeDriver(string hostUrl)
        {
            var provider = new ChromeDriverProvider(_appSettings);

            return provider.GetChromeDriver(hostUrl);
        }
    }
}
