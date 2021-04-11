using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.Core
{
    public class ReportService
    {
        public static void LaunchReport(AppSettings appSettings, DateTime testRunStart)
        {
            if (appSettings.DeveloperSettings != null)
            {
                if (appSettings.DeveloperSettings.AutoOpenReportOnFinish)
                {
                    //We need to launch this out of process to allow reports to be built first.
                    var testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .Replace("file:\\", string.Empty);
                    var resultsDir = Path.Combine(testDirectory, "Results");

                    var reportLauncherScript = Path.Combine(testDirectory, "PowershellScripts\\SpecFlowReportLauncher.ps1");

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"powershell.exe",
                            Arguments = $@"& '{reportLauncherScript}' '{resultsDir}' {testRunStart:yyyy-MM-ddTHHmmss}"
                        }
                    };
                    process.Start();

                }
            }
        }
    }
}
