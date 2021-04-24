using CliWrap;
using CliWrap.Buffered;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TQP.QA.Automation.Shared;

namespace TQP.QA.Automation.UI.BrowserDrivers.Chrome
{
    public class ChromeDriverProvider
    {
        private static string _defaultDriversDirectoryChrome 
            = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Drivers\\Chrome");

        private readonly AppSettings.BrowserConfiguration.ChromeConfiguration _chromeConfig;
        private readonly string _testDirectory;

        public ChromeDriverProvider(AppSettings appSettings)
        {
            if (appSettings?.Browser?.Chrome == null) throw new ArgumentNullException("ChromeSetting");

            _chromeConfig = appSettings.Browser.Chrome;
            _testDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Replace("file:\\", string.Empty);
        }

        public IWebDriver GetChromeDriver(string hostUrl)
        {
            if (_chromeConfig.RemoteDriver)
            {
                return GetChromeDriverRemote(string.IsNullOrEmpty(_chromeConfig.Host) ? "127.0.0.1" : _chromeConfig.Host);
            }
            else
            {
                return GetChromeDriverLocal(hostUrl);
            }
        }

        private IWebDriver GetChromeDriverRemote(string hubHost)
        {
            var chromeOptions = ChromeOptions();

            //TODO: CFG For this
            var chromeDriver = new RemoteWebDriver(new Uri($"http://{hubHost}:4444/wd/hub"), chromeOptions);

            return chromeDriver;
        }

        public IWebDriver GetChromeDriverLocal(string hostUrl)
        {
            var chromeDriverService = ChromeDriverService.CreateDefaultService(_chromeConfig.DriverDirectory);

            var chromeOptions = ChromeOptions();

            var chromeDriver = new ChromeDriver(chromeDriverService, chromeOptions)
            {
                Url = hostUrl
            };

            return chromeDriver;
        }

        private ChromeOptions ChromeOptions()
        {
            var chromeOptions = new ChromeOptions();

            chromeOptions.AddArgument("ignore-certificate-errors");
            chromeOptions.AddArgument("no-sandbox");
            chromeOptions.AcceptInsecureCertificates = true;

            // Install Chrome extension
            //var extensionPath = Path.Combine(_testDirectory, "Chrome\\0.0.7");
            //
            //if (new DirectoryInfo(extensionPath).Exists)
            //    chromeOptions.AddArguments($"--load-extension={extensionPath}");

            if (_chromeConfig.Headless)
            {
                chromeOptions.AddArgument("headless");
            }

            var httpProxy = Environment.GetEnvironmentVariable("http_proxy");
            if (!string.IsNullOrWhiteSpace(httpProxy))
            {
                var proxyUri = new Uri(httpProxy);
                var proxy = new Proxy()
                {
                    IsAutoDetect = false,
                    Kind = ProxyKind.Manual,
                    SslProxy = $"{proxyUri.Host}:{proxyUri.Port}",
                    HttpProxy = $"{proxyUri.Host}:{proxyUri.Port}"
                };

                chromeOptions.Proxy = proxy;
            }

            return chromeOptions;

        }

        public static async Task ConfigureChromeDriverForLocal(
            AppSettings appSettings,
            ChromeDriverHttpClient chromeDriverHttpClient)
        {
            if (appSettings == null)
                throw new ConfigurationErrorsException();

            if (appSettings.Hosts == null)
                throw new ConfigurationErrorsException();

            if (appSettings.Browser == null)
                throw new ConfigurationErrorsException();

            if (appSettings.Browser.Chrome == null)
                appSettings.Browser.Chrome = new AppSettings.BrowserConfiguration.ChromeConfiguration();

            if (string.IsNullOrWhiteSpace(appSettings.Browser.Chrome.DriverDirectory))
            {
                var defaultChromeDriverDir =
                    new DirectoryInfo(_defaultDriversDirectoryChrome);

                if (!defaultChromeDriverDir.Exists)
                    defaultChromeDriverDir.Create();

                var installedChromeVersion = await GetChromeVersion();

                var chromeDriverFolderMatch = defaultChromeDriverDir.GetDirectories()
                    .SingleOrDefault(x => x.Name.StartsWith(installedChromeVersion, StringComparison.OrdinalIgnoreCase));

                var platformDriverDir = chromeDriverFolderMatch?.FullName;
                if (chromeDriverFolderMatch == null)
                {
                    platformDriverDir = await DownloadChromeDriver(chromeDriverHttpClient, installedChromeVersion);
                }

                if (!Directory.Exists(platformDriverDir))
                    throw new Exception(
                        $"appSettings.Browser.Chrome.DriverDirectory was not configured in appsettings.json and the resolved directory '{platformDriverDir}' did not exist'");

                appSettings.Browser.Chrome.DriverDirectory = platformDriverDir;
            }
        }

        private static async Task<string> DownloadChromeDriver(ChromeDriverHttpClient chromeDriverHttpClient, string installedChromeVersion)
        {
            var chromeDriverVersion = await chromeDriverHttpClient.GetLatestReleaseByChromeVersion(installedChromeVersion);

            string driverName;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                driverName = "chromedriver.exe";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                     RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                driverName = "chromedriver";
            else
                throw new PlatformNotSupportedException("Your operating system is not supported.");

            var versionDir = new DirectoryInfo(Path.Combine(_defaultDriversDirectoryChrome, chromeDriverVersion));
            if (!versionDir.Exists)
                versionDir.Create();

            var targetPath = Path.Combine(_defaultDriversDirectoryChrome, chromeDriverVersion, driverName);

            using (var zipFileStream = await chromeDriverHttpClient.Download(chromeDriverVersion))
            using (var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
            using (var chromeDriverWriter = new FileStream(targetPath, FileMode.Create))
            {
                var entry = zipArchive.GetEntry(driverName);
                using Stream chromeDriverStream = entry.Open();
                await chromeDriverStream.CopyToAsync(chromeDriverWriter);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x {targetPath}" ,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                );
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                process.Kill();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception("Failed to make chromedriver executable");
                }
            }

            return versionDir.FullName;
        }

        private static async Task<string> GetChromeVersion()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var output = Cli.Wrap("wmic")
                        .WithArguments(
                            "datafile where name=\"C:\\\\Program Files (x86)\\\\Google\\\\Chrome\\\\Application\\\\chrome.exe\" get Version /value")
                        .ExecuteBufferedAsync().Task.Result;

                var versionText = output.StandardOutput.Trim();

                if (string.IsNullOrWhiteSpace(versionText))
                {
                    output = Cli.Wrap("wmic")
                    .WithArguments(
                        "datafile where name=\"C:\\\\Program Files\\\\Google\\\\Chrome\\\\Application\\\\chrome.exe\" get Version /value")
                    .ExecuteBufferedAsync().Task.Result;

                    versionText = output.StandardOutput.Trim();

                    //Can't determine chrome version, can't do anything else here, continue
                    //... If chrome is genuinely not installed this will runtime error later
                    //... The version check is limited and assumes default path, don't error here incase it's not installed to the default path
                    if (string.IsNullOrWhiteSpace(versionText))
                        return null;
                }

                var versionRegex = new Regex("Version=((\\d*).*)");
                var groups = versionRegex.Match(versionText);
                var version = new Version(groups.Groups[1].Value);
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "google-chrome",
                            Arguments = "--product-version",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    process.Kill();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occurred trying to execute 'google-chrome --product-version'", ex);
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                try
                {
                    using var process = Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
                            Arguments = "--version",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                        }
                    );
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    process.Kill();

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }

                    output = output.Replace("Google Chrome ", "");
                    return output;
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred trying to execute '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version'", ex);
                }
            }
            else
            {
                throw new PlatformNotSupportedException("Your operating system is not supported.");
            }
        }
    }
}
