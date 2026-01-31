using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkAudit.Core;
using WorkAudit.Services;

namespace WorkAudit.UI.ViewModels;

public partial class WorkspacePageViewModel : ObservableObject
{
    private readonly DocumentService _docService;
    private readonly SettingsStore _settingsStore;
    private readonly Action<Document?> _onSelectDocument;

    [ObservableProperty]
    private string _title = "Workspace";

    [ObservableProperty]
    private ObservableCollection<Document> _documents = [];

    [ObservableProperty]
    private Document? _selectedDocument;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private string _status = "Draft";

    [ObservableProperty]
    private string _filterSection = string.Empty;

    [ObservableProperty]
    private string _filterBranch = string.Empty;

    [ObservableProperty]
    private string _filterDocType = string.Empty;

    [ObservableProperty]
    private string _filterStatus = string.Empty;

    [ObservableProperty]
    private string _filterDateFrom = string.Empty;

    [ObservableProperty]
    private string _filterDateTo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<FileSystemNode> _fileTreeRoots = [];

    public IReadOnlyList<string> StatusOptions { get; } = ["Draft", "Reviewed", "Ready for Audit", "Issue", "Cleared"];
    public IReadOnlyList<string> SectionOptions { get; } = ["", "Individuals", "Companies", "Clearing"];

    public ICommand LoadDocumentsCommand { get; }
    public ICommand SaveNotesCommand { get; }
    public ICommand ApplyStatusCommand { get; }
    public ICommand MarkReviewedCommand { get; }
    public ICommand ReadyForAuditCommand { get; }
    public ICommand OpenFileCommand { get; }

    public string? PreviewFilePath => SelectedDocument != null && File.Exists(SelectedDocument.FilePath) ? SelectedDocument.FilePath : null;

    private readonly string _baseDir;

    public WorkspacePageViewModel(
        DocumentService docService,
        SettingsStore settingsStore,
        string baseDir,
        Action<Document?> onSelectDocument)
    {
        _docService = docService;
        _settingsStore = settingsStore;
        _baseDir = baseDir;
        _onSelectDocument = onSelectDocument;

        LoadFileTreeRoot();

        LoadDocumentsCommand = new RelayCommand(LoadDocuments);
        SaveNotesCommand = new RelayCommand(SaveNotes, () => SelectedDocument != null);
        ApplyStatusCommand = new RelayCommand(ApplyStatus, () => SelectedDocument != null);
        MarkReviewedCommand = new RelayCommand(MarkReviewed, () => SelectedDocument != null);
        ReadyForAuditCommand = new RelayCommand(ReadyForAudit, () => SelectedDocument != null);
        OpenFileCommand = new RelayCommand(OpenFile, () => SelectedDocument != null && File.Exists(SelectedDocument.FilePath));
    }

    partial void OnSelectedDocumentChanged(Document? value)
    {
        _onSelectDocument(value);
        if (value != null)
        {
            Notes = value.Notes ?? string.Empty;
            Status = value.Status;
        }
        else
        {
            Notes = string.Empty;
            Status = "Draft";
        }
        ((RelayCommand)SaveNotesCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ApplyStatusCommand).NotifyCanExecuteChanged();
        ((RelayCommand)MarkReviewedCommand).NotifyCanExecuteChanged();
        ((RelayCommand)ReadyForAuditCommand).NotifyCanExecuteChanged();
        ((RelayCommand)OpenFileCommand).NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(PreviewFilePath));
    }

    private void LoadDocuments()
    {
        var filters = new DocumentFilters
        {
            Section = string.IsNullOrWhiteSpace(FilterSection) ? null : FilterSection,
            Branch = string.IsNullOrWhiteSpace(FilterBranch) ? null : FilterBranch,
            DocumentType = string.IsNullOrWhiteSpace(FilterDocType) ? null : FilterDocType,
            Status = string.IsNullOrWhiteSpace(FilterStatus) ? null : FilterStatus,
            DateFrom = string.IsNullOrWhiteSpace(FilterDateFrom) ? null : FilterDateFrom,
            DateTo = string.IsNullOrWhiteSpace(FilterDateTo) ? null : FilterDateTo
        };
        var list = _docService.ListDocuments(filters, 500);
        Documents.Clear();
        foreach (var d in list) Documents.Add(d);
    }

    private void SaveNotes()
    {
        if (SelectedDocument == null) return;
        _docService.UpdateNotes(SelectedDocument.Id, Notes);
        SelectedDocument.Notes = Notes;
    }

    private void ApplyStatus()
    {
        if (SelectedDocument == null) return;
        _docService.UpdateStatus(SelectedDocument.Id, Status);
        SelectedDocument.Status = Status;
    }

    private void MarkReviewed()
    {
        if (SelectedDocument == null) return;
        _docService.MarkReviewed(SelectedDocument.Id, setStatusReviewed: true);
        SelectedDocument.Status = "Reviewed";
        SelectedDocument.ReviewedAt = DateTime.UtcNow.ToString("o");
        Status = "Reviewed";
    }

    private void ReadyForAudit()
    {
        if (SelectedDocument == null) return;
        _docService.UpdateStatus(SelectedDocument.Id, "Ready for Audit");
        SelectedDocument.Status = "Ready for Audit";
        Status = "Ready for Audit";
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

    private void LoadFileTreeRoot()
    {
        if (!Directory.Exists(_baseDir)) return;
        FileTreeRoots.Clear();
        var root = new FileSystemNode
        {
            Name = Path.GetFileName(_baseDir) ?? _baseDir,
            FullPath = _baseDir,
            IsDirectory = true
        };
        root.Children.Add(new FileSystemNode { Name = "...", FullPath = "", IsDirectory = false }); // Placeholder for expand arrow
        FileTreeRoots.Add(root);
    }

    public void LoadFileNodeChildren(FileSystemNode node)
    {
        if (!node.IsDirectory || node.IsLoaded) return;
        node.Children.Clear(); // Remove placeholder
        try
        {
            foreach (var dir in Directory.GetDirectories(node.FullPath))
            {
                var child = new FileSystemNode
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    IsDirectory = true
                };
                child.Children.Add(new FileSystemNode { Name = "...", FullPath = "", IsDirectory = false });
                node.Children.Add(child);
            }
            foreach (var file in Directory.GetFiles(node.FullPath))
            {
                node.Children.Add(new FileSystemNode
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsDirectory = false
                });
            }
            node.IsLoaded = true;
        }
        catch { }
    }
}
