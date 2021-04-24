using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SolidToken.SpecFlow.DependencyInjection;
using System;
using TechTalk.SpecFlow;
using TechTalk.SpecRun;
using TQP.QA.Automation.Shared;
using TQP.QA.Automation.UI.BrowserDrivers.Chrome;
using TQP.QA.Automation.UI.Core;

namespace TQP.QA.Automation.UI.Test
{
    [Binding]
    public class Hooks
    {
        private static AppSettings _appSettings;
        private static DateTime _testRunStart;
        private static ServiceCollection _serviceCollection = new ServiceCollection();
        private static IConfiguration _configuration;

        [BeforeTestRun]
        public static void StartRun(IObjectContainer objectContainer)
        {
            RegisterConfiguration(objectContainer);
            
            _testRunStart = DateTime.Now;
        }

        [BeforeScenario]
        public void Startup()
        {
        }

        [ScenarioDependencies]
        public static IServiceCollection CreateServices()
        {
            var serviceCollection = new ServiceCollection();

            if (_configuration == null)
                return serviceCollection;

            serviceCollection.AddSingleton(_configuration);

            var opts = new OptionsWrapper<AppSettings>(_appSettings);
            serviceCollection.AddSingleton<IOptions<AppSettings>>(opts);
            serviceCollection.AddSingleton(new BrowserDriverManager());

            serviceCollection
                .RegisterChromeDriverHttpClient()
                .RegisterPageDrivers(typeof(Hooks))
                .ConfigureWebDrivers(_appSettings);

            _serviceCollection = serviceCollection;

            return serviceCollection;
        }

        private static void RegisterConfiguration(IObjectContainer objectContainer)
        {
            if (_configuration != null)
                return;

            var testRunContext = objectContainer.Resolve<TestRunContext>();
            _configuration = AppSettingsBuilder.BuildConfiguration(testRunContext.TestDirectory);
            _appSettings = AppSettingsBuilder.Build(_configuration, testRunContext.TestDirectory);
        }

        [AfterScenario]
        public void EndScenario()
            => GetBrowserDriver().Dispose();

        [AfterStep]
        public void AfterStep()
            => GetBrowserDriver().TakeScreenshotWithCurrent();

        private static BrowserDriver GetBrowserDriver()
        {
            var sp = _serviceCollection.BuildServiceProvider();
            var browserDriverManager = sp.GetService<BrowserDriverManager>();
            var browserDriver = browserDriverManager.RetrieveCurrentInstance();
            return browserDriver;
        }

        [AfterTestRun]
        public static void FinishRun()
        {
            BrowserDriverManager.DisposeAll();
            ReportService.LaunchReport(_appSettings, _testRunStart);
        }
    }
}
