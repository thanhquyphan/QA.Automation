using OpenQA.Selenium;

namespace TQP.QA.Automation.UI.Core.Extensions
{
    public static class WebElementExtensions
    {
        public static IWebElement FindElementSafe(this IWebElement webElement, By by)
        {
            try
            {
                return webElement.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public static bool Exists(this IWebElement element)
            => element != null;
    }
}
