using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using System.Net.Http;

namespace PVPlus
{
    public class UpdateHelper
    {
        public static async Task DownloadAndExtractUpdate(string downloadUrl)
        {
            string currentPath = AppDomain.CurrentDomain.BaseDirectory;
            string zipPath = Path.Combine(currentPath, "update.zip");
            string extractPath = Path.Combine(currentPath, "UpdateFiles");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(downloadUrl);
                using (var fs = new FileStream(zipPath, System.IO.FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
            }

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            string actualSourcePath = extractPath;
            string pvPlusFolder = Path.Combine(extractPath, "PVPlus");
            if (Directory.Exists(pvPlusFolder))
            {
                actualSourcePath = pvPlusFolder;
            }

            string updaterPath = Path.Combine(actualSourcePath, "UpdateHelper.exe");
            string arguments = $"\"{actualSourcePath}\" \"{currentPath}\"";
            Process.Start(updaterPath, arguments);
            Environment.Exit(0);
        }
    }

    public class VersionChecker
    {
        private const string owner = "k-j-k-test";
        private const string repo = "PVPlus";

        public static async Task<(string LatestVersion, string DownloadUrl)> GetLatestVersionInfo()
        {
            var client = new GitHubClient(new ProductHeaderValue("PVPlus-Updater"));
            var releases = await client.Repository.Release.GetAll(owner, repo);

            if (releases.Count > 0)
            {
                var latestRelease = releases[0];
                var asset = latestRelease.Assets.FirstOrDefault(a => a.Name == "PVPlus.zip");
                return (latestRelease.TagName, asset?.BrowserDownloadUrl);
            }

            return (null, null);
        }

        public static bool IsUpdateAvailable(string currentVersion, string latestVersion)
        {
            Version current = Version.Parse(currentVersion.TrimStart('v'));
            Version latest = Version.Parse(latestVersion.TrimStart('v'));
            return latest > current;
        }
    }
}
