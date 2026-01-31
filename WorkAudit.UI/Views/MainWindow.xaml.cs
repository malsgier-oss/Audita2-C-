using System.Windows;
using WorkAudit.Core;
using WorkAudit.Services;

namespace WorkAudit.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow(UserSettings settings)
    {
        InitializeComponent();
        DataContext = new ViewModels.MainViewModel(this, App.SettingsStore, settings);
    }
}
