# Legacy PVPlus Logic

## Overview

The legacy `reference_PVPlus` project is a Windows Forms application that validates or recalculates P/V/S table data by combining:

- an input Excel file path,
- a sibling `Data` folder that contains exported text files,
- a selected table file (`P`, `V`, or `S`),
- company-specific line adjustment rules,
- and a rule engine based on compiled expressions.

The old design is strongly stateful. Global static state is shared through `Configure`, `PV`, and `DataReader`.

## Main Runtime Flow

1. `UI/MainPVForm.cs` gathers user input.
2. `SetConfigure()` locates the `Data` folder next to the selected Excel file and stores values into `Configure`.
3. `PV.Run()` or another entry method calls `SetData()`.
4. `SetData()` creates or refreshes `DataReader`, then creates `RuleFinder`, resets helper state, and prepares summary collections.
5. The selected P/V/S table file is read line by line.
6. Each line is converted into `LineInfo`.
7. `LineInfo` resolves layouts, variables, rates, expenses, and S-related data.
8. A calculator (`PVCalculator` hierarchy) produces `PVResult`.
9. Output files such as normal/error/result files are written beside the source table file.

## Configuration State

`RULES/Configure.cs` stores process-wide state:

- `WorkingDI`: the `Data` directory
- `PVSTableInfo`: selected table file
- `TableType`: `P`, `V`, `SRatio`, or `StdAlpha`
- `CompanyRule`
- `ProductCode`
- separator mode and delimiter
- limit-check and line-summary flags

This makes the old code easy to wire together, but tightly couples every stage to global mutable state.

## Text Files Read From the Data Folder

`RULES/DataReader.cs` loads multiple tab-delimited text files from `Configure.WorkingDI`.

- `Product.txt`
  - Loads one `ProductRule` for the selected product code.
- `Rider.txt`
  - Loads `RiderRule` rows for the selected product code.
  - Also injects a synthetic default rider for `정기사망`.
- `Expense.txt`
  - Loads expense rules.
  - Splits comma-separated rider codes into multiple logical rules.
- `Rate.txt`
  - Loads all rate rows and their age-based arrays.
- `LayoutP.txt`, `LayoutV.txt`, `LayoutS.txt`
  - Loads the layout definition for the active table type.
- `VarChg.txt`
  - Loads variable override expressions for `Base`, product, or product+rider scope.
- `EvaluatedSInfo.txt`
  - If present, loads pre-evaluated S information for the selected product.
- `ChkExprs.txt`
  - Loads company-dependent check expressions.
- `Sinfo.txt`
  - Loaded only in the S-evaluation path, then transformed into `EvaluatedSInfo.txt`.

## Parsing and Expression Handling

The legacy loader uses a simple helper:

- `ToArrList(path)` reads the entire file into memory.
- Encoding is `Encoding.Default`.
- Every line is split by tab using `Split('\t')`.

Many columns are not stored as raw values. They are compiled into Flee expressions during load:

- integer expressions,
- double expressions,
- string expressions,
- boolean expressions,
- and dynamic expressions.

Compiled expressions are cached inside `DataReader`.

## Lookup Layer

`RULES/RuleFinder.cs` converts loaded lists into grouped lookup structures:

- rider rules by rider code,
- variable changes by `product|rider`,
- expense rules by `product|rider`,
- rate rules by rate name,
- S information by `MinSKey`,
- check expressions by check item.

This is the old project's main query layer between raw rule data and per-line calculation.

## Per-Line Processing

`RULES/LineInfo.cs` is the bridge between raw table text and calculator-ready variables.

For each input line it:

1. adjusts the raw line through `CompanyRule`,
2. parses by delimiter or fixed-width layout,
3. identifies rider code,
4. selects the applicable layouts,
5. copies parsed values into shared variables,
6. applies `VarChg` overrides,
7. resolves product/rider/rate/expense/S information,
8. then creates the calculator input.

## Output Behavior

The main execution path writes several result files, depending on options:

- normal rows,
- mismatch rows,
- mismatch source rows,
- error source rows,
- excess-limit rows,
- line summary files.

The S-evaluation path also writes `EvaluatedSInfo.txt`.

## Important Legacy Characteristics

- Heavy reliance on global static state
- Most reference data loaded into mutable lists first
- Rule lookup happens after loading
- Expression compilation is mixed into the loading layer
- Text parsing is permissive and tab-based
- File encoding depends on Windows default encoding

## Rewrite Implications for PVPlus2

The legacy project is useful as a source of business rules and file semantics, but not as a structure to copy directly.

For PVPlus2, the main rewrite opportunities are:

- replace global static state with instance-owned data,
- separate raw data loading from expression compilation,
- replace list scanning with dictionary-based indexing,
- make encoding, validation, and parsing more explicit,
- and keep the UI layer separate from calculation infrastructure.
