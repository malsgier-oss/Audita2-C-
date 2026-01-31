using System.Windows;

namespace WorkAudit.UI.Views.Dialogs;

public partial class IdentityDialog : Window
{
    public IdentityDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.IdentityDialogViewModel vm)
            vm.RequestClose += (_, result) =>
            {
                DialogResult = result;
                Close();
            };
    }
}
