using System.Windows;

namespace WorkAudit.UI;

public partial class App : Application
{
    public static WorkAudit.Core.SessionUser? SessionUser { get; private set; }
    public static WorkAudit.Services.SettingsStore SettingsStore { get; } = new();

    private void OnStartup(object sender, StartupEventArgs e)
    {
        var settings = SettingsStore.Load();
        SettingsStore.EnsureFoldersExist(settings);
        WorkAudit.Data.DatabaseMigrator.EnsureMainDb(SettingsStore.GetEffectiveBaseDir(settings));
        WorkAudit.Data.DatabaseMigrator.EnsureAuditDb(SettingsStore.GetEffectiveBaseDir(settings));

        var identityDialog = new Views.Dialogs.IdentityDialog
        {
            DataContext = new ViewModels.IdentityDialogViewModel()
        };
        if (identityDialog.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var vm = (ViewModels.IdentityDialogViewModel)identityDialog.DataContext;
        SessionUser = vm.Result;
        if (SessionUser == null)
        {
            Shutdown();
            return;
        }

        var mainWindow = new Views.MainWindow(settings);
        mainWindow.Show();
    }
}
