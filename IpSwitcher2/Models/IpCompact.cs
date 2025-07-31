using CommunityToolkit.Mvvm.ComponentModel;

namespace IpSwitcher2.Models;

public class IpCompact : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ip { get; set; }
    public string Subnet { get; set; }
}