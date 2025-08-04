using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IpSwitcher2.Classes;

public class Validations
{
    public static bool IsRunAsAdmin()
    {
#if DEBUG
        return true;
#else
        if (!OperatingSystem.IsWindows()) return geteuid() == 0;
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);

        [DllImport("libc")]
        static extern uint geteuid();
#endif
    }

    public static bool IsValidIpv4(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4) return false;

        foreach (var part in parts)
        {
            if (!int.TryParse(part, out var value)) return false;
            if (value is < 0 or > 255) return false;
        }

        return true;
    }
}