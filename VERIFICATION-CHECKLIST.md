# HubLink Verification Checklist

Verification date: November 27, 2026.
Student: John Mark Fuentes - BSIT 3D.

## Evidence and scope

The project builds with zero warnings and errors. During the initial review, a temporary C# harness used the application's actual ShipmentApiService, MainWindowViewModel, and CsvExportService against the supplied HubLink.Api executable over HTTP at 127.0.0.1:15085. The API was stopped after testing.

After the fixes, ten focused regression assertions passed using a controlled in-memory HTTP handler (not live API evidence). The actual desktop window was launched and inspected, including loading feedback, disabled controls during retrieval, and recovery from an unavailable service at the default address.

| ID | Observed result | Verification status |
|---|---|---|
| AC-01 | Real HTTP retrieval returned all five BR-001 records through the actual application services and ViewModel. XAML binds all required fields. | Integration passed before fixes; final populated-window check remains. |
| AC-02 | All/Pending/InTransit/Delivered/Exception yielded 5/1/2/1/1 visible records. | ViewModel check passed; interactive filter check remains. |
| AC-03 | Summary was 5 total, 1 Pending, 2 InTransit, 1 Delivered, 1 Exception and stayed unchanged under filters. | Integration passed. |
| AC-04 | Window showed Contacting the shipment service and disabled configuration, Refresh, status filter and export during the request; controls recovered afterward. Regression assertions confirmed overlap prevention. | Loading/recovery observed; live successful refresh in final UI remains. |
| AC-05 | Real HTTP 400, 500, delayed timeout, stopped service and retry exercised. Final UI displayed an understandable unreachable-service message. Regression checks confirmed stale-data notice survives filtering/export and clears after success. | Service/ViewModel checks passed; final live stale-warning UI check remains. |
| AC-06 | BR-999 and empty mode returned successful zero records; invalid-json produced failure feedback. | Live integration passed before fixes. |
| AC-07 | Actual application export produced exactly two InTransit rows with the required header. Regression checks passed comma and quotation-mark escaping. | Export logic passed; native save-dialog completion remains. |
| AC-08 | Exporting to a directory produced understandable failure feedback. Empty export created no file. UI disables export when no records exist. | Logic checks passed; native save-dialog failure path remains. |

## CSV sample provenance

HubLink-CSV-Sample.csv is the unchanged filtered CSV created during the real API review on November 27, 2026 using the application's ExportVisibleAsync method. Branch: BR-001. Filter: InTransit. Records: HUB-1001 and HUB-1005. It is not the static format example.

## Final regression checks

All ten passed: retrieval/summary; persistent stale warning; warning after filtering; warning after export; CSV commas/quotes; export write failure; successful retry clears warning; loading prevents overlap; controls recover; empty export creates no file.

## Remaining verification

Starting the supplied service for the final UI pass was rejected by automatic approval review with 'blocked by policy'. No final live UI checks are claimed as complete. Start the supplied service normally and run the remaining UI checks above. Restore /api/shipments, BR-001, and a blank Test Mode afterward.

The activity specifies pair work. This project identifies one student at the user's explicit request; instructor acceptance of individual submission is not established.
