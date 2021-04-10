using Microsoft.Extensions.DependencyInjection;
using System;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.BrowserDrivers.Chrome
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterChromeDriverHttpClient(this IServiceCollection services)
        {
            services.AddHttpClient<ChromeDriverHttpClient>(client =>
            {
                client.BaseAddress = new Uri("https://chromedriver.storage.googleapis.com/");
            });

            return services;
        }

        public static IServiceCollection ConfigureWebDrivers(this IServiceCollection services, AppSettings appSettings)
        {
            if (EnvironmentHelpers.IsLocal())
            {
                //Do some magic to make it easier when running this locally
                //... assume if env is not set its local given that when running non-local this will be configured correctly
                if (appSettings.Browser.BrowserType == AppSettings.BrowserConfiguration.BrowserTypeEnum.Chrome)
                {
                    var sp = services.BuildServiceProvider();
                    ChromeDriverProvider.ConfigureChromeDriverForLocal(appSettings, sp.GetService<ChromeDriverHttpClient>()).Wait();
                }
            }

            return services;
        }
    }
}

