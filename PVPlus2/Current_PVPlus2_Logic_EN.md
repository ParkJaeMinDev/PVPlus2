# Current PVPlus2 Logic

## Status

`PVPlus2` is an early-stage WPF rewrite of the legacy PVPlus tool.

The project currently has:

- a HandyControl tabbed main window
- a `MainPV` screen bound to `MainPVViewModel`
- file-picking commands for Excel, P, V, and W files
- a bound `ProductCode` input
- a bound delimiter checkbox (`구분자체크`)
- a text-based log area
- an `ExcelData` in-memory container
- an `ExcelDataLoader` service that owns Excel workbook loading and sheet dispatch
- an `ExpressionCompiler` service that currently supports only minimal arithmetic parsing with Parlot

The app is still in the data-loading and parser-foundation stage. The legacy calculation pipeline has not been ported yet.

## Window Structure

`MainWindow.xaml` hosts a HandyControl `TabControl` with four tabs:

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

`MainPV` is the primary screen under active development.

## MainPV Screen

`Views/MainPVView.xaml` currently contains:

- Excel file path input and open button
- P/V/W file path inputs and open buttons
- an `Output` button bound to `LoadExcelCommand`
- a `상품코드` input bound TwoWay
- a `구분자` checkbox bound TwoWay to `구분자체크`
- placeholder company/options/radio/button UI
- a read-only multiline log `TextBox`

`Views/MainPVView.xaml.cs` still assigns `DataContext = new MainPVViewModel();` in code-behind.

## MainPVViewModel

`ViewModels/MainPVViewModel.cs` is now thinner than before.

### Observable fields

- `엑셀파일경로`
- `P파일경로`
- `V파일경로`
- `W파일경로`
- `로그텍스트`
- `상품코드`
- `구분자체크`

### Responsibilities

`MainPVViewModel` currently owns:

- UI-bound state
- file dialog commands
- `LoadExcelCommand`
- log accumulation through `AddLog(string message)`

### Current load flow

`LoadExcel()` no longer parses worksheets directly.

It now:

1. creates an `ExcelDataLoader`
2. passes `AddLog` into the service
3. calls `loader.LoadExcel(엑셀파일경로, 상품코드, 구분자체크)`
4. replaces `_excelData` only when the service returns non-null data

## ExcelDataLoader Service

`Services/ExcelDataLoader.cs` now owns workbook loading and sheet dispatch.

### Service input and state

The service currently receives:

- Excel file path
- product code
- delimiter-mode checkbox state
- an optional log callback (`Action<string>`)

It stores product code and delimiter mode in private fields during a load run.

### Workbook opening

`LoadExcel(...)` currently:

- validates that product code is not blank
- validates that Excel path is not blank
- validates that the file exists
- creates a fresh `ExcelData`
- opens the workbook with `Sylvan.Data.Excel`
- iterates worksheets
- dispatches each worksheet by name

### Current sheet dispatch targets

- `Layout`
- `Product`
- `Rider`
- `Rate`
- `Expense`
- `VarChg`
- `SInfo`
- `ChkExprs`

Unknown sheets are skipped.

### Implemented sheet loaders

#### `LoadLayoutSheet`

`Layout` loading is now partially implemented as real data population.

Current behavior:

- treats the first two rows as header rows
- reads three blocks from one row:
  - P block starting at column 0
  - V block starting at column 7
  - S block starting at column 14
- keeps only rows whose product code is one of:
  - `RiderCode`
  - `Check`
  - `Base`
  - current product code
- skips rows with blank `FactorName`
- if delimiter mode is enabled, skips rows with blank `Index`
- if delimiter mode is disabled, skips rows with blank `Start`
- converts `Start`, `Length`, and `Index` with `ToIntOrDefault(..., 0)`
- stores layouts into `_excelData.PLayout`, `_excelData.VLayout`, and `_excelData.SLayout`
- groups layouts by `상품코드` as `Dictionary<string, List<Layout>>`

This behavior intentionally mirrors the important filtering rules of legacy PVPlus layout loading.

#### `LoadProductSheet`

`Product` loading is now partially implemented.

Current behavior:

- scans the `Product` sheet row by row
- finds the first row whose first column matches the current product code
- reads:
  - `상품코드`
  - `판매시기`
  - `상품명`
  - `예정이율`
  - `평균공시이율`
  - `판매채널`
- stores one `Product` into `_excelData.Product`
- logs the loaded values
- logs an error if parsing fails
- logs a not-found message if no matching row exists

Current implementation expects numeric product cells to already be valid numeric Excel cells.

### Unimplemented sheet loaders

The following methods still contain only loop skeletons:

- `LoadRiderSheet`
- `LoadRateSheet`
- `LoadExpenseSheet`
- `LoadVarChgSheet`
- `LoadSInfoSheet`
- `LoadChkExprsSheet`

## Model Direction

The current model direction is to keep raw Excel text or simple scalar values first, then add compile/runtime layers later.

Examples:

- `Product` already stores scalar values as numeric types where simple
- `Rider` currently stores expression-related fields as raw `string`
- `RateKeyByRateVariable` is already modeled as a dictionary
- `ExcelData` is a public container that groups loaded data by domain

This is a deliberate separation from legacy PVPlus, where loading and expression compilation were tightly mixed.

## ExpressionCompiler

`Services/ExpressionCompiler.cs` is the first Parlot-based parser prototype.

### Current implemented behavior

It currently supports only:

- numeric literals
- parentheses
- binary `+`
- binary `-`
- binary `*`
- binary `/`

### Current design decisions

The current compiler design is intentionally simple:

- all numeric parsing is treated as `double`
- evaluation result type is `double`
- a static compiled parser is reused
- `Eof()` is applied so the full input must be consumed
- the compiler currently parses and evaluates immediately, not to a custom AST yet

This means an expression like `1 / 1000` is expected to produce `0.001` in the new design.

### Currently unimplemented operators

Flee-style operators not implemented yet include:

- `%`
- `^`
- `=`
- `<>`
- `<`
- `>`
- `<=`
- `>=`
- `And`
- `Or`
- `Xor`
- `Not`
- `<<`
- `>>`

Also not fully implemented yet:

- general unary minus such as `-(1+2)`
- unary plus

### Currently unimplemented functions

No function-call support exists yet.

That means the compiler does not yet support expressions such as:

- `If(...)`
- `Abs(...)`
- `Min(...)`
- `Max(...)`
- `Round(...)`
- `Floor(...)`
- `Ceiling(...)`
- `Pow(...)`
- `cast(...)`
- `in`
- project-specific helper functions

### Currently unimplemented variable and runtime binding features

The compiler does not yet support variable references.

Examples not yet supported:

- factor variables like `F1` to `F10`
- rate variables like `q1` to `q30`
- MP factors like `n`, `m`, `Age`, `Freq`, `Jong`, `ElapseYear`
- S factors like `S1` to `S10`
- check/result variables like `NP0`, `GP0`, `V0`, `W0`
- temporary variables such as `TempStr1`, `TempCK0`

Also not yet supported:

- array indexing such as `VWhole[0]`
- property/member access
- string expressions
- boolean expressions
- dynamic mixed-type expressions
- runtime delegate generation over a variable context

## Current Limitations

Current limitations include:

- only `Layout` and `Product` sheet loading are partially implemented
- rider/rate/expense/variable-change/check-expression sheets are not loaded yet
- expression parsing is still a minimal arithmetic prototype
- no variable-aware evaluation exists yet
- no Flee-compatible expression runtime exists yet
- no legacy calculation classes have been connected yet

## Next Direction

The natural next steps are:

1. implement `LoadRiderSheet` using the current raw-string model strategy
2. implement the remaining sheet loaders
3. extend `ExpressionCompiler` from arithmetic-only parsing to variables and functions
4. build a runtime evaluation layer over loaded rule data
5. connect compiled expressions to the future calculation pipeline
