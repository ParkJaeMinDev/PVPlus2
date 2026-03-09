using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PVPlus2.Models;
using System.IO;
using Sylvan.Data.Excel;

namespace PVPlus2.ViewModels;

public partial class MainPVViewModel : ObservableObject
{
    [ObservableProperty]
    private string _엑셀파일경로 = string.Empty;

    [ObservableProperty]
    private string _P파일경로 = string.Empty;

    [ObservableProperty]
    private string _V파일경로 = string.Empty;

    [ObservableProperty]
    private string _W파일경로 = string.Empty;

    [ObservableProperty]
    private string _로그텍스트 = string.Empty;

    [ObservableProperty]
    private string productCode = string.Empty;

    private ExcelData _excelData = new();

    [RelayCommand]
    private void OpenFile(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Title = GetDialogTitle(target),
            Filter = GetDialogFilter(target),
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        switch (target)
        {
            case "Excel":
                엑셀파일경로 = dialog.FileName;
                break;
            case "P":
                P파일경로 = dialog.FileName;
                break;
            case "V":
                V파일경로 = dialog.FileName;
                break;
            case "W":
                W파일경로 = dialog.FileName;
                break;
        }

    }

    private static string GetDialogTitle(string target)
    {
        return target switch
        {
            "Excel" => "Excel 파일 선택",
            "P" => "P 파일 선택",
            "V" => "V 파일 선택",
            "W" => "W 파일 선택",
            _ => "파일 선택"
        };
    }

    private static string GetDialogFilter(string target)
    {
        return target == "Excel"
            ? "Excel Files (*.xlsx;*.xls;*.xlsm;*.xlsb)|*.xlsx;*.xls;*.xlsm;*.xlsb|All Files (*.*)|*.*"
            : "All Files (*.*)|*.*";
    }

    [RelayCommand]
    private void LoadExcel()
    {
        AddLog("엑셀 파일 로드 시작");

        if (string.IsNullOrWhiteSpace(엑셀파일경로))
        {
            AddLog("엑셀 파일 경로가 비어 있습니다.");
            return;
        }

        if (!File.Exists(엑셀파일경로))
        {
            AddLog($"엑셀 파일이 존재하지 않습니다. 경로: {엑셀파일경로}");
            return;
        }

        var options = new ExcelDataReaderOptions
        {
            Schema = ExcelSchema.NoHeaders
        };

        FileStream stream;
        ExcelDataReader edr;

        try
        {
            ExcelWorkbookType workbookType = ExcelDataReader.GetWorkbookType(엑셀파일경로);
            stream = new FileStream(
                엑셀파일경로,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite);
            edr = ExcelDataReader.Create(stream, workbookType, options);
        }
        catch(ArgumentException ex)
        {
            AddLog($"엑셀 파일을 여는 중 오류가 발생했습니다: 지원되지 않는 파일 형식입니다. 오류={ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            AddLog($"엑셀 파일을 여는 중 오류가 발생했습니다. 오류={ex.Message}");
            return;
        }

        using (stream)
        using (edr)
        do
        {
            var sheetName = edr.WorksheetName;
            if(string.IsNullOrWhiteSpace(sheetName))
            {
                AddLog("오류: 현재 시트 이름이 null 또는 빈 문자열입니다. Excel 읽기를 중단합니다.");
                continue;
            }
            AddLog($"시트 시작: {sheetName}");
            DispatchSheetLoad(sheetName, edr);
        } while (edr.NextResult());

    }

    private void DispatchSheetLoad(string sheetName, ExcelDataReader edr)
    {
        switch (sheetName)
        {
            case "Layout":
                LoadLayoutSheet(edr);
                break;
            case "Product":
                LoadProductSheet(edr);
                break;
            case "Rider":
                LoadRiderSheet(edr);
                break;
            case "Rate":
                LoadRateSheet(edr);
                break;
            case "Expense":
                LoadExpenseSheet(edr);
                break;
            case "VarChg":
                LoadVarChgSheet(edr);
                break;
            case "SInfo":
                LoadSInfoSheet(edr);
                break;
            case "ChkExprs":
                LoadChkExprsSheet(edr);
                break;
            default:
                break;
        }
    }

    private void LoadLayoutSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;
            if(rowIndex <= 2)
            {
                continue; // 헤더 행 건너뛰기
            }
            try
            {
                var P인덱스 = 0;
                var P테이블상품코드 = edr.GetString(P인덱스);
                var P테이블담보코드 = edr.GetString(P인덱스 + 1);
                var P테이블Start = edr.GetInt32(P인덱스 + 2);
                var P테이블Length = edr.GetInt32(P인덱스 + 3);
                var P테이블Index = edr.GetInt32(P인덱스 + 4);
                var P테이블FactorName = edr.GetString(P인덱스 + 5);

                var V인덱스 = 7;
                var V테이블상품코드 = edr.GetString(V인덱스);
                var V테이블담보코드 = edr.GetString(V인덱스 + 1);
                var V테이블Start = edr.GetInt32(V인덱스 + 2);
                var V테이블Length = edr.GetInt32(V인덱스 + 3);
                var V테이블Index = edr.GetInt32(V인덱스 + 4);
                var V테이블FactorName = edr.GetString(V인덱스 + 5);

                var S인덱스 = 7;
                var S테이블상품코드 = edr.GetString(S인덱스);
                var S테이블담보코드 = edr.GetString(S인덱스 + 1);
                var S테이블Start = edr.GetInt32(S인덱스 + 2);
                var S테이블Length = edr.GetInt32(S인덱스 + 3);
                var S테이블Index = edr.GetInt32(S인덱스 + 4);
                var S테이블FactorName = edr.GetString(S인덱스 + 5);

                AddLog(
                    $"P 상품코드={P테이블상품코드}, 담보코드={P테이블담보코드}, Start={P테이블Start}, " +
                    $"Length={P테이블Length}, Index={P테이블Index}, FactorName={P테이블FactorName}"
                );

                AddLog(
                    $"V 상품코드={V테이블상품코드}, 담보코드={V테이블담보코드}, Start={V테이블Start}, " +
                    $"Length={V테이블Length}, Index={V테이블Index}, FactorName={V테이블FactorName}"
                );

                AddLog(
                    $"S 상품코드={S테이블상품코드}, 담보코드={S테이블담보코드}, Start={S테이블Start}, " +
                    $"Length={S테이블Length}, Index={S테이블Index}, FactorName={S테이블FactorName}"
                );
            }
            catch (Exception ex)
            {
                AddLog($"Layout 시트의 행 {rowIndex}을 읽는 중 오류가 발생했습니다. 오류={ex.Message}");
            }



        }
    }

    private void LoadProductSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadRiderSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadRateSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadExpenseSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadVarChgSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadSInfoSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void LoadChkExprsSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            for (int i = 0; i < edr.RowFieldCount; i++)
            {
                
            }
        }
    }

    private void AddLog(string message)
    {
        if (!string.IsNullOrEmpty(로그텍스트))
        {
            로그텍스트 += Environment.NewLine;
        }

        로그텍스트 += $"[{DateTime.Now:HH:mm:ss.fffff}] {message}";
    }

}
