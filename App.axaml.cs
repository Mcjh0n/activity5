using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HubLink.Desktop.Services;
using HubLink.Desktop.ViewModels;
using HubLink.Desktop.Views;
using System.Net.Http;

namespace HubLink.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            var api = new ShipmentApiService(httpClient);
            var csv = new CsvExportService();
            var viewModel = new MainWindowViewModel(api, csv);
            desktop.MainWindow = new MainWindow(viewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }
}
