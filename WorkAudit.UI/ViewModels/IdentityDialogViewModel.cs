using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkAudit.Core;

namespace WorkAudit.UI.ViewModels;

public partial class IdentityDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _staffId = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public SessionUser? Result { get; private set; }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public IdentityDialogViewModel()
    {
        OkCommand = new RelayCommand(Ok, CanOk);
        CancelCommand = new RelayCommand(Cancel);
    }

    partial void OnNameChanged(string value) => ((RelayCommand)OkCommand).NotifyCanExecuteChanged();
    partial void OnStaffIdChanged(string value) => ((RelayCommand)OkCommand).NotifyCanExecuteChanged();

    private bool CanOk() => !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(StaffId);

    private void Ok()
    {
        ErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Name is required.";
            return;
        }
        if (string.IsNullOrWhiteSpace(StaffId))
        {
            ErrorMessage = "Staff ID is required.";
            return;
        }
        Result = new SessionUser { Name = Name.Trim(), StaffId = StaffId.Trim() };
        RequestClose?.Invoke(this, true);
    }

    private void Cancel()
    {
        Result = null;
        RequestClose?.Invoke(this, false);
    }

    public event EventHandler<bool>? RequestClose;
}
