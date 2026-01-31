using System.Collections.ObjectModel;

namespace WorkAudit.UI.ViewModels;

public class FileSystemNode
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public ObservableCollection<FileSystemNode> Children { get; } = [];
    public bool IsLoaded { get; set; }
}
