using System.Globalization;

namespace HubLink.Desktop.Models;

public sealed class Shipment
{
    public string TrackingNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset LastUpdatedUtc { get; set; }

    public string LastUpdatedDisplay => LastUpdatedUtc
        .ToUniversalTime()
        .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
}
