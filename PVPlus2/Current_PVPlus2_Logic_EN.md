# Current PVPlus2 Logic

## Status

`PVPlus2` is still an early-stage WPF rewrite of the legacy PVPlus tool.

The project currently has:

- a HandyControl tabbed main window
- a `MainPV` screen bound to `MainPVViewModel`
- a `TabTest` screen backed by `TestView` and `TestViewModel`
- file-picking commands for Excel, P, V, and W files
- a bound `ProductCode` input
- a bound delimiter checkbox (`구분자체크`)
- text-based log areas
- an `ExcelData` in-memory container
- an `ExcelDataLoader` service that owns Excel workbook loading and sheet dispatch
- an `ExpressionCompiler` service that builds compiled delegates with `System.Linq.Expressions`

The app is still in the data-loading and expression-runtime foundation stage. The legacy business calculation pipeline has not been ported yet.

## Window Structure

`MainWindow.xaml` hosts a HandyControl `TabControl` with four tabs:

- `MainPV`
- `Sample`
- `LTFHelper`
- `TabTest`

`MainPV` is the primary business screen. `TabTest` is the current sandbox for parser, runtime, and correctness experiments.

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

`ViewModels/MainPVViewModel.cs` is thinner than before.

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

`Services/ExcelDataLoader.cs` owns workbook loading and sheet dispatch.

### Service input and state

The service currently receives:

- Excel file path
- product code
- delimiter-mode checkbox state
- an optional log callback (`Action<string>`)

It stores product code and delimiter mode in private fields during one load run.

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

`Layout` loading is partially implemented as real data population.

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
- groups layouts by product code as `Dictionary<string, List<Layout>>`

This intentionally mirrors the core filtering rules of legacy PVPlus layout loading.

#### `LoadProductSheet`

`Product` loading is also partially implemented.

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

## Expression Runtime Model

The current expression runtime is no longer the old `x, y`-only prototype.

The runtime is now built around:

- a fixed `ExpressionContext` model
- a static `ExpressionCompiler`
- `System.Linq.Expressions`-based delegate generation
- case-insensitive property/function lookup

## ExpressionContext

`Models/ExpressionContext.cs` is the current expression input model.

Current shape:

- public `double` properties from `a` through `z`

Important current meaning:

- expressions are compiled against `ExpressionContext`
- identifiers are resolved to public instance properties on that type
- property lookup is case-insensitive

Examples:

- `x + y`
- `X + Y`
- `a * 3`

All of the above resolve against `ExpressionContext`.

## ExpressionFunctions

`Services/ExpressionFunctions.cs` is the current static function container.

Current state:

- the class is `static`
- methods are discovered via reflection at compiler startup
- function lookup is case-insensitive
- current test methods are:
  - `test(long a)`
  - `test(double a)`

Function-call parsing already exists, but the benchmark harness is not using custom-function expressions yet.

## ExpressionCompiler

`Services/ExpressionCompiler.cs` is the current parser/compiler entry point.

### Compile entry points

The service currently exposes:

- `CompileDouble(string text)` -> `Func<ExpressionContext, double>`
- `CompileLong(string text)` -> `Func<ExpressionContext, long>`
- `CompileBool(string text)` -> `Func<ExpressionContext, bool>`

Current behavior:

- input text is trimmed
- the parser builds a `System.Linq.Expressions.Expression`
- the final body is wrapped into a lambda with one `ExpressionContext context` parameter
- `CompileDouble` and `CompileLong` explicitly convert the final result to the requested return type
- `CompileBool` requires the final body type to already be `bool`

### Supported literals

Currently supported:

- integer literals -> parsed as `long`
  - examples: `1`, `2`, `100`
- decimal literals -> parsed as `double`
  - examples: `1.0`, `10.5`, `.5`
- boolean literals
  - `True`
  - `False`
  - case-insensitive

Currently not supported:

- scientific notation
  - `1e10`
  - `1e-5`
- numeric group separators
  - `1,000`
- string literals
- percent literals such as `2.75%`

`%` is currently treated only as the modulo operator.

### Supported arithmetic operators

Currently supported:

- unary `+`
- unary `-`
- binary `+`
- binary `-`
- binary `*`
- binary `/`
- binary `%`
- binary `^`

Important current rules:

- `^` is implemented with `Expression.Power(...)`
- `^` is right-associative
  - `2 ^ 3 ^ 2` means `2 ^ (3 ^ 2)`
- `%` uses numeric remainder semantics
- `/` always promotes operands to `double`

Current numeric promotion rules:

- `long op long` stays `long` for `+`, `-`, `*`, `%`
- mixed `long`/`double` is promoted to `double`
- `/` converts both sides to `double`
- `^` converts both sides to `double`

### Supported comparison operators

Currently supported:

- `=`
- `==`
- `!=`
- `<>`
- `>`
- `>=`
- `<`
- `<=`

Current meaning:

- `=` and `==` both mean equality
- `!=` and `<>` both mean inequality
- relational operators (`>`, `>=`, `<`, `<=`) are numeric-only
- equality/inequality support:
  - numeric vs numeric
  - bool vs bool

Examples:

- `1 = 1`
- `1 == 1`
- `1 <> 2`
- `1 != 2`
- `x >= y`

### Supported logical operators

Currently supported:

- `NOT`
- `AND`
- `OR`

All three are case-insensitive.

Examples:

- `NOT (1 == 2)`
- `TRUE AND NOT FALSE`
- `x > y OR y > x`

Current meaning:

- `NOT` requires a bool operand
- `AND` and `OR` require bool operands
- the compiler emits `Expression.Not`, `Expression.AndAlso`, and `Expression.OrElse`

### Parser precedence

Current precedence order is:

1. primary
   - literals
   - identifiers
   - function calls
   - parenthesized expressions
2. unary
   - `+`
   - `-`
3. power
   - `^` (right-associative)
4. multiplicative
   - `*`
   - `/`
   - `%`
5. additive
   - `+`
   - `-`
6. relational
   - `>`
   - `>=`
   - `<`
   - `<=`
7. equality
   - `=`
   - `==`
   - `!=`
   - `<>`
8. logical not
   - `NOT`
9. logical and
   - `AND`
10. logical or
   - `OR`

### Case sensitivity

The current compiler is intentionally case-insensitive for the user-facing expression surface wherever reasonable.

Current case-insensitive behavior:

- property lookup on `ExpressionContext`
- function lookup in `ExpressionFunctions`
- boolean keywords `True`, `False`
- logical keywords `AND`, `OR`, `NOT`

Examples:

- `X + y`
- `true or FALSE`
- `TeSt(1)` once a function is actually used in a test expression

The symbolic operators remain symbolic and are not affected by case.

### Function-call support

Current syntax:

- `name(arg1, arg2, ...)`

Current binding rules:

- function names are resolved from `ExpressionFunctions`
- public static methods only
- overload resolution is based on:
  - exact type match first
  - then `long -> double`
  - then `double -> long`
- if more than one overload has the same score, the call is treated as ambiguous

### Important current runtime behavior

The current implementation has a few important edge behaviors:

- `1 / 0` does not throw in `CompileDouble`
  - it becomes `double` division
  - current result is positive infinity
- `0 / 0` produces `NaN`
- `1 % 0` can still throw `DivideByZeroException`
  - because integer remainder can stay in the `long` pipeline
- `--1` currently parses successfully
- unary plus is currently a pass-through operator
  - this means `+True` currently compiles successfully
- chained comparisons such as `1 < 2 < 3` are not supported as a valid boolean chain
  - the compiler reaches a type mismatch on the second comparison

### Current limitations

Still not implemented:

- string expressions
- string comparison
- scientific-notation numbers
- numeric group separators
- percent literals
- ternary syntax
- dedicated `If(...)` support
- general function libraries beyond the current test methods
- domain-aware business variable binding beyond `ExpressionContext`
- array indexing in the general expression language
- legacy Flee-compatible full feature parity

## TestView and Test Harness

`Views/TestView.xaml` is the current parser/runtime test screen used from the `TabTest` tab.

### Current UI

The screen currently contains:

- a `Parlot` button bound to `RunTestParlotCommand`
- a `TotalTest` button bound to `TotalTestCommand`
- an `Array Length` input bound to `ArrayLength`
- a read-only multiline `InputText` box
- a read-only multiline `OutputText` log box

`Views/TestView.xaml.cs` assigns `DataContext = new TestViewModel();` in code-behind.

### Current benchmark/test flow

`ViewModels/TestViewModel.cs` is no longer measuring compile cost.

Current `TotalTest()` behavior:

- clears `OutputText`
- validates `ArrayLength > 0`
- builds four expression groups:
  - valid numeric expressions
  - invalid numeric expressions
  - valid bool expressions
  - invalid bool expressions
- compiles valid numeric expressions once with `CompileDouble`
- compiles valid bool expressions once with `CompileBool`
- generates random `xValues` and `yValues`
- evaluates compiled numeric expressions repeatedly
- evaluates matching native C# numeric lambdas repeatedly
- evaluates compiled bool expressions repeatedly
- evaluates matching native C# bool lambdas repeatedly
- validates expected-failure expressions by:
  - compiling them
  - invoking them once
  - treating thrown exceptions as success

### Current output sections

`TotalTest()` currently prints:

- numeric evaluation time table
- numeric checksum comparison table
- bool evaluation time table
- bool true-count comparison table
- invalid numeric expression validation table
- invalid bool expression validation table

### Current checksum policy

Numeric validation currently compares:

- compiler checksum
- native checksum
- absolute difference
- match flag

Special cases are treated explicitly:

- `NaN` vs `NaN` is considered a match
- `+Infinity` vs `+Infinity` is considered a match
- `-Infinity` vs `-Infinity` is considered a match

Bool validation compares:

- the total count of `true` results across repeated runs

### Current test intent

The current test screen is not only a microbenchmark harness.

It now also acts as:

- a parser regression check
- a semantic parity check against native C#
- an expected-error verification harness

## Current Limitations

Current limitations include:

- only `Layout` and `Product` sheet loading are partially implemented
- rider/rate/expense/variable-change/check-expression sheets are not loaded yet
- the expression runtime is still isolated from real business rule execution
- `ExpressionContext` is still a fixed testing-oriented property bag
- function support exists structurally, but only trivial test functions are currently registered
- no string or domain-object expression model exists yet
- no legacy calculation classes have been connected yet

## Next Direction

The natural next steps are:

1. implement the remaining Excel sheet loaders
2. decide the final shape of business variable binding beyond `ExpressionContext`
3. expand the registered function set in `ExpressionFunctions`
4. add business-focused expression tests instead of only edge-case/runtime tests
5. connect loaded rule data to a runtime evaluation layer
6. connect the future calculation pipeline
