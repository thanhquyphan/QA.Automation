using OpenQA.Selenium;

namespace TQP.QA.Automation.UI.Core.Extensions
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElementSafe(this IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }
    }
}
