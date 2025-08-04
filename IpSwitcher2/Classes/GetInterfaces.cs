using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using IpSwitcher2.Models;

namespace IpSwitcher2.Classes;

public class GetInterfaces
{
    public static ObservableCollection<IpProperties> GetInterface()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic is
            {
                OperationalStatus: OperationalStatus.Up,
                NetworkInterfaceType: NetworkInterfaceType.Ethernet
                or NetworkInterfaceType.Wireless80211
            });

        var addresses = new ObservableCollection<IpProperties>();

        foreach (var _interface in interfaces)
        {
            var ip = GetIPv4Address(_interface);

            // Continue if the interface has no IPv4 Address as we're not interested in it.
            if (string.IsNullOrEmpty(ip)) continue;

            var address = new IpProperties(
                ip,
                _interface.Name,
                _interface.NetworkInterfaceType.ToString(),
                GetSubnetMask(_interface),
                GetGateway(_interface),
                HasDhcp(_interface)
            );

            addresses.Add(address);
        }

        return addresses;
    }

    private static string GetIPv4Address(NetworkInterface networkInterface)
    {
        return (from ip in networkInterface.GetIPProperties().UnicastAddresses
            where ip.Address.AddressFamily == AddressFamily.InterNetwork
            select ip.Address.ToString()).FirstOrDefault() ?? string.Empty;
    }

    private static string GetSubnetMask(NetworkInterface networkInterface)
    {
        return (from ip in networkInterface.GetIPProperties().UnicastAddresses
            where ip.Address.AddressFamily == AddressFamily.InterNetwork
            select ip.IPv4Mask.ToString()).FirstOrDefault() ?? string.Empty;
    }

    private static string GetGateway(NetworkInterface networkInterface)
    {
        return (from gateway in networkInterface.GetIPProperties().GatewayAddresses
            where gateway.Address.AddressFamily == AddressFamily.InterNetwork
            select gateway.Address.ToString()).FirstOrDefault() ?? string.Empty;
    }

    private static bool HasDhcp(NetworkInterface networkInterface)
    {
        if (OperatingSystem.IsMacOS())
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "ipconfig",
                    Arguments = $"getpacket {networkInterface.Name}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process == null) return false;

                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Output means DHCP, no output no DHCP
                return !string.IsNullOrWhiteSpace(output);
            }
            catch
            {
                return false;
            }
        }

        var dhcp = networkInterface.GetIPProperties().DhcpServerAddresses;
        return dhcp.Count > 0;
    }
}