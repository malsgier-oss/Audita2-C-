using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkAudit.Core;
using WorkAudit.Services;

namespace WorkAudit.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly SettingsStore _settingsStore;
    private readonly Window _owner;

    [ObservableProperty]
    private object? _currentViewModel;

    [ObservableProperty]
    private bool _isSidebarVisible = true;

    [ObservableProperty]
    private bool _isPropertiesDockVisible = true;

    [ObservableProperty]
    private Document? _selectedDocument;

    private readonly UserSettings _settings;

    public MainViewModel(Window owner, SettingsStore settingsStore, UserSettings settings)
    {
        _settingsStore = settingsStore;
        _owner = owner;
        _settings = settings;
        ApplySettings(settings);

        CurrentViewModel = new WebcamPageViewModel();

        NavigateToWebcamCommand = new RelayCommand(() => NavigateTo<WebcamPageViewModel>());
        NavigateToImportCommand = new RelayCommand(() => NavigateTo<ImportPageViewModel>());
        NavigateToSearchCommand = new RelayCommand(NavigateToSearch);
        NavigateToWorkspaceCommand = new RelayCommand(NavigateToWorkspace);
        NavigateToToolsCommand = new RelayCommand(() => NavigateTo<ToolsPageViewModel>());
        NavigateToReportsCommand = new RelayCommand(() => NavigateTo<ReportsPageViewModel>());

        ToggleSidebarCommand = new RelayCommand(ToggleSidebar);
        TogglePropertiesDockCommand = new RelayCommand(TogglePropertiesDock);
        OpenPreferencesCommand = new RelayCommand(OpenPreferences);
        RefreshCommand = new RelayCommand(Refresh);
        SeedDocumentsCommand = new RelayCommand(SeedDocuments);
        SaveNotesCommand = new RelayCommand(SaveNotes);
    }

    public ICommand SeedDocumentsCommand { get; }
    public ICommand SaveNotesCommand { get; }

    public ICommand NavigateToWebcamCommand { get; }
    public ICommand NavigateToImportCommand { get; }
    public ICommand NavigateToSearchCommand { get; }
    public ICommand NavigateToWorkspaceCommand { get; }
    public ICommand NavigateToToolsCommand { get; }
    public ICommand NavigateToReportsCommand { get; }

    public ICommand ToggleSidebarCommand { get; }
    public ICommand TogglePropertiesDockCommand { get; }
    public ICommand OpenPreferencesCommand { get; }
    public ICommand RefreshCommand { get; }

    private void NavigateTo<T>() where T : class, new()
    {
        CurrentViewModel = new T();
    }

    private void NavigateToWorkspace()
    {
        var baseDir = _settingsStore.GetEffectiveBaseDir(_settings);
        var sessionUser = App.SessionUser?.Name ?? "unknown";
        var deviceId = _settings.DeviceId;
        var docService = new DocumentService(baseDir, sessionUser, deviceId);
        CurrentViewModel = new WorkspacePageViewModel(docService, _settingsStore, baseDir, d => SelectedDocument = d);
    }

    private void NavigateToSearch()
    {
        var baseDir = _settingsStore.GetEffectiveBaseDir(_settings);
        var sessionUser = App.SessionUser?.Name ?? "unknown";
        var deviceId = _settings.DeviceId;
        var docService = new DocumentService(baseDir, sessionUser, deviceId);
        CurrentViewModel = new SearchPageViewModel(docService, d => SelectedDocument = d, _settings.SearchPanelVisible);
    }

    private void ToggleSidebar()
    {
        IsSidebarVisible = !IsSidebarVisible;
    }

    private void TogglePropertiesDock()
    {
        IsPropertiesDockVisible = !IsPropertiesDockVisible;
    }

    private void OpenPreferences()
    {
        var settings = _settingsStore.Load();
        var vm = new PreferencesDialogViewModel(_settingsStore, settings);
        var dialog = new Views.Dialogs.PreferencesDialog
        {
            Owner = _owner,
            DataContext = vm
        };
        dialog.ShowDialog();
        if (vm.Saved && vm.SavedSettings != null)
            ApplySettings(vm.SavedSettings);
    }

    private void ApplySettings(Core.UserSettings settings)
    {
        _owner.FlowDirection = settings.Language == "ar"
            ? System.Windows.FlowDirection.RightToLeft
            : System.Windows.FlowDirection.LeftToRight;
    }

    private void Refresh()
    {
        if (CurrentViewModel is WorkspacePageViewModel wvm)
            wvm.LoadDocumentsCommand.Execute(null);
        else if (CurrentViewModel is SearchPageViewModel svm)
            svm.SearchCommand.Execute(null);
    }

    private void SaveNotes()
    {
        if (CurrentViewModel is WorkspacePageViewModel wvm)
            wvm.SaveNotesCommand.Execute(null);
        else if (CurrentViewModel is SearchPageViewModel svm)
            svm.SaveNotesCommand.Execute(null);
    }

    private void SeedDocuments()
    {
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = true,
            Title = "Select files to add as sample documents"
        };
        if (dlg.ShowDialog() != true) return;

        var baseDir = _settingsStore.GetEffectiveBaseDir(_settings);
        var sessionUser = App.SessionUser?.Name ?? "unknown";
        var deviceId = _settings.DeviceId;
        var docService = new DocumentService(baseDir, sessionUser, deviceId);
        var now = DateTime.UtcNow.ToString("o");
        var count = 0;
        foreach (var path in dlg.FileNames.Take(5))
        {
            var doc = new Core.Document
            {
                FilePath = path,
                DocumentType = Path.GetExtension(path).TrimStart('.'),
                CaptureTime = now,
                Source = "Seed",
                Engagement = "Test",
                Section = "Individuals",
                Status = "Draft",
                CreatedByUser = sessionUser,
                CreatedByDevice = deviceId
            };
            docService.Insert(doc);
            count++;
        }
        System.Windows.MessageBox.Show($"Inserted {count} document(s).", "Seed complete", System.Windows.MessageBoxButton.OK);
        Refresh();
    }
}
