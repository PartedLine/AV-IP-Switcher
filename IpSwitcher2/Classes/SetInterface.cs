using System.Diagnostics;

namespace IpSwitcher2.Classes;

public class SetInterface
{
    public static void SetIp(string name, string ip, string subnet)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C netsh interface ip set address name=\"{name}\" static {ip} {subnet}"
        };
        process.StartInfo = startInfo;
        process.Start();
    }

    public static void SetDhcp(string name)
    {
        var process = new Process();
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "cmd.exe",
            Arguments = $"/C netsh interface ip set address name=\"{name}\" dhcp"
        };
        process.StartInfo = startInfo;
        process.Start();
    }
}