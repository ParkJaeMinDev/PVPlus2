# Current PVPlus2 Logic

## Status

`PVPlus2` is currently an early-stage WPF rewrite. The project already has:

- a WPF shell with tabs,
- a `MainPV` screen bound to a ViewModel,
- file-picking commands for Excel, P, V, and W files,
- a simple text log output,
- an `ExcelData` container model,
- and base domain models ported from the legacy system.

The app is not yet running the legacy calculation pipeline. It is currently focused on UI wiring and data-loading groundwork.

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
- a placeholder control area for product, company, options, and radio selections,
- and a bottom log area implemented as a read-only multiline `TextBox`.

The screen layout uses WPF `Grid` containers and HandyControl styles.

## MainPVViewModel

`ViewModels/MainPVViewModel.cs` currently owns the screen state.

### Observable fields

- `엑셀파일경로`
- `P파일경로`
- `V파일경로`
- `W파일경로`
- `로그텍스트`

These are created from `[ObservableProperty]` fields using CommunityToolkit.Mvvm.

### Private data container

- `_excelData`

This is an instance of `Models/ExcelData.cs`. It is intended to become the in-memory container for loaded reference data.

### Commands

- `OpenFileCommand`
  - Opens a file dialog.
  - Uses a command parameter (`Excel`, `P`, `V`, `W`) to decide which path property to update.
  - Excel uses an Excel filter; the others use `All Files (*.*)`.

- `LoadExcelCommand`
  - Adds a startup log line.
  - Validates that the Excel path is not blank.
  - Validates that the file exists.
  - Creates an `ExcelDataReader` using Sylvan.Data.Excel with `ExcelSchema.NoHeaders`.
  - Iterates through sheets and rows.
  - Writes sheet names and cell values into the log text.

At this point, the Excel reader is being used for exploration and logging, not for building the final data structures yet.

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

The current design intentionally stores expression-based fields as `string` values for now.

Examples:

- rider expressions,
- expense conditions and formulas,
- variable change formulas,
- check expressions,
- S-related formulas.

The goal is to separate:

1. raw text loading,
2. later expression compilation,
3. and later calculation execution.

This is simpler than the legacy design, where file loading and expression compilation were tightly mixed together.

## ExcelData Container

`ExcelData.cs` is the current in-memory holder for loaded data.

It contains:

- file metadata such as source Excel path and data folder path,
- a load timestamp,
- dictionaries for products and riders,
- grouped dictionaries for rates, layouts, variable changes, expenses, S information, and check expressions.

Right now the container is created and owned by `MainPVViewModel`. It is not app-global.

## Current Differences From Legacy PVPlus

- WPF + MVVM instead of WinForms + event-heavy forms
- CommunityToolkit.Mvvm instead of manual property/event boilerplate
- planned instance-owned data instead of global static runtime state
- planned dictionary-based lookup instead of list-first lookup
- Sylvan.Data.Excel for modern Excel reading
- expression fields are still raw strings, not compiled delegates yet

## Current Limitations

- `LoadExcel()` currently logs workbook contents instead of populating `ExcelData`
- legacy calculation classes are not yet ported
- company rules, layouts, rates, expenses, and checks are not yet connected to execution flow
- no dedicated loader class exists yet
- data parsing and indexing strategy is still being shaped

## Expected Next Step

The next natural step is:

1. read the Excel workbook intentionally,
2. identify which sheet or exported data is needed,
3. populate `ExcelData`,
4. and then add dictionary-based query helpers on top of that data.
