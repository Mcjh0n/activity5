using HubLink.Desktop.Models;
using HubLink.Desktop.Services;
using Avalonia.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace HubLink.Desktop.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ShipmentApiService _api;
    private readonly CsvExportService _csv;
    private List<Shipment> _allShipments = [];
    private bool _isBusy;
    private string _dataNotice = "No shipment data loaded.";
    private string _baseAddress = "http://127.0.0.1:5080";
    private string _branchCode = "BR-001";
    private string _endpointPath = "/api/shipments";
    private string _testMode = string.Empty;
    private string _selectedStatus = "All";
    private string _message = "Start the supplied local API, then press Refresh to load shipments.";
    private bool _isMessageError;
    private string _lastUpdated = "Last updated: not loaded yet";

    public MainWindowViewModel(ShipmentApiService api, CsvExportService csv)
    {
        _api = api;
        _csv = csv;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
        StatusOptions = ["All", "Pending", "InTransit", "Delivered", "Exception"];
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public AsyncRelayCommand RefreshCommand { get; }
    public IReadOnlyList<string> StatusOptions { get; }
    public ObservableCollection<Shipment> VisibleShipments { get; } = [];

    public string BaseAddress { get => _baseAddress; set => SetField(ref _baseAddress, value); }
    public string BranchCode { get => _branchCode; set => SetField(ref _branchCode, value); }
    public string EndpointPath { get => _endpointPath; set => SetField(ref _endpointPath, value); }
    public string TestMode { get => _testMode; set => SetField(ref _testMode, value); }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetField(ref _selectedStatus, value))
            {
                RebuildVisibleShipments();
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetField(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsNotBusy));
                OnPropertyChanged(nameof(CanExport));
                RefreshCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsNotBusy => !IsBusy;
    public bool CanExport => !IsBusy && HasVisibleShipments;
    public string DataNotice { get => _dataNotice; private set => SetField(ref _dataNotice, value); }
    public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
    public bool IsMessageError
    {
        get => _isMessageError;
        private set
        {
            if (SetField(ref _isMessageError, value))
            {
                OnPropertyChanged(nameof(MessageForeground));
            }
        }
    }
    public IBrush MessageForeground => new SolidColorBrush(Color.Parse(IsMessageError ? "#B91C1C" : "#334155"));
    public string LastUpdated { get => _lastUpdated; private set => SetField(ref _lastUpdated, value); }
    public string Message
    {
        get => _message;
        private set
        {
            if (SetField(ref _message, value))
            {
                OnPropertyChanged(nameof(HasMessage));
            }
        }
    }

    public int TotalCount => _allShipments.Count;
    public int PendingCount => CountStatus("Pending");
    public int InTransitCount => CountStatus("InTransit");
    public int DeliveredCount => CountStatus("Delivered");
    public int ExceptionCount => CountStatus("Exception");

    public async Task RefreshAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        Message = "Contacting the shipment service…";
        IsMessageError = false;

        try
        {
            var requestedBranch = BranchCode;
            var shipments = await _api.GetShipmentsAsync(BaseAddress, requestedBranch, EndpointPath, TestMode);
            _allShipments = shipments.ToList();
            RaiseSummaryChanged();
            RebuildVisibleShipments();
            LastUpdated = $"Last updated: {DateTimeOffset.Now:MMM d, yyyy  h:mm tt}";
            DataNotice = $"Loaded branch {requestedBranch} — {LastUpdated}";
            Message = _allShipments.Count == 0
                ? $"The API successfully returned no shipments for {requestedBranch}."
                : $"Loaded {_allShipments.Count} shipment(s) for {requestedBranch} from the API.";
            IsMessageError = false;
        }
        catch (TaskCanceledException)
        {
            ShowRefreshError("The request timed out. Check the service address and try again.");
        }
        catch (HttpRequestException ex)
        {
            ShowRefreshError(ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
                ? "Could not reach the shipment service. Make sure it is running and the address is correct."
                : ex.Message);
        }
        catch (InvalidDataException ex)
        {
            ShowRefreshError($"The service returned unusable data: {ex.Message}");
        }
        catch (JsonException)
        {
            ShowRefreshError("The service returned invalid JSON. Check the endpoint and retry.");
        }
        catch (ArgumentException ex)
        {
            ShowRefreshError(ex.Message);
        }
        catch (Exception)
        {
            ShowRefreshError("The refresh failed. Check the service settings and try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ExportVisibleAsync(string path)
    {
        if (IsBusy) return;
        if (VisibleShipments.Count == 0)
        {
            Message = "There are no displayed shipments to export. Choose another status or refresh the data.";
            IsMessageError = false;
            return;
        }

        var rows = VisibleShipments.ToList();
        IsBusy = true;
        try
        {
            await _csv.WriteAsync(path, rows);
            Message = $"Exported {rows.Count} displayed shipment(s) to {Path.GetFileName(path)}.";
            IsMessageError = false;
        }
        catch (Exception)
        {
            Message = "The CSV export could not be saved. Choose another location and try again.";
            IsMessageError = true;
        }
        finally { IsBusy = false; }
    }

    public void ReportExportError()
    {
        Message = "The export dialog could not be opened. Try again or choose another location.";
        IsMessageError = true;
    }

    private void ShowRefreshError(string error)
    {
        if (_allShipments.Count > 0 && !DataNotice.StartsWith("Previously loaded data"))
            DataNotice = $"Previously loaded data — latest refresh failed. {DataNotice}";
        Message = _allShipments.Count > 0
            ? $"Refresh failed: {error} Previously loaded data remains on screen ({LastUpdated})."
            : $"Refresh failed: {error}";
        IsMessageError = true;
    }

    private int CountStatus(string status) => _allShipments.Count(item => item.Status == status);

    private void RebuildVisibleShipments()
    {
        var filtered = SelectedStatus == "All"
            ? _allShipments
            : _allShipments.Where(item => item.Status == SelectedStatus).ToList();

        VisibleShipments.Clear();
        foreach (var shipment in filtered)
        {
            VisibleShipments.Add(shipment);
        }

        OnPropertyChanged(nameof(VisibleCount));
        OnPropertyChanged(nameof(HasVisibleShipments));
        OnPropertyChanged(nameof(CanExport));
        OnPropertyChanged(nameof(HasNoVisibleShipments));
        if (IsBusy) return;
        if (_allShipments.Count > 0 && filtered.Count == 0)
        {
            Message = $"No {SelectedStatus} shipments match this filter.";
            IsMessageError = false;
        }
        else if (filtered.Count > 0)
        {
            Message = $"Showing {filtered.Count} shipment(s). Summary counts include all retrieved branch shipments.";
            IsMessageError = false;
        }
    }

    public int VisibleCount => VisibleShipments.Count;
    public bool HasVisibleShipments => VisibleShipments.Count > 0;
    public bool HasNoVisibleShipments => !HasVisibleShipments;

    private void RaiseSummaryChanged()
    {
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(InTransitCount));
        OnPropertyChanged(nameof(DeliveredCount));
        OnPropertyChanged(nameof(ExceptionCount));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
