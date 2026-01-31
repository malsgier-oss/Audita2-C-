using System.Windows;

namespace WorkAudit.UI.Views.Dialogs;

public partial class PreferencesDialog : Window
{
    public PreferencesDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.PreferencesDialogViewModel vm)
            vm.RequestClose += (_, _) => Close();
    }
}
