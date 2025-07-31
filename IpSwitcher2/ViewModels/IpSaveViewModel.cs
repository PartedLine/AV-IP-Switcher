using CommunityToolkit.Mvvm.ComponentModel;

namespace IpSwitcher2.ViewModels;

public partial class IpSaveViewModel : ViewModelBase
{
    [ObservableProperty] private string ip;
    [ObservableProperty] private string name;
    [ObservableProperty] private string subnet;
}