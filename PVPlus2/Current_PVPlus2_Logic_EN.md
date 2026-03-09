# Current PVPlus2 Logic

## Status

`PVPlus2` is currently an early-stage WPF rewrite. The project already has:

- a HandyControl tabbed main window,
- a `MainPV` screen bound to `MainPVViewModel`,
- file-picking commands for Excel, P, V, and W files,
- a bound `ProductCode` input,
- a simple text log output,
- an `ExcelData` container model,
- base domain models ported from the legacy system,
- a sheet-dispatch loading structure,
- and an exploratory `Layout` sheet loader draft.

The app is not yet running the legacy calculation pipeline. It is currently focused on UI wiring, worksheet dispatch, and Excel-loading groundwork.

## Window Structure

`MainWindow.xaml` hosts a HandyControl `TabControl` with four tabs:

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

At the moment, `MainPV` is the primary screen under active development.

## MainPV Screen

`Views/MainPVView.xaml` currently contains:

- an Excel file path row,
- P/V/W file path rows,
- `Open` buttons bound to a shared command,
- an `Output` button bound to `LoadExcelCommand`,
- a product-code input bound TwoWay to `ProductCode`,
- placeholder `Start` and `Cancel` buttons,
- placeholder UI for company, delimiter, options, and radio selections,
- and a bottom log area implemented as a read-only multiline `TextBox`.

The screen layout uses WPF `Grid` containers and HandyControl styles.

### DataContext Wiring

`Views/MainPVView.xaml.cs` sets `DataContext = new MainPVViewModel();` in the constructor. The current `MainPV` bindings are therefore connected in code-behind.

## MainPVViewModel

`ViewModels/MainPVViewModel.cs` currently owns both the `MainPV` screen state and the Excel loading flow.

### Observable fields

- `엑셀파일경로`
- `P파일경로`
- `V파일경로`
- `W파일경로`
- `로그텍스트`
- `ProductCode`

These are created from `[ObservableProperty]` fields using CommunityToolkit.Mvvm.

### Private data container

- `_excelData`

This is an instance of `Models/ExcelData.cs`. It is a screen-scoped in-memory container that already exposes grouped dictionaries such as `PLayout`, `VLayout`, and `SLayout`, but the actual Excel-to-container population is still in progress.

### Commands

- `OpenFileCommand`
  - Opens a file dialog.
  - Uses a command parameter (`Excel`, `P`, `V`, `W`) to decide which path property to update.
  - Excel uses an Excel filter; the others use `All Files (*.*)`.

- `LoadExcelCommand`
  - Adds a startup log line.
  - Validates that the Excel path is not blank.
  - Validates that the file exists.
  - Creates `ExcelDataReaderOptions` with `ExcelSchema.NoHeaders`.
  - Determines the workbook type with `ExcelDataReader.GetWorkbookType()`.
  - Opens a `FileStream` with `FileMode.Open`, `FileAccess.Read`, and `FileShare.ReadWrite`.
  - Creates an `ExcelDataReader` through the stream overload.
  - Iterates worksheets and routes each one through `DispatchSheetLoad(sheetName, edr)`.
  - Disposes both the stream and reader with nested `using` blocks.

At this point, `LoadExcel()` has moved beyond raw logging and now acts as an initial worksheet loader shell with per-sheet dispatch.

### Sheet dispatch

`DispatchSheetLoad()` currently routes these worksheet names:

- `Layout`
- `Product`
- `Rider`
- `Rate`
- `Expense`
- `VarChg`
- `SInfo`
- `ChkExprs`

Unknown sheets are skipped silently.

### Current handler status

- `LoadLayoutSheet`
  - Partially implemented.
  - Skips the first two rows as header rows.
  - Reads P/V/S blocks from hard-coded column offsets within each row.
  - Logs `상품코드`, `담보코드`, `Start`, `Length`, `Index`, and `FactorName`.
  - Wraps row parsing in a per-row `try-catch`.

- `LoadProductSheet`
- `LoadRiderSheet`
- `LoadRateSheet`
- `LoadExpenseSheet`
- `LoadVarChgSheet`
- `LoadSInfoSheet`
- `LoadChkExprsSheet`

These methods currently contain only loop skeletons and do not yet populate data.

## Logging

Logging is currently text-based.

- `AddLog(string message)` appends a timestamped line to `로그텍스트`.
- The UI binds `TextBox.Text` to `로그텍스트`.

The current timestamp format is:

- `HH:mm:ss.fffff`

This approach is simple and stable. It also avoids the `RichTextBox.Document` binding issue that was encountered earlier.

## Model Layer

The `Models` folder currently contains:

- `Product`
- `Rider`
- `Rate`
- `Layout`
- `VarChg`
- `Expense`
- `SInfo`
- `ChkExprs`
- `ExcelData`

### Model strategy

Simple scalar fields stay as numeric types where appropriate, while expression-based fields are intentionally stored as raw `string` values for now.

Examples:

- rider expressions,
- expense conditions and formulas,
- variable change formulas,
- check expressions,
- S-related formulas.

The goal is to separate:

1. raw text or cell loading,
2. later expression compilation,
3. and later calculation execution.

This is simpler than the legacy design, where file loading and expression compilation were tightly mixed together.

## ExcelData Container

`ExcelData.cs` is the current in-memory holder for loaded data.

It contains:

- file metadata such as source Excel path and data folder path,
- a load timestamp,
- dictionaries for products and riders,
- a grouped dictionary for rates,
- `PLayout`, `VLayout`, and `SLayout`,
- grouped dictionaries for variable changes, expenses, S information, and check expressions.

Right now the container is created and owned by `MainPVViewModel`. It is not app-global.

## Current Differences From Legacy PVPlus

- WPF + MVVM instead of WinForms + event-heavy forms
- CommunityToolkit.Mvvm instead of manual property/event boilerplate
- planned instance-owned data instead of global static runtime state
- planned dictionary-based lookup instead of list-first lookup
- Sylvan.Data.Excel for modern Excel reading
- stream-based shared-read opening instead of simple path-based opening
- a per-sheet dispatch loader shape is being built first
- expression fields are still raw strings, not compiled delegates yet

## Current Limitations

- `LoadExcel()` still does not populate `_excelData`
- the `Layout` sheet handler is currently logging-only
- the other sheet handlers are still empty
- column positions are hard-coded instead of being generalized
- the `Start`, `Cancel`, company, and option controls are not yet connected to execution flow
- legacy calculation classes are not yet ported

## Expected Next Step

The next natural step is:

1. turn `Layout` logging into real population of `_excelData.PLayout`, `_excelData.VLayout`, and `_excelData.SLayout`,
2. implement the remaining sheet loaders,
3. centralize cell parsing, type conversion, and blank-value handling,
4. and then connect loaded `ExcelData` to lookup and calculation flow.
