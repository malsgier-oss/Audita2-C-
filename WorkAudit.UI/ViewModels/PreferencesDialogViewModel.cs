using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkAudit.Core;

namespace WorkAudit.UI.ViewModels;

public partial class PreferencesDialogViewModel : ObservableObject
{
    private readonly SettingsStore _settingsStore;

    [ObservableProperty]
    private string _baseDir = string.Empty;

    [ObservableProperty]
    private string _theme = "light";

    [ObservableProperty]
    private string _language = "en";

    [ObservableProperty]
    private string _ocrLang = "eng";

    [ObservableProperty]
    private bool _searchPanelVisible = true;

    public IReadOnlyList<string> ThemeOptions { get; } = ["light", "dark", "midnight", "modern_dark"];
    public IReadOnlyList<string> LanguageOptions { get; } = ["en", "ar"];
    public IReadOnlyList<string> OcrLangOptions { get; } = ["eng", "ara", "eng+ara"];

    public ICommand BrowseBaseDirCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public PreferencesDialogViewModel(SettingsStore settingsStore, UserSettings settings)
    {
        _settingsStore = settingsStore;
        LoadFrom(settings);

        BrowseBaseDirCommand = new RelayCommand(BrowseBaseDir);
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    public UserSettings? SavedSettings { get; private set; }
    public bool Saved { get; private set; }
    public event EventHandler<bool>? RequestClose;

    private void LoadFrom(UserSettings s)
    {
        BaseDir = _settingsStore.GetEffectiveBaseDir(s);
        Theme = s.Theme;
        Language = s.Language;
        OcrLang = s.OcrLang;
        SearchPanelVisible = s.SearchPanelVisible;
    }

    private void BrowseBaseDir()
    {
        var folderDialog = new System.Windows.Forms.FolderBrowserDialog
        {
            SelectedPath = BaseDir,
            Description = "Select base directory for WorkAudit documents"
        };
        if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            BaseDir = folderDialog.SelectedPath;
    }

    private void Save()
    {
        var settings = _settingsStore.Load();
        settings.BaseDir = string.IsNullOrWhiteSpace(BaseDir) ? string.Empty : BaseDir.Trim();
        settings.Theme = Theme;
        settings.Language = Language;
        settings.OcrLang = OcrLang;
        settings.SearchPanelVisible = SearchPanelVisible;
        _settingsStore.Save(settings);
        _settingsStore.EnsureFoldersExist(settings);
        SavedSettings = settings;
        Saved = true;
        RequestClose?.Invoke(this, true);
    }

    private void Cancel()
    {
        Saved = false;
        RequestClose?.Invoke(this, false);
    }
}
