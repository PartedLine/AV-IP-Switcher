using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using IpSwitcher2.Classes;
using IpSwitcher2.Models;
using IpSwitcher2.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using SukiUI;
using SukiUI.Controls;
using SukiUI.Dialogs;
using Timer = System.Timers.Timer;

namespace IpSwitcher2.Views;

public partial class MainWindow : SukiWindow
{
    private TrayIcon? _trayIcon;
    private readonly Timer _refreshTimer;
    
    public static ISukiDialogManager DialogManager = new SukiDialogManager();
    
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        _refreshTimer = new Timer(5000);
        _refreshTimer.Elapsed += (_, _) => Dispatcher.UIThread.InvokeAsync(UpdateTrayMenu);
        _refreshTimer.Start();
        SetupTrayIcon();
        DialogHost.Manager = DialogManager;
        
        if (ConfigManager.Config.FirstRun)
        {
            Show_FirstRun();
        }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new TrayIcon()
        {
            Icon = new WindowIcon(
                new Bitmap(AssetLoader.Open(new Uri("avares://IpSwitcher2/Assets/icons8-ethernet-96.png")))),
            ToolTipText = "IP Switcher"
        };

        UpdateTrayMenu();
        
        TrayIcon.SetIcons(Application.Current, new TrayIcons {_trayIcon});
    }
    
    private void UpdateTrayMenu()
    {
        var menu = new NativeMenu();

        // Get all interfaces with IP.
        var interfaces = GetInterfaces.GetInterface(true);

        foreach (var iface in interfaces)
        {
            var menuItem = new NativeMenuItem(iface.Name);
            var submenu = new NativeMenu();

            if (!string.IsNullOrEmpty(iface.Ip))
            {
                submenu.Add(new NativeMenuItem($"IP: {iface.Ip}")
                {
                    IsEnabled = false
                });
                
                if (!string.IsNullOrEmpty(iface.Subnet))
                {
                    submenu.Add(new NativeMenuItem($"Subnet: {iface.Subnet}")
                    {
                        IsEnabled = false
                    });
                }
                
                submenu.Add(new NativeMenuItem($"DHCP: {(iface.Dhcp ? "Yes" : "No")}")
                {
                    IsEnabled = false
                });
            }
            else
            {
                submenu.Add(new NativeMenuItem("No IP assigned")
                {
                    IsEnabled = false
                });
            }
            
            menuItem.Menu = submenu;
            menu.Add(menuItem);
        }
        
        // Add separator
        menu.Add(new NativeMenuItemSeparator());
        
        // Add preset list
        var presetMenu = new NativeMenuItem("Presets");
        var presetList = new NativeMenu();
        var saved = GetSaved.GetSavedIps();
        foreach (var savedIp in saved)
        {
            var menuItem = new NativeMenuItem(savedIp.Name);
            var submenu = new NativeMenu();

            foreach (var iface in interfaces)
            {
                if (string.IsNullOrEmpty(iface.Ip)) continue;
                var item = new NativeMenuItem(iface.Name);
                item.Click += (_, _) =>
                {
                    Console.WriteLine($"Setting {iface.Name} to {savedIp.Ip}/{savedIp.Subnet}");
                    SetInterface.SetIp(iface.Name, savedIp.Ip, savedIp.Subnet);
                    Thread.Sleep(500);
                    if (DataContext is MainWindowViewModel viewModel) viewModel.RefreshInterfaces();
                    UpdateTrayMenu();
                };
                submenu.Add(item);
            }
            menuItem.Menu = submenu;
            presetList.Add(menuItem);
        }
        
        presetMenu.Menu = presetList;
        
        menu.Add(presetMenu);
        
        // Add separator
        menu.Add(new NativeMenuItemSeparator());

        // Add standard menu items
        var openMenuItem = new NativeMenuItem("Open");
        openMenuItem.Click += TrayIcon_OnClicked;
        menu.Add(openMenuItem);

        var exitMenuItem = new NativeMenuItem("Exit");
        exitMenuItem.Click += ExitButton_OnClicked;
        menu.Add(exitMenuItem);

        // Update the tray icon menu
        // ReSharper disable once InvertIf
        if (_trayIcon != null)
        {
            _trayIcon.Menu = menu;
            _trayIcon.Clicked += TrayIcon_OnClicked;
        }

    }
    
    private void TrayIcon_OnClicked(object? sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void ExitButton_OnClicked(object? sender, EventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
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
        switch ((bool)DhcpCheckBox.IsChecked!)
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
        
        SavedList.SelectedItems?.Clear();
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

    private void MenuAbout_OnClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = new About();
        aboutWindow.ShowDialog(this);
    }

    private void MenuExit_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }
    
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        _refreshTimer.Stop();
        _refreshTimer.Dispose();
        _trayIcon?.Dispose();
        base.OnUnloaded(e);
    }

    private void Theme_OnClick(object? sender, RoutedEventArgs e)
    {
        SukiTheme.GetInstance().SwitchBaseTheme();
    }

    private void Show_FirstRun()
    {
        var textBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(10),
            FontSize = 14
        };

        textBlock.Inlines = new InlineCollection
        {
            new Run("Welcome to IP Switcher!") { FontSize = 24, FontWeight = FontWeight.Bold},
            new LineBreak(), new LineBreak(),
            new Run("What is IP Switcher?") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run(
                "A simple tool to manage and switch between IP configurations for your network interfaces." +
                " Perfect for anyone who frequently needs to change network settings."),
            new LineBreak(), new LineBreak(),

            new Run("Key Features:") { FontWeight = FontWeight.Bold, FontSize = 18 },
            new LineBreak(),

            new Run("System Tray Integration") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• The app stays in your system tray with an Ethernet icon"),
            new LineBreak(),
            new Run("• Left-click the tray icon to open the main window"),
            new LineBreak(),
            new Run("• Right-click the tray icon to:"),
            new LineBreak(),
            new Run("  - View all network interfaces and their current IP settings"),
            new LineBreak(),
            new Run("  - Quick-access your saved IP presets"),
            new LineBreak(),
            new Run("  - Open the main window"),
            new LineBreak(),
            new Run("  - Exit the application"),
            new LineBreak(), new LineBreak(),

            new Run("Network Interface Management") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• View all your network interfaces in one place"),
            new LineBreak(),
            new Run("• For each interface you can:"),
            new LineBreak(),
            new Run("  - See current IP address and subnet mask"),
            new LineBreak(),
            new Run("  - Toggle between DHCP and static IP"),
            new LineBreak(),
            new Run("  - Set custom IP address and subnet mask"),
            new LineBreak(),
            new Run("  - View DHCP status"),
            new LineBreak(), new LineBreak(),

            new Run("IP Presets") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• Save frequently used IP configurations as presets"),
            new LineBreak(),
            new Run("• Apply presets to any network interface with just two clicks"),
            new LineBreak(),
            new Run("• Import/Export your presets to share between computers or colleagues"),
            new LineBreak(),
            new Run("• Delete presets you no longer need"),
            new LineBreak(), new LineBreak(),
            
            new Run("Additional Features") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• Dark/Light theme toggle"),
            new LineBreak(), new LineBreak(),
            
            new Run("Quick Start") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("1. Click the Ethernet icon in your system tray to access quick controls"),
            new LineBreak(),
            new Run("2. Open the main window for full functionality"),
            new LineBreak(),
            new Run("3. Select a network interface from the list"),
            new LineBreak(),
            new Run("4. Either:"),
            new LineBreak(),
            new Run("  - Enable DHCP for automatic IP configuration"),
            new LineBreak(),
            new Run("  - Or disable DHCP to set a static IP address and subnet mask"),
            new LineBreak(),
            new Run("5. Save your common IP configurations as presets for quick access"),
            new LineBreak(), new LineBreak(),
            
            new Run("Important Notes") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• This program requires administrator privileges."),
            new LineBreak(),
            new Run("• The close button minimizes the app to the system tray"),
            new LineBreak(),
            new Run("• Use the Exit button in the menu or tray to completely close the application"),
            new LineBreak(),
            new Run("• Updates are automatically checked on startup"),
            new LineBreak(),
            new Run("• When available, updates can be downloaded from the provided link or through your package manager"),
            new LineBreak(),
            new Run("• This introduction will only show once unless the config file is deleted"),
            new LineBreak(), new LineBreak(),

            new Run("Tips") { FontWeight = FontWeight.Bold },
            new LineBreak(),
            new Run("• Keep the app running in the background for quick access"),
            new LineBreak(),
            new Run("• The system tray menu provides quick access to all interfaces and saved presets"),
            new LineBreak(),
            new Run("• Use the export feature to share configurations with colleagues"),
            new LineBreak(),
            new Run("• Use the refresh button to manually update network interface status")
        };

        var scrollViewer = new ScrollViewer
        {
            Content = textBlock,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MaxHeight = 250,
            MaxWidth = 600,
            Padding = new Thickness(5)
        };

        DialogManager.CreateDialog()
            .WithTitle("First Run Introduction")
            .WithContent(scrollViewer)
            .WithActionButton("Dismiss", _ => { FirstRunSuccess(); }, true)
            .TryShow();
    }

    private static void FirstRunSuccess()
    {
        ConfigManager.Config.FirstRun = false;
        ConfigManager.SaveConfig();
    }
}