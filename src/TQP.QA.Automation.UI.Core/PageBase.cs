using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.Core
{
    public abstract class PageBase
    {
        protected BrowserDriver BrowserDriver { get; set; }
        protected IWebDriver WebDriver => BrowserDriver.Current;

        protected PageBase(BrowserDriverManager browserDriverManager, IOptions<AppSettings> appSettings)
        {
            BrowserDriver = browserDriverManager.GetInstance(appSettings, appSettings.Value.Hosts.MainApp);
        }

        protected PageBase(BrowserDriver browserDriver)
        {
            BrowserDriver = browserDriver;
        }
    }
}