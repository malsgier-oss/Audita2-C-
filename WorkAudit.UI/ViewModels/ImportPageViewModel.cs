using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkAudit.UI.ViewModels;

public partial class ImportPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Import";
}
