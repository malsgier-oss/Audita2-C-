using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WorkAudit.Core;

namespace WorkAudit.UI.Views.Controls;

public partial class PropertiesDock : UserControl
{
    public PropertiesDock()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.MainViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
            UpdateContent(vm.SelectedDocument);
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.MainViewModel.SelectedDocument) && sender is ViewModels.MainViewModel vm)
            UpdateContent(vm.SelectedDocument);
    }

    private void UpdateContent(Document? doc)
    {
        if (doc == null)
        {
            Placeholder.Visibility = Visibility.Visible;
            ContentPanel.Visibility = Visibility.Collapsed;
            return;
        }
        Placeholder.Visibility = Visibility.Collapsed;
        ContentPanel.Visibility = Visibility.Visible;
        IdText.Text = $"ID: {doc.Id}";
        TypeText.Text = $"Type: {doc.DocumentType ?? "-"}";
        StatusText.Text = $"Status: {doc.Status}";
        SectionText.Text = $"Section: {doc.Section}";
        PathText.Text = $"Path: {doc.FilePath}";
    }
}
