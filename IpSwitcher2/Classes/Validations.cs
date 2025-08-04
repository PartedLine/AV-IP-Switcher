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
    
    public static bool IsValidSubnetMask(string subnetMask)
    {
        // Convert the subnet mask to a 32-bit integer
        var octets = subnetMask.Split('.');
        if (octets.Length != 4) return false;
    
        long mask = 0;
        for (var i = 0; i < 4; i++)
        {
            if (!byte.TryParse(octets[i], out byte octet))
                return false;
            mask = (mask << 8) | octet;
        }

        // If mask is 0, it's invalid
        if (mask == 0) return false;
    
        // Convert to binary and check for continuous 1s
        var binary = Convert.ToString(mask, 2).PadLeft(32, '0');
    
        var seenZero = false;
        foreach (var bit in binary)
        {
            if (bit == '0')
                seenZero = true;
            else if (seenZero) // if we see a 1 after seeing a 0, it's invalid
                return false;
        }
    
        return true;
    }
}