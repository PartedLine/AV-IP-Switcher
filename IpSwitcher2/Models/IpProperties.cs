using CommunityToolkit.Mvvm.ComponentModel;

namespace IpSwitcher2.Models;

public class IpProperties : ObservableObject
{
    private static int _nextId; // Static variable to keep track of the next available ID

    private bool _dhcp;
    private string _gateway;
    private string _ip;
    private string _name;
    private string _subnet;
    private string _type;

    public IpProperties(string ip, string name, string type, string subnet, string gateway, bool dhcp)
    {
        Id = _nextId++; // Assign the next available ID
        Ip = ip;
        Name = name;
        Type = type;
        Subnet = subnet;
        Gateway = gateway;
        Dhcp = dhcp;
    }

    public int Id { get; }

    public string Ip
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }

    public string Subnet
    {
        get => _subnet;
        set => SetProperty(ref _subnet, value);
    }

    public string Gateway
    {
        get => _gateway;
        set => SetProperty(ref _gateway, value);
    }

    public bool Dhcp
    {
        get => _dhcp;
        set => SetProperty(ref _dhcp, value);
    }
}