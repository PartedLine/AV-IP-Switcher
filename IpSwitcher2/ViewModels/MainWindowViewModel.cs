using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using IpSwitcher2.Classes;
using IpSwitcher2.Models;

namespace IpSwitcher2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ObservableCollection<IpProperties> _addresses;
    private ObservableCollection<IpCompact> _saved;
    [ObservableProperty] private string interfaceName;
    [ObservableProperty] private string ip;
    [ObservableProperty] private bool isDhcp;
    [ObservableProperty] private string subnet;

    public MainWindowViewModel()
    {
        Addresses = GetInterfaces.GetInterface();
        Saved = GetSaved.GetSavedIps();
    }

    public ObservableCollection<IpProperties> Addresses
    {
        get => _addresses;
        set => SetProperty(ref _addresses, value);
    }

    public ObservableCollection<IpCompact> Saved
    {
        get => _saved;
        set => SetProperty(ref _saved, value);
    }

    public void DeleteSavedIp(IpCompact ipCompact)
    {
        try
        {
            var addresses = FileService.LoadSavedAddresses();
            addresses.RemoveAll(x => x.Id == ipCompact.Id);
            FileService.SaveAddresses(addresses);
            RefreshSavedItems();
        }
        catch (Exception)
        {
            throw new Exception("Failed to delete saved IP");
        }
    }


    public void RefreshSavedItems()
    {
        Saved.Clear();
        Saved = GetSaved.GetSavedIps();
    }

    public void RefreshInterfaces()
    {
        Addresses.Clear();
        Addresses = GetInterfaces.GetInterface();
    }
}