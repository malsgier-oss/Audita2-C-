using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkAudit.UI.ViewModels;

public partial class ToolsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Tools";
}
