using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TQP.QA.Automation.UI.BrowserDrivers.Chrome
{
    public class ChromeDriverHttpClient
    {
        private readonly HttpClient _httpClient;

        public ChromeDriverHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetLatestReleaseByChromeVersion(string chromeVersion)
        {
            var chromeDriverVersionResponse = await _httpClient.GetAsync($"LATEST_RELEASE_{chromeVersion}");
            if (!chromeDriverVersionResponse.IsSuccessStatusCode)
            {
                if (chromeDriverVersionResponse.StatusCode == HttpStatusCode.NotFound)
                    throw new Exception($"ChromeDriver version not found for Chrome version {chromeVersion}");
                else
                    throw new Exception($"ChromeDriver version request failed with status code: {chromeDriverVersionResponse.StatusCode}, reason phrase: {chromeDriverVersionResponse.ReasonPhrase}");
            }

            return await chromeDriverVersionResponse.Content.ReadAsStringAsync();
        }

        public async Task<Stream> Download(string chromeDriverVersion)
        {
            string zipName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                zipName = "chromedriver_win32.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                zipName = "chromedriver_linux64.zip";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                zipName = "chromedriver_mac64.zip";
            else
                throw new PlatformNotSupportedException("Your operating system is not supported.");

            var driverZipResponse = await _httpClient.GetAsync($"{chromeDriverVersion}/{zipName}");
            if (!driverZipResponse.IsSuccessStatusCode)
                throw new Exception($"ChromeDriver download request failed with status code: {driverZipResponse.StatusCode}, reason phrase: {driverZipResponse.ReasonPhrase}");

            return await driverZipResponse.Content.ReadAsStreamAsync();
        }
    }
}
