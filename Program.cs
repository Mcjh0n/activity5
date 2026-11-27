using Avalonia;
using HubLink.Desktop.Services;
using System.Net.Http;

namespace HubLink.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0].Equals("--export-sample", StringComparison.OrdinalIgnoreCase))
        {
            return ExportSampleAsync(args).GetAwaiter().GetResult();
        }

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        return 0;
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static async Task<int> ExportSampleAsync(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: HubLink.Desktop --export-sample <csv-path> [base-address] [branch-code]");
            return 2;
        }

        var destination = Path.GetFullPath(args[1]);
        var baseAddress = args.Length > 2 ? args[2] : "http://127.0.0.1:5080";
        var branchCode = args.Length > 3 ? args[3] : "BR-001";
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
        var api = new ShipmentApiService(httpClient);

        try
        {
            var shipments = await api.GetShipmentsAsync(baseAddress, branchCode, "/api/shipments", null);
            if (shipments.Count == 0)
            {
                Console.Error.WriteLine("The API returned no shipments; no sample CSV was created.");
                return 1;
            }

            await new CsvExportService().WriteAsync(destination, shipments);
            Console.WriteLine($"Exported {shipments.Count} API shipment(s) to {destination}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Sample export failed: {ex.Message}");
            return 1;
        }
    }
}
