using HubLink.Desktop.Models;
using System.Net;
using System.Text.Json;

namespace HubLink.Desktop.Services;

public sealed class ShipmentApiService(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> ValidStatuses =
        ["Pending", "InTransit", "Delivered", "Exception"];

    public async Task<IReadOnlyList<Shipment>> GetShipmentsAsync(
        string baseAddress,
        string branchCode,
        string endpointPath,
        string? testMode,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(baseAddress.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var baseUri) ||
            (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Enter a valid HTTP or HTTPS service address.", nameof(baseAddress));
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(branchCode, "^BR-[0-9]{3}$"))
        {
            throw new ArgumentException("Branch code must use the format BR-001.", nameof(branchCode));
        }

        var relativePath = endpointPath.Trim().TrimStart('/');
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath.Contains('?') || relativePath.Contains('#'))
        {
            throw new ArgumentException("Enter an endpoint path without a query string, such as /api/shipments.", nameof(endpointPath));
        }

        var query = $"branchCode={Uri.EscapeDataString(branchCode)}";
        if (!string.IsNullOrWhiteSpace(testMode))
        {
            query += $"&mode={Uri.EscapeDataString(testMode.Trim())}";
        }

        var requestUri = new Uri(baseUri, $"{relativePath}?{query}");
        using var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseContentRead, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var serviceMessage = TryGetServiceError(body);
            var status = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}".Trim();
            throw new HttpRequestException(
                string.IsNullOrWhiteSpace(serviceMessage) ? $"The service returned {status}." : $"The service returned {status}: {serviceMessage}",
                null,
                response.StatusCode);
        }

        ShipmentApiResponse? result;
        try
        {
            result = JsonSerializer.Deserialize<ShipmentApiResponse>(body, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException("The service response was not valid shipment JSON.", ex);
        }

        if (result?.Shipments is null)
        {
            throw new InvalidDataException("The service response is missing the shipments list.");
        }

        if (!string.Equals(result.BranchCode, branchCode, StringComparison.Ordinal))
        {
            throw new InvalidDataException("The response branch code does not match the requested branch.");
        }

        foreach (var shipment in result.Shipments)
        {
            if (shipment is null || string.IsNullOrWhiteSpace(shipment.TrackingNumber) ||
                string.IsNullOrWhiteSpace(shipment.Origin) ||
                string.IsNullOrWhiteSpace(shipment.Destination) ||
                string.IsNullOrWhiteSpace(shipment.Status) ||
                shipment.LastUpdatedUtc == default)
            {
                throw new InvalidDataException("The service returned a shipment with missing or invalid required fields.");
            }

            if (!string.Equals(shipment.BranchCode, branchCode, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The service returned a shipment for a different branch.");
            }

            if (!ValidStatuses.Contains(shipment.Status))
            {
                throw new InvalidDataException($"The service returned an unrecognized shipment status: {shipment.Status}.");
            }
        }

        return result.Shipments;
    }

    private static string? TryGetServiceError(string body)
    {
        try
        {
            using var json = JsonDocument.Parse(body);
            return json.RootElement.TryGetProperty("error", out var error) ? error.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
