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
    private string _상품코드 = string.Empty;

    [ObservableProperty]
    private bool _구분자체크;

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

        if (string.IsNullOrWhiteSpace(상품코드))
        {
            AddLog("상품코드가 비어 있습니다.");
            return;
        }

        _excelData = new ExcelData();
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

        const int P인덱스 = 0;
        const int V인덱스 = 7;
        const int S인덱스 = 14;

        var layoutBlocks = new (string TableName, int BaseIndex, Dictionary<string, List<Layout>> Target)[]
        {
            ("P", P인덱스, _excelData.PLayout),
            ("V", V인덱스, _excelData.VLayout),
            ("S", S인덱스, _excelData.SLayout),
        };

        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;
            if(rowIndex <= 2)
            {
                continue; // 헤더 행 건너뛰기
            }

            foreach (var block in layoutBlocks)
            {
                try
                {
                    string 상품코드값 = edr.GetString(block.BaseIndex);
                    string 담보코드값 = edr.GetString(block.BaseIndex + 1);
                    string startText = edr.GetString(block.BaseIndex + 2);
                    string lengthText = edr.GetString(block.BaseIndex + 3);
                    string indexText = edr.GetString(block.BaseIndex + 4);
                    string factorName값 = edr.GetString(block.BaseIndex + 5);

                    bool 상품코드일치 = 상품코드값 == "RiderCode" || 상품코드값 == "Check" || 상품코드값 == "Base" || 상품코드값 == 상품코드;

                    if (!상품코드일치)
                    {
                        continue;
                    }

                    // factorName값이 비어 있으면 해당 행을 건너뜁니다.
                    if (string.IsNullOrWhiteSpace(factorName값))
                    {
                        continue;
                    }

                    // 구분자를 체크하는 경우, indexText가 비어 있으면 해당 행을 건너뜁니다.
                    if (구분자체크 && string.IsNullOrWhiteSpace(indexText))
                    {
                        continue;
                    }

                    // 구분자를 체크하지 않는 경우, startText가 비어 있으면 해당 행을 건너뜁니다.
                    if (!구분자체크 && string.IsNullOrWhiteSpace(startText))
                    {
                        continue;
                    }

                    int startValue = ToIntOrDefault(startText, 0);
                    int lengthValue = ToIntOrDefault(lengthText, 0);
                    int indexValue = ToIntOrDefault(indexText, 0);

                    Layout layout = new Layout
                    {
                        상품코드 = 상품코드값,
                        담보코드 = 담보코드값,
                        Start = startValue,
                        Length = lengthValue,
                        Index = indexValue,
                        FactorName = factorName값
                    };

                    if (!block.Target.TryGetValue(layout.상품코드, out List<Layout>? list))
                    {
                        list = new List<Layout>();
                        block.Target[layout.상품코드] = list;
                    }

                    list.Add(layout);

                    AddLog(
                        $"{block.TableName} 상품코드={layout.상품코드}, 담보코드={layout.담보코드}, " +
                        $"Start={layout.Start}, Length={layout.Length}, Index={layout.Index}, FactorName={layout.FactorName}");

                }
                catch (Exception ex)
                {
                    AddLog($"Layout 시트의 행 {rowIndex} {block.TableName} 블록 처리 중 오류가 발생했습니다. 오류={ex.Message}");
                }
            }
        }

        AddLog("Layout 시트 로드 완료");

    }

    private void LoadProductSheet(ExcelDataReader edr)
    {
        int rowIndex = 0;

        while (edr.Read())
        {
            rowIndex++;

            try
            {
                string 상품코드값 = edr.GetString(0);

                if (상품코드값 != 상품코드)
                {
                    continue;
                }

                string 판매시기값 = edr.GetString(1);
                string 상품명값 = edr.GetString(2);
                double 예정이율 = edr.GetDouble(3);
                double 평균공시이율 = edr.GetDouble(4);
                int 판매채널 = edr.GetInt32(5);

                Product product = new Product
                {
                    상품코드 = 상품코드값,
                    판매시기 = 판매시기값,
                    상품명 = 상품명값,
                    예정이율 = 예정이율,
                    평균공시이율 = 평균공시이율,
                    판매채널 = 판매채널
                };

                _excelData.Product[product.상품코드] = product;

                AddLog(
                    $"Product 상품코드={상품코드값}, 판매시기={판매시기값}, 상품명={상품명값}, " +
                    $"예정이율={예정이율}, 평균공시이율={평균공시이율}, 판매채널={판매채널}");
                return;
            }
            catch (Exception ex)
            {
                AddLog($"Product 시트의 행 {rowIndex}을 읽는 중 오류가 발생했습니다. 오류={ex.Message}");
                return;
            }
        }

        AddLog($"Product 시트에서 상품코드({상품코드})를 찾을 수 없습니다.");

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

    public int ToIntOrDefault(string s, int defaultVal)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return defaultVal;
        }

        return int.TryParse(s, out int val) ? val : defaultVal;
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
