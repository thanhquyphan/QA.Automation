using OpenQA.Selenium;
using TQP.QA.Automation.UI.Core;
using TQP.QA.Automation.UI.Core.Extensions;

namespace TQP.QA.Automation.UI.Test.Pages
{
    public class GoogleSearchResultPage : PageBase
    {
        public GoogleSearchResultPage(BrowserDriver browserDriver) 
            : base(browserDriver) { }

        private IWebElement SearchResultsWrapper
            => WebDriver.WaitAndFindElement(By.Id("search"));

        public bool SearchResultsContainsLink(string link)
        {
            return SearchResultsWrapper.FindElementSafe(By.XPath($"//div[@id='search']//div[@class='g']//a[@href='{link}']")).Exists();
        }
    }
}
