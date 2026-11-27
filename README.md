# HubLink Desktop

HubLink is an Avalonia desktop client for the supplied read-only shipment API. It retrieves shipment JSON over HTTP, displays and filters the returned records, summarizes all shipments for the branch, and exports the currently displayed rows as CSV.

## Student

John Mark Fuentes - BSIT 3D

Individual submission approved by the instructor, as confirmed by the student on November 27, 2026.

## Requirements

- Windows 10/11 x64
- .NET 10 SDK
- The provided HubLink local service, or the instructor's reachable service address

This repository contains the desktop client source. Obtain the instructor-provided HubLink service separately and place its folder beside `HubLink.Desktop`, or configure the instructor's reachable API address.

## Start the supplied service

From the workspace root (the folder containing both `HubLink.Desktop` and `HubLink-PT5-Local-Server-Windows-x64`), run `HubLink-PT5-Local-Server-Windows-x64/Start-HubLink.cmd` and keep its console window open. The default service address is `http://127.0.0.1:5080`. If port 5080 is occupied, start `Start-HubLink-5081.cmd` from the same package folder and use port 5081 instead. The local service listens only on your own computer. For an instructor-hosted service, use the host and port supplied by the instructor.

## Run the desktop app

From the repository folder containing `HubLink.Desktop.csproj`, restore and launch the project:

```powershell
dotnet restore .\HubLink.Desktop.csproj
dotnet run --project .\HubLink.Desktop.csproj
```

In the app, set the service address and branch code. Defaults are `http://127.0.0.1:5080` and `BR-001`. The endpoint defaults to `/api/shipments`; leave Test Mode blank for normal use. Press **Refresh** to make a real GET request. The app's request timeout is eight seconds.

The controlled test modes use endpoint `/api/test/shipments` and a mode such as `empty`, `bad-request`, `server-error`, `invalid-json`, or `delay`. Set the endpoint and mode fields in the app to try them, then restore `/api/shipments` and leave Test Mode blank. The `delay` mode waits ten seconds, so it should trigger the app's timeout.

## Features

- Asynchronous HTTP retrieval with loading feedback and retry by refreshing
- JSON validation, HTTP error handling, timeout handling, and useful error messages
- Status filtering for All, Pending, InTransit, Delivered, and Exception
- Summary counts calculated from all retrieved branch shipments, independent of the active filter
- CSV export of the displayed rows, with quoted and escaped CSV fields
- Editable service address, branch, endpoint, and test mode; no business logic edits are needed to configure a request

## Architecture

- `Models/` contains the API response and shipment data model.
- `Services/ShipmentApiService.cs` handles HTTP requests and JSON validation.
- `Services/CsvExportService.cs` writes correctly escaped CSV data.
- `ViewModels/MainWindowViewModel.cs` performs filtering, summary calculations, and UI state management.
- `Views/MainWindow.axaml` defines the interface; its code-behind only opens the save-file picker and delegates the export to the ViewModel.

## Deliverables

- `HubLink-UI-InTransit.csv` was saved through the desktop export dialog and verified against the two displayed InTransit records.
- `VERIFICATION-CHECKLIST.md` records the acceptance scenarios and current verification status.
- `HubLink-CSV-Sample.csv` contains the two BR-001 InTransit records exported by the application's ViewModel and CSV service after a real request to the supplied API on November 27, 2026. To regenerate a sample containing all branch records while the API is running:

```powershell
dotnet run --project .\HubLink.Desktop.csproj -- --export-sample .\HubLink-CSV-Sample.csv
```

This command defaults to `BR-001` at `http://127.0.0.1:5080`. A different base address and branch can be supplied after the destination path.

## AI-assisted work

AI assistance was used to create the initial application implementation and documentation. The student must review the code, run the acceptance checks, and be prepared to explain the implementation.
