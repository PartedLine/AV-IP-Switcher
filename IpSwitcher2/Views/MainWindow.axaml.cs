using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using IpSwitcher2.Classes;
using IpSwitcher2.Models;
using IpSwitcher2.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace IpSwitcher2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void SelectingItemsControl_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count == 0) return;
        if (e.AddedItems[0] is not IpProperties selection) return;

        if (DataContext is not MainWindowViewModel viewModel) return;

        viewModel.InterfaceName = selection.Name;
        viewModel.Ip = selection.Ip;
        viewModel.Subnet = selection.Subnet;
        viewModel.IsDhcp = selection.Dhcp;

        DhcpCheckBox.IsEnabled = true;
        switch ((bool)DhcpCheckBox.IsChecked)
        {
            case false:
                IpBox.IsEnabled = true;
                SubnetBox.IsEnabled = true;
                IpButton.IsEnabled = true;
                IpButton.Content = "Set IP";
                break;
            default:
                IpBox.IsEnabled = false;
                SubnetBox.IsEnabled = false;
                IpButton.IsEnabled = true;
                IpButton.Content = "Set to DHCP";
                break;
        }
    }

    private void Ip_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;

        if ((bool)DhcpCheckBox.IsChecked)
        {
            var name = viewModel.InterfaceName;
            SetInterface.SetDhcp(name);
        }
        else
        {
            var name = viewModel.InterfaceName;
            var ip = IpBox.Text;
            var subnet = SubnetBox.Text;

            if (string.IsNullOrEmpty(ip) || !Validations.IsValidIpv4(ip))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Invalid IP", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                box.ShowAsync();

                return;
            }

            if (string.IsNullOrEmpty(subnet) || !Validations.IsValidIpv4(subnet))
            {
                var box = MessageBoxManager
                    .GetMessageBoxStandard("Error", "Invalid subnet", ButtonEnum.Ok, MsBox.Avalonia.Enums.Icon.Error);
                box.ShowAsync();

                return;
            }

            SetInterface.SetIp(name, ip, subnet);
        }

        Thread.Sleep(500);
        viewModel.RefreshInterfaces();
    }

    private void Ip_OnInitialized(object? sender, EventArgs e)
    {
        if (Validations.IsRunAsAdmin()) return;
        var button = sender as Button;
        button.IsEnabled = false;
        button.Content = "You need to run as admin to change the IP.";
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var checkbox = sender as CheckBox;
        switch ((bool)checkbox.IsChecked)
        {
            case false:
                IpBox.IsEnabled = true;
                SubnetBox.IsEnabled = true;
                IpButton.IsEnabled = true;
                IpButton.Content = "Set IP";
                break;
            default:
                IpBox.IsEnabled = false;
                SubnetBox.IsEnabled = false;
                IpButton.Content = "Set to DHCP";
                break;
        }
    }

    private void Save_OnClick(object? sender, RoutedEventArgs e)
    {
        var ipSaveDialog = new IpSave("", IpBox.Text, SubnetBox.Text);
        ipSaveDialog.SaveCompleted += (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel) viewModel.RefreshSavedItems();
        };
        ipSaveDialog.ShowDialog(this);
    }

    private void SavedList_OnTapped(object? sender, TappedEventArgs e)
    {
        if (SavedList.SelectedItems.Count == 0) return;
        if (SavedList.SelectedItems[0] is not IpCompact selection) return;

        if (DataContext is not MainWindowViewModel viewModel) return;

        viewModel.Ip = selection.Ip;
        viewModel.Subnet = selection.Subnet;
        viewModel.IsDhcp = false;
    }

    private void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: IpCompact ipCompact } && DataContext is MainWindowViewModel viewModel)
            viewModel.DeleteSavedIp(ipCompact);
    }

    private void Refresh_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;

        viewModel.RefreshSavedItems();
        viewModel.RefreshInterfaces();
    }

    private void Input_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        IpButton.IsDefault = true;
    }

    private void Input_OnLostFocus(object? sender, RoutedEventArgs e)
    {
        IpButton.IsDefault = false;
    }

    private async void Export_OnClick(object? sender, RoutedEventArgs e)
    {
        await FileOperations.ExportAddresses(StorageProvider, this);
    }

    private async void Import_OnClick(object? sender, RoutedEventArgs e)
    {
        await FileOperations.ImportAddresses(
            StorageProvider,
            this,
            () =>
            {
                if (DataContext is MainWindowViewModel viewModel) viewModel.RefreshSavedItems();
            });
    }
}