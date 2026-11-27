using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using HubLink.Desktop.ViewModels;

namespace HubLink.Desktop.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
        : this(CreateDefaultViewModel())
    {
    }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private static MainWindowViewModel CreateDefaultViewModel()
    {
        var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        return new MainWindowViewModel(
            new HubLink.Desktop.Services.ShipmentApiService(httpClient),
            new HubLink.Desktop.Services.CsvExportService());
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (!_viewModel.CanExport) return;
        try
        {
        var selectedFile = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export displayed shipments",
            SuggestedFileName = $"HubLink-{_viewModel.BranchCode}-{DateTime.Now:yyyyMMdd-HHmm}.csv",
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV files")
                {
                    Patterns = ["*.csv"],
                    MimeTypes = ["text/csv"]
                }
            ]
        });

        if (selectedFile is null)
        {
            return;
        }

        await _viewModel.ExportVisibleAsync(selectedFile.Path.LocalPath);
        }
        catch (Exception) { _viewModel.ReportExportError(); }
    }
}
