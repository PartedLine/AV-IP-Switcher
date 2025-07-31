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


        // var appDataPath = Path.Combine(
        //     Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        //     "IpSwitcher");
        // var filePath = Path.Combine(appDataPath, "saved_addresses.xml");
        //
        // if (!File.Exists(filePath)) return;
        //
        // try
        // {
        //     using var reader = new StreamReader(filePath);
        //     var list = (List<IpCompact>)new XmlSerializer(typeof(List<IpCompact>)).Deserialize(reader)!;
        //     reader.Close();
        //     list.RemoveAll(x => x.Id == ipCompact.Id);
        //     using var writer = new StreamWriter(filePath);
        //     new XmlSerializer(typeof(List<IpCompact>)).Serialize(writer, list);
        //     writer.Close();
        //
        //     RefreshSavedItems();
        // }
        // catch (Exception)
        // {
        //     throw new Exception("Something went wrong");
        // }
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