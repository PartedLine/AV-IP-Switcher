using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace IpSwitcher2.Classes;

public class UpdateChecker
{
    private const string GithubApiUrl = "https://api.github.com/repos/PartedLine/AV-IP-Switcher/releases/latest";
    private const string ReleasesUrl = "https://github.com/PartedLine/AV-IP-Switcher/releases/latest";
    private static bool _hasCheckedForUpdates;

    public static async Task CheckForUpdates()
    {
        if (_hasCheckedForUpdates) return;

        try
        {
            _hasCheckedForUpdates = true;
            var currentVersion = GetCurrentVersion();
            var latestVersion = await GetLatestVersion();

            if (IsNewVersionAvailable(currentVersion, latestVersion))
            {
                await ShowUpdateDialog();
            }
        }
        catch (HttpRequestException)
        {
            // Do nothing
        }
        catch (Exception)
        {
            // Do nothing
        }
    }
    
    private static string GetCurrentVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
    }

    private static async Task<string> GetLatestVersion()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "IpSwitcher-UpdateChecker");

        var response = await client.GetStringAsync(GithubApiUrl);
        
        var match = Regex.Match(response, """
                                          "tag_name":"v?([0-9]+\.[0-9]+\.[0-9]+)"
                                          """);
        return match.Success ? match.Groups[1].Value : "0.0.0";
    }

    private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        var current = Version.Parse(currentVersion);
        var latest = Version.Parse(latestVersion);
        
        return latest > current;
    }

    private static async Task ShowUpdateDialog()
    {
        var messageBox = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ContentTitle = "Update Available",
                ContentMessage = "A new version of IP Switcher is available. Would you like to download it?",
                ButtonDefinitions = ButtonEnum.YesNo,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                MinWidth = 300
            });

        var result = await messageBox.ShowAsync();

        if (result == ButtonResult.Yes)
        {
            OpenBrowser(ReleasesUrl);
        }
    }
    
    private static void OpenBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // Silently fail if browser cannot be opened
        }
    }

}