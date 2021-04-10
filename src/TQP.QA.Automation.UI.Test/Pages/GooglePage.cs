using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using TQP.QA.Automation.Shared;
using TQP.QA.Automation.UI.Core;

namespace TQP.QA.Automation.UI.Test.Pages
{
    public class GooglePage : PageBase
    {
        public GooglePage(BrowserDriverManager browserDriverManager, IOptions<AppSettings> appSettings) 
            : base(browserDriverManager, appSettings) { }

        private IWebElement SearchField
            => WebDriver.FindElement(By.Name("q"));

        private GoogleSearchResultPage ResultPage { get; set; }

        public void Navigate()
            => BrowserDriver.Navigate();

        public void EnterSearchValue(string value)
            => SearchField.SendKeys(value);

        public void Search()
            => SearchField.Submit();

        public bool SearchResultsContainsLink(string link)
            => ResultPage.SearchResultsContainsLink(link);

    }
}
