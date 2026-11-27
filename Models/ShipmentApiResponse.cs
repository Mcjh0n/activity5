namespace HubLink.Desktop.Models;

public sealed class ShipmentApiResponse
{
    public string? BranchCode { get; set; }
    public List<Shipment>? Shipments { get; set; }
}
