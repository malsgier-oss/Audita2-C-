using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkAudit.UI.ViewModels;

public partial class ReportsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Reports";
}
