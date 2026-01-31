using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

namespace WorkAudit.UI.Views.Pages;

public partial class SearchPage : Page
{
    public SearchPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.SearchPageViewModel vm)
        {
            vm.PropertyChanged += Vm_PropertyChanged;
            UpdatePreview(vm.PreviewFilePath);
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.SearchPageViewModel.PreviewFilePath) && sender is ViewModels.SearchPageViewModel vm)
            UpdatePreview(vm.PreviewFilePath);
    }

    private void Preview_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (System.Windows.Input.Keyboard.Modifiers != System.Windows.Input.ModifierKeys.Control) return;
        if (PreviewWebView.Visibility != System.Windows.Visibility.Visible) return;
        var delta = e.Delta > 0 ? 0.25 : -0.25;
        PreviewWebView.ZoomFactor = Math.Max(0.25, Math.Min(5, PreviewWebView.ZoomFactor + delta));
        e.Handled = true;
    }

    private void UpdatePreview(string? path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            PreviewWebView.Visibility = System.Windows.Visibility.Collapsed;
            PreviewPlaceholder.Visibility = System.Windows.Visibility.Visible;
            PreviewPlaceholder.Text = "Select a result to preview";
            return;
        }

        var ext = Path.GetExtension(path).ToLowerInvariant();
        if (ext is ".pdf" or ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp")
        {
            PreviewPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
            PreviewWebView.Visibility = System.Windows.Visibility.Visible;
            PreviewWebView.Source = new Uri(path);
        }
        else
        {
            PreviewWebView.Visibility = System.Windows.Visibility.Collapsed;
            PreviewPlaceholder.Visibility = System.Windows.Visibility.Visible;
            PreviewPlaceholder.Text = $"Preview not supported for {ext}";
        }
    }
}
