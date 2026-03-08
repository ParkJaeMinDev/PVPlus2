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

        using ExcelDataReader edr = ExcelDataReader.Create(엑셀파일경로, options);
        do
        {
            var sheetName = edr.WorksheetName;
            AddLog($"{sheetName}");
            while (edr.Read())
            {
                for (int i = 0; i < edr.RowFieldCount; i++)
                {
                    var value = edr.GetString(i);
                    AddLog($"{value}");
                }
            }
        } while (edr.NextResult());

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

    private void AddLog(string message)
    {
        if (!string.IsNullOrEmpty(로그텍스트))
        {
            로그텍스트 += Environment.NewLine;
        }

        로그텍스트 += $"[{DateTime.Now:HH:mm:ss.fffff}] {message}";
    }

}
