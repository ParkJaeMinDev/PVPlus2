using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

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
}
