using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.Core
{
    public class AppSettingsBuilder
    {
        /// <summary>
        /// Root dir for test execution. Should contain all required settings files
        /// </summary>
        /// <param name="testDirectory"></param>
        public static IConfiguration BuildConfiguration(string testDirectory)
        {
            var cfgBuilder = new ConfigurationBuilder();

            var env = EnvironmentHelpers.GetEnvironment();

            var settingsFileNames = new List<string>
            {
                "appsettings.json"
            };

            if (!string.IsNullOrWhiteSpace(env))
                settingsFileNames.Add($"appsettings.{env}.json");

            var settingFilePaths = settingsFileNames.Select(x => Path.Combine(testDirectory, x));

            foreach (var settingFilePath in settingFilePaths)
            {
                if (File.Exists(settingFilePath))
                {
                    Console.WriteLine($"Using settings from {settingFilePath}");
                    object p = cfgBuilder.AddJsonFile(settingFilePath);
                }
                else
                {
                    Console.WriteLine($"Expected Settings file not found, skipping. File path: {settingFilePath}");
                }
            }

            Console.WriteLine($"Using settings from Environment Variables");
            cfgBuilder.AddEnvironmentVariables();

            var cfg = cfgBuilder.Build();

            return cfg;
        }


        public static AppSettings Build(IConfiguration cfg, string testDirectory)
        {
            var appSettings = new AppSettings();

            cfg.Bind(appSettings);

            return appSettings;

        }
    }
}
