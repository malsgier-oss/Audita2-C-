using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkAudit.Core;
using WorkAudit.Services;

namespace WorkAudit.UI.ViewModels;

public partial class SearchPageViewModel : ObservableObject
{
    private readonly DocumentService _docService;
    private readonly Action<Document?> _onSelectDocument;

    [ObservableProperty]
    private string _title = "Search";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Document> _results = [];

    [ObservableProperty]
    private Document? _selectedDocument;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _filterSection = string.Empty;

    [ObservableProperty]
    private string _filterStatus = string.Empty;

    public IReadOnlyList<string> StatusOptions { get; } = ["", "Draft", "Reviewed", "Ready for Audit", "Issue", "Cleared"];
    public IReadOnlyList<string> SectionOptions { get; } = ["", "Individuals", "Companies", "Clearing"];

    public ICommand SearchCommand { get; }
    public ICommand SaveNotesCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand OpenFileCommand { get; }

    public string? PreviewFilePath => SelectedDocument != null && File.Exists(SelectedDocument.FilePath) ? SelectedDocument.FilePath : null;

    [ObservableProperty]
    private bool _detailsPanelVisible = true;

    public SearchPageViewModel(
        DocumentService docService,
        Action<Document?> onSelectDocument,
        bool searchPanelVisible = true)
    {
        _docService = docService;
        _onSelectDocument = onSelectDocument;
        DetailsPanelVisible = searchPanelVisible;

        SearchCommand = new RelayCommand(Search);
        SaveNotesCommand = new RelayCommand(SaveNotes, () => SelectedDocument != null);
        DeleteCommand = new RelayCommand(Delete, () => SelectedDocument != null);
        OpenFileCommand = new RelayCommand(OpenFile, () => SelectedDocument != null && File.Exists(SelectedDocument.FilePath));
    }

    partial void OnSelectedDocumentChanged(Document? value)
    {
        _onSelectDocument(value);
        if (value != null)
            Notes = value.Notes ?? string.Empty;
        else
            Notes = string.Empty;
        ((RelayCommand)SaveNotesCommand).NotifyCanExecuteChanged();
        ((RelayCommand)DeleteCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenFileCommand).NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(PreviewFilePath));
    }

    private void Search()
    {
        var filters = new DocumentFilters
        {
            Section = string.IsNullOrWhiteSpace(FilterSection) ? null : FilterSection,
            Status = string.IsNullOrWhiteSpace(FilterStatus) ? null : FilterStatus
        };
        var list = _docService.Search(string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery, filters, 500);
        Results.Clear();
        foreach (var d in list) Results.Add(d);
    }

    private void SaveNotes()
    {
        if (SelectedDocument == null) return;
        _docService.UpdateNotes(SelectedDocument.Id, Notes);
        SelectedDocument.Notes = Notes;
    }

    private void Delete()
    {
        if (SelectedDocument == null) return;
        var id = SelectedDocument.Id;
        _docService.Delete(id);
        Results.Remove(SelectedDocument);
        SelectedDocument = null;
    }

    private void OpenFile()
    {
        if (SelectedDocument == null || !File.Exists(SelectedDocument.FilePath)) return;
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = SelectedDocument.FilePath,
            UseShellExecute = true
        });
    }
}
