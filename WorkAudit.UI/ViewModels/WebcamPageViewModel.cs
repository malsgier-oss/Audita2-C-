using CommunityToolkit.Mvvm.ComponentModel;

namespace WorkAudit.UI.ViewModels;

public partial class WebcamPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Webcam";
}
