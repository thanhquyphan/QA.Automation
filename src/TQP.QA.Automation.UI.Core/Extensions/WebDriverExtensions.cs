using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

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

        public static IWebElement WaitAndFindElement(this IWebDriver driver, By by, int secs = 60)
        {
            var webDriverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(secs));
            return webDriverWait.Until((wd) => wd.FindElement(by));
        }

        public static IWebElement WaitAndFindElementSafe(this IWebDriver driver, By by, int secs = 60)
        {
            try
            {
                driver.WaitAndFindElement(by, secs);
            }
            catch { }

            return null;
        }


    }
}
