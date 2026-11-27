using HubLink.Desktop.Models;
using System.Globalization;
using System.Text;

namespace HubLink.Desktop.Services;

public sealed class CsvExportService
{
    private static readonly string[] Header =
        ["TrackingNumber", "Origin", "Destination", "Status", "LastUpdatedUtc"];

    public async Task WriteAsync(string path, IEnumerable<Shipment> shipments, CancellationToken cancellationToken = default)
    {
        var rows = shipments.ToList();
        if (rows.Count == 0)
        {
            throw new InvalidOperationException("There are no shipments to export.");
        }

        var output = new StringBuilder();
        output.AppendLine(string.Join(',', Header));

        foreach (var shipment in rows)
        {
            var values = new[]
            {
                shipment.TrackingNumber,
                shipment.Origin,
                shipment.Destination,
                shipment.Status,
                shipment.LastUpdatedUtc.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture)
            };
            output.AppendLine(string.Join(',', values.Select(EscapeField)));
        }

        await File.WriteAllTextAsync(path, output.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), cancellationToken);
    }

    private static string EscapeField(string? value)
    {
        value ??= string.Empty;
        if (value.IndexOfAny([',', '"', '\r', '\n']) < 0)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
