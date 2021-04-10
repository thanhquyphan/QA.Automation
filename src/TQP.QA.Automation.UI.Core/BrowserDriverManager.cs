using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.Core
{
    public class BrowserDriverManager
    {
        private static readonly List<BrowserDriver> BrowserDrivers = new List<BrowserDriver>();

        private BrowserDriver _browserDriver;

        public BrowserDriver GetInstance(IOptions<AppSettings> appSettings, string host)
        {
            if (_browserDriver == null)
            {
                _browserDriver = new BrowserDriver(
                    appSettings,
                    host,
                    new BrowserDriverFactorySelenium(appSettings));

                BrowserDrivers.Add(_browserDriver);
            }

            return _browserDriver;

        }

        public BrowserDriver RetrieveCurrentInstance()
        {
            return _browserDriver;
        }

        public static void DisposeAll()
        {
            foreach (var browserDriver in BrowserDrivers)
            {
                browserDriver.Dispose();
            }
        }
    }
}
