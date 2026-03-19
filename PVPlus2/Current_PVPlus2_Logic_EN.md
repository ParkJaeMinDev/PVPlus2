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
- a Parlot AST parse step plus a binding step that builds `System.Linq.Expressions.Expression`
- `System.Linq.Expressions`-based delegate generation
- case-insensitive property/function lookup

## ExpressionContext

`Models/ExpressionContext.cs` is the current expression input model.

Current shape:

- public `double` properties from `a` through `z`
- `string` properties: `상품명`, `담보명` (Korean field names retained for now; rename planned in a future refactoring pass)

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

Currently registered functions:

- `Min(params double[])`, `Max(params double[])` — variable-argument, returns `double`
- `Abs(long)`, `Abs(double)`
- `Floor(long)`, `Floor(double)`
- `Ceiling(long)`, `Ceiling(double)`
- `Round(long)`, `Round(double)` — uses `MidpointRounding.AwayFromZero`
- `Round(long, long)`, `Round(double, long)` — with digits argument, also `AwayFromZero`
- `Pow(long, long)`, `Pow(double, double)`, `Pow(long, double)`, `Pow(double, long)`
- `Sqrt(long)`, `Sqrt(double)`
- `Truncate(long)`, `Truncate(double)`
- `Sign(long)`, `Sign(double)` — returns `long`
- `test(long)`, `test(double)` — testing only

Context-injected functions (first parameter is `ExpressionContext`, injected automatically by the binder):

- `ProductNameContains(ExpressionContext, string)` — checks if `상품명` contains the given substring (`StringComparison.Ordinal`)
- `RiderNameContains(ExpressionContext, string)` — checks if `담보명` contains the given substring (`StringComparison.Ordinal`)

Usage in expressions (no context argument written by the expression author):

- `ProductNameContains("종신")`
- `RiderNameContains("암")`

## ExpressionCompiler

`Services/ExpressionCompiler.cs` is the current parser/compiler entry point.

### Compile entry points

The service currently exposes:

- `CompileDouble(string text)` -> `Func<ExpressionContext, double>`
- `CompileLong(string text)` -> `Func<ExpressionContext, long>`
- `CompileBool(string text)` -> `Func<ExpressionContext, bool>`
- `CompileString(string text)` -> `Func<ExpressionContext, string>`

Current behavior:

- input text is trimmed
- the parser builds an `AstNode` tree first
- `BindSyntax(...)` converts the AST into a `System.Linq.Expressions.Expression`
- the final body is wrapped into a lambda with one `ExpressionContext context` parameter
- `CompileDouble` and `CompileLong` explicitly convert the final result to the requested return type
- `CompileBool` requires the final body type to already be `bool`
- `CompileString` requires the final body type to already be `string`

### Supported literals

Currently supported:

- integer literals -> parsed as `long`
  - examples: `1`, `2`, `100`
- decimal literals -> parsed as `double`
  - examples: `1.0`, `10.5`, `.5`
- string literals -> parsed into AST as `Parlot.TextSpan`, materialized to `string` during binding
  - examples: `"hello"`, `"A"`, `"literal with space"`
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
- `+` becomes string concatenation if either side is `string`
- string concatenation currently uses pairwise `string.Concat(object, object)`
- strings are only allowed in binary `+`

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
  - string vs string
- mixed string/non-string equality is rejected

Examples:

- `1 = 1`
- `1 == 1`
- `1 <> 2`
- `1 != 2`
- `x >= y`
- `"a" = "a"`
- `"a" <> "b"`

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

### Special built-in functions

The compiler now has a dedicated special-function binder layer separate from the reflection-based function path.

#### `if`

Syntax:

- `if(condition, true_value, false_value)`

Rules:

- exactly 3 arguments required
- `condition` must be `bool`
- `true_value` and `false_value` must be the same type, or both numeric (promoted to `double`)
- compiled to `Expression.Condition(...)` — only the selected branch is evaluated at runtime
- mixed-type branches such as `if(True, 1, "x")` are rejected at compile time

Examples:

- `if(x > 5, x * 2, 0.0)`
- `if(True, "A", "B")`
- `if(x > y, x + y, x - y)`

#### `ifs`

Syntax:

- `ifs(condition1, value1, condition2, value2, ..., conditionN, valueN, default_value)`

Rules:

- argument count must be odd and at least 3
- each `conditionN` must be `bool`
- all `valueN` and `default_value` must be the same type, or all numeric (promoted to `double`)
- compiled as nested `Expression.Condition(...)` from right to left — only the matching branch and its value are evaluated at runtime
- branches after the first match are never evaluated

Examples:

- `ifs(x > 10, "high", x > 5, "mid", "low")`
- `ifs(x > y + 100, x + y, x > y, x - y, y - x)`

#### `cast`

Syntax:

- `cast(value, type)`

Rules:

- exactly 2 arguments required
- second argument must be a type name identifier
- supported type aliases:
  - `int`, `int32` → `long`
  - `long`, `int64` → `long`
  - `double`, `float64` → `double`
  - `bool`, `boolean` → `bool`
  - `string` → `string`
- `float`, `single` are explicitly unsupported (compile error)
- numeric-to-numeric conversion uses `Expression.Convert`
- same-type cast is identity
- non-numeric cross-type cast is rejected at compile time
- `double` → `long` boundary behavior (NaN, Infinity) is delegated to CLR

Examples:

- `cast(1.9, int)` → `1L`
- `cast(x, double)` → same as `x` when `x` is already `double`

#### Function binder architecture

`CreateFunctionCallExpression` is now the single function binding entry point.

It:

1. calls `TryBindSpecialFunction(name, rawArguments, out expression)` first
2. if that returns `true`, returns the special expression directly (arguments were never pre-bound)
3. otherwise binds all arguments via `BindSyntax` and passes them to `CreateReflectionFunctionCallExpression`

This ensures special functions receive unbound `AstNode` arguments, which is required for short-circuit semantics.

### General function-call support

Current syntax:

- `name(arg1, arg2, ...)`
- minimum 1 argument required — `fn()` is rejected at parse time with `FormatException`

Current binding rules:

- function names are resolved from `ExpressionFunctions`
- public static methods only
- overload resolution score:
  - exact type match: 0
  - `long` → `double`: 1
  - `double` → `long`: 1
  - `IsAssignableFrom` / `object`: 2
  - `params` candidate penalty: +1
- all candidates are evaluated before ambiguous judgement — the loop does not throw early on a tie
- if the best score is still tied after all candidates are checked, the call is treated as ambiguous

### `params T[]` support

Methods with a `params T[]` last parameter are supported.

Rules:

- detected via `ParamArrayAttribute` on the last parameter
- `arguments.Count >= fixedParameterCount` is required
- fixed parameters are matched individually as normal
- trailing arguments are each converted to the element type and bundled into `Expression.NewArrayInit(...)`
- `params` candidates carry a `+1` score penalty so fixed-arity overloads always win when scores are equal

### Context-injected function support

Methods whose first parameter is `ExpressionContext` are automatically detected by the binder and treated as context-injected functions.

Rules:

- detection condition: `parameters[0].ParameterType == typeof(ExpressionContext)`
- the binder injects `_contextParameter` at index 0 automatically
- expression authors omit the context argument entirely
- effective argument count for matching: `parameters.Length - 1`
- no score penalty — injection is an identity connection, not a conversion
- `context + params` combination is not supported in this release (skipped by the binder)
- the three matching paths (`isContextInjected`, `isParams`, fixed-arity) are dispatched through `TryConvertFunctionCallArguments`

Unsupported function forms (binder will fail to match):

- `ExpressionContext` not in the first parameter position
- optional parameters
- generic methods
- `ref` / `out` / `in` parameters
- context-injected + `params` combination

Element type guidelines for `ExpressionFunctions` authors:

| Element type | Intended use |
|---|---|
| `double[]` | numeric calculation functions (default choice) |
| `long[]` | integer-only functions where `long` return is required |
| `object[]` | heterogeneous-argument functions only — avoid for numeric functions |

### Important current runtime behavior

The current implementation has a few important edge behaviors:

- `1 / 0` does not throw in `CompileDouble`
  - it becomes `double` division
  - current result is positive infinity
- `0 / 0` produces `NaN`
- `1 % 0` can still throw `DivideByZeroException`
  - because integer remainder can stay in the `long` pipeline
- `--1` currently parses successfully
- unary plus is currently a pass-through operator for non-string operands
  - this means `+True` currently compiles successfully
  - `+"a"` is rejected
- chained comparisons such as `1 < 2 < 3` are not supported as a valid boolean chain
  - the compiler reaches a type mismatch on the second comparison
- string relational comparisons such as `"a" > "b"` are not supported
- mixed string/non-string equality such as `"a" == 1` is rejected

### Current limitations

Still not implemented:

- scientific-notation numbers
- numeric group separators
- percent literals
- string relational operators
- general string function support beyond literal/concat/equality
- domain-aware business variable binding beyond `ExpressionContext`
- array indexing in the general expression language
- legacy Flee-compatible full feature parity
- `fn()` zero-argument call syntax (parse-time `FormatException` by current policy)

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

`ViewModels/TestViewModel.cs` is now a typed regression and microbenchmark harness.

Current `TotalTest()` behavior:

- clears `OutputText`
- validates `ArrayLength > 0`
- builds eight expression groups:
  - valid double expressions
  - invalid double expressions
  - valid long expressions
  - invalid long expressions
  - valid bool expressions
  - invalid bool expressions
  - valid string expressions
  - invalid string expressions
- rebuilds `InputText` with all expression groups
- benchmarks compile time for each valid expression with:
  - `CompileDouble`
  - `CompileLong`
  - `CompileBool`
  - `CompileString`
- stores the last compiled delegate for each expression in typed caches
- generates deterministic random `xValues` and `yValues`
- benchmarks compiled delegate evaluation for all four result types
- benchmarks matching native C# delegates for all four result types
- validates expected-failure expressions by:
  - compiling them
  - invoking them once
  - treating thrown exceptions as success

### Current output sections

`TotalTest()` currently prints:

- a typed benchmark summary header
- compile time tables for:
  - double
  - long
  - bool
  - string
- evaluation time tables for:
  - double
  - long
  - bool
  - string
- checksum/hash comparison tables for:
  - double
  - long
  - bool
  - string
- invalid expression validation tables for:
  - double
  - long
  - bool
  - string

### Current checksum policy

Double validation currently compares:

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

Long validation compares:

- compiler checksum
- native checksum
- absolute difference
- match flag

String validation compares:

- compiler FNV-1a hash checksum
- native FNV-1a hash checksum
- exact mismatch count across repeated runs
- match flag

### Current test intent

The current test screen is not only a microbenchmark harness.

It now also acts as:

- a parser regression check
- a semantic parity check against native C#
- an expected-error verification harness
- a typed compile/evaluate benchmark for `double`, `long`, `bool`, and `string`

## Current Limitations

Current limitations include:

- only `Layout` and `Product` sheet loading are partially implemented
- rider/rate/expense/variable-change/check-expression sheets are not loaded yet
- the expression runtime is still isolated from real business rule execution
- `ExpressionContext` is still a fixed testing-oriented property bag
- function support exists structurally, but only trivial test functions are currently registered
- string support is still intentionally narrow:
  - string literals
  - string `+` concatenation
  - string equality/inequality only when both sides are string
- no domain-object expression model exists yet
- no legacy calculation classes have been connected yet

## Next Direction

The natural next steps are:

1. implement the remaining Excel sheet loaders
2. decide the final shape of business variable binding beyond `ExpressionContext`
3. decide how far string support should go beyond the current concat/equality subset
4. add business-focused expression tests instead of only edge-case/runtime tests
5. connect loaded rule data to a runtime evaluation layer
6. connect the future calculation pipeline

Note: `ExpressionContext` is planned to be renamed to `CommutationTable` in a future refactoring pass. Field names `상품명` and `담보명` may also be renamed to English at that time.
