using System;
using System.Diagnostics;
using System.Linq;

namespace IpSwitcher2.Classes;

public class SetInterface
{
    private static int GetPrefix(string subnetMask)
    {
        return subnetMask.Split('.')
            .Select(byte.Parse)
            .Sum(octet => System.Numerics.BitOperations.PopCount((uint)octet));
    }
    
    public static void SetIp(string name, string ip, string subnet)
    {
        var process = new Process();
        ProcessStartInfo startInfo = null;
        if (OperatingSystem.IsWindows())
        {
            startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C netsh interface ip set address name=\"{name}\" static {ip} {subnet}"
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            var prefix = GetPrefix(subnet);
            startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"nmcli connection modify {name} ipv4.method manual ipv4.addresses {ip}/{prefix}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            startInfo = new ProcessStartInfo
            {
                FileName = "networksetup",
                Arguments = $"-setmanual \"{name}\" {ip} {subnet}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        
        process.StartInfo = startInfo;
        process.Start();
    }

    public static void SetDhcp(string name)
    {
        var process = new Process();
        ProcessStartInfo startInfo = null;
        if (OperatingSystem.IsWindows())
        {
            startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C netsh interface ip set address name=\"{name}\" dhcp"
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            startInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"nmcli connection modify {name} ipv4.method auto",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            startInfo = new ProcessStartInfo
            {
                FileName = "networksetup",
                Arguments = $"-setdhcp \"{name}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        process.StartInfo = startInfo;
        process.Start();
    }
}
