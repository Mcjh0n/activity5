# HubLink Verification Checklist

Verification date: November 27, 2026.
Student: John Mark Fuentes - BSIT 3D.

## Evidence and scope

The project builds with zero warnings and errors. During the initial review, a temporary C# harness used the application's actual ShipmentApiService, MainWindowViewModel, and CsvExportService against the supplied HubLink.Api executable over HTTP at 127.0.0.1:15085. The API was stopped after testing.

After the fixes, ten focused regression assertions passed using a controlled in-memory HTTP handler (not live API evidence). The actual desktop window was launched and inspected, including loading feedback, disabled controls during retrieval, and recovery from an unavailable service at the default address.

| ID | Observed result | Verification status |
|---|---|---|
| AC-01 | Real HTTP retrieval returned all five BR-001 records through the actual application services and ViewModel. XAML binds all required fields. | Passed: final window displayed all five API records. |
| AC-02 | All/Pending/InTransit/Delivered/Exception yielded 5/1/2/1/1 visible records. | Passed: all filters verified against live API in final ViewModel; InTransit also selected in actual UI. |
| AC-03 | Summary was 5 total, 1 Pending, 2 InTransit, 1 Delivered, 1 Exception and stayed unchanged under filters. | Passed again against live API on final code. |
| AC-04 | Window showed Contacting the shipment service and disabled configuration, Refresh, status filter and export during the request; controls recovered afterward. Regression assertions confirmed overlap prevention. | Passed: loading/recovery previously observed; final successful UI refresh displayed five records. |
| AC-05 | Real HTTP 400, 500, delayed timeout, stopped service and retry exercised. Final UI displayed an understandable unreachable-service message. Regression checks confirmed stale-data notice survives filtering/export and clears after success. | Passed in final live service/ViewModel checks; persistent notice bound in UI. |
| AC-06 | BR-999 and empty mode returned successful zero records; invalid-json produced failure feedback. | Passed again against live API on final code. |
| AC-07 | Actual application export produced exactly two InTransit rows with the required header. Regression checks passed comma and quotation-mark escaping. | Export logic passed; native save-dialog completion remains. |
| AC-08 | Exporting to a directory produced understandable failure feedback. Empty export created no file. UI disables export when no records exist. | Logic checks passed; native save-dialog failure path remains. |

## CSV sample provenance

HubLink-CSV-Sample.csv is the unchanged filtered CSV created during the real API review on November 27, 2026 using the application's ExportVisibleAsync method. Branch: BR-001. Filter: InTransit. Records: HUB-1001 and HUB-1005. It is not the static format example.

## Final regression checks

All ten passed: retrieval/summary; persistent stale warning; warning after filtering; warning after export; CSV commas/quotes; export write failure; successful retry clears warning; loading prevents overlap; controls recover; empty export creates no file.

## Remaining verification

The student started the supplied service at http://127.0.0.1:5080. The final desktop window successfully retrieved five BR-001 records and displayed the correct fields and summaries. Selecting InTransit displayed HUB-1001 and HUB-1005. Sixteen final live API assertions passed, covering summary, all five filters, HTTP 400/500, invalid JSON, timeout, stale notice, empty/normal responses, empty branch, retry and unwritable export target.

The native save dialog opened, but the Windows automation helper failed to target it ('no screenshot targets found'). The student has been asked to save C:\Users\John Mark Fuentes\AppData\Local\Temp\HubLink-UI-InTransit.csv. File verification and native save-dialog failure checking remain pending; no completion is claimed for those steps.

The student confirmed on November 27, 2026 that the instructor approved individual submission by John Mark Fuentes - BSIT 3D.
