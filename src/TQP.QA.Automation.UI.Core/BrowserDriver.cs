using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.Core
{
    public class BrowserDriver : IDisposable
    {
        private readonly BrowserDriverFactorySelenium _browserSeleniumDriverFactory;
        private readonly Lazy<IWebDriver> _currentWebDriverLazy;
        private readonly string _hostUrl;

        private bool _isDisposed;

        public BrowserDriver(
            IOptions<AppSettings> configuration,
            string hostUrl,
            BrowserDriverFactorySelenium browserSeleniumDriverFactory)
        {
            _browserSeleniumDriverFactory = browserSeleniumDriverFactory;
            _currentWebDriverLazy = new Lazy<IWebDriver>(GetWebDriver);

            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.Value == null) throw new ArgumentNullException(nameof(configuration.Value));

            if (string.IsNullOrWhiteSpace(hostUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(hostUrl));

            _hostUrl = hostUrl;
        }

        public IWebDriver Current => _currentWebDriverLazy.Value;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_currentWebDriverLazy.IsValueCreated)
            {
                Current.Quit();
                Current.Dispose();
            }

            _isDisposed = true;
        }

        public void Navigate(string urlPart = null)
        {
            var targetUrl = string.IsNullOrWhiteSpace(urlPart) ? _hostUrl : $"{_hostUrl}/{urlPart}";
            Current.Navigate().GoToUrl(targetUrl);
            Current.Manage().Window.Maximize();
        }

        public void TakeScreenshotWithCurrent()
        {
            if (Current is not ITakesScreenshot its)
                throw new InvalidOperationException("Could not take screenshot with Current driver instance");

            var screenshot = its.GetScreenshot().AsByteArray;

            var screenshotDir = ScreenshotDirectory();

            var tempFileName = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".png";
            var tempFilePath = Path.Combine(screenshotDir, tempFileName);

            File.WriteAllBytes(tempFilePath, screenshot);

            Console.WriteLine($"SCREENSHOT[ {tempFilePath} ]SCREENSHOT");
        }

        private static string ScreenshotDirectory()
        {
            // Assuming screenshots and result are in same folder, change absolute URl to relative

            var screenshotsDir = "./Screenshots";
            //var screenshotsDir = Path.Combine(ResultsDirectory(), "Screenshots");
            if (!Directory.Exists(screenshotsDir))
                Directory.CreateDirectory(screenshotsDir);

            return screenshotsDir;
        }

        private IWebDriver GetWebDriver()
            => _browserSeleniumDriverFactory.GetWebDriver(_hostUrl);
    }
}
