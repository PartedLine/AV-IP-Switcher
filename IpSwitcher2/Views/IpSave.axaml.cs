using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IpSwitcher2.Classes;
using IpSwitcher2.Models;
using IpSwitcher2.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace IpSwitcher2.Views;

public partial class IpSave : Window
{
    public IpSave()
    {
        InitializeComponent();
    }

    public IpSave(string nameMessage, string ipMessage, string subnetMessage)
    {
        DataContext = new IpSaveViewModel();

        if (DataContext is not IpSaveViewModel viewModel) return;

        viewModel.Name = nameMessage;
        viewModel.Ip = ipMessage;
        viewModel.Subnet = subnetMessage;

        InitializeComponent();
    }

    public event EventHandler? SaveCompleted;

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Name.Text))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "The name cannot be empty",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            box.ShowAsync();

            return;
        }

        if (string.IsNullOrEmpty(IpBox.Text) || !Validations.IsValidIpv4(IpBox.Text))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Invalid IP",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            box.ShowAsync();

            return;
        }

        if (string.IsNullOrEmpty(SubnetBox.Text) || !Validations.IsValidSubnetMask(SubnetBox.Text))
        {
            var box = MessageBoxManager
                .GetMessageBoxStandard("Error", "Invalid subnet",
                    ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
            box.ShowAsync();

            return;
        }


        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IpSwitcher");
        var filePath = Path.Combine(appDataPath, "saved_addresses.xml");

        List<IpCompact>? existingEntries = [];
        var nextId = 0;

        var serializer = new XmlSerializer(typeof(List<IpCompact>));

        // Load existing entries if file exists
        if (File.Exists(filePath))
            try
            {
                using var reader = new StreamReader(filePath);
                existingEntries = (List<IpCompact>)serializer.Deserialize(reader)!;
                nextId = existingEntries.Count > 0 ? existingEntries.Max(x => x.Id) + 1 : 0;
            }
            catch (Exception)
            {
                // If there's an error reading the file, we'll start with an empty list
                existingEntries = [];
            }

        var toSave = new IpCompact
        {
            Id = nextId,
            Name = Name.Text,
            Ip = IpBox.Text,
            Subnet = SubnetBox.Text
        };

        // Add new entry
        existingEntries.Add(toSave);

        // Save all entries back to file
        using var writer = new StreamWriter(filePath);
        serializer.Serialize(writer, existingEntries);
        writer.Close();

        // Trigger save completed event
        SaveCompleted?.Invoke(this, EventArgs.Empty);

        // Close the window after saving
        Close();
    }

    private void Input_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        Save.IsDefault = true;
    }

    private void Input_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        Save.IsDefault = false;
    }
}