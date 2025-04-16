using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.ApplicationModel.Background;

public class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    private const string explorerPath = "C:\\Windows\\Explorer.EXE";
    private const string clockName = "Digital Clock";

    public static List<(string WindowTitle, string ExecutablePath, Icon Icon)> GetVisibleTaskbarWindows()
    {
        var visibleWindows = new List<(string, string, Icon)>();
        var allProcesses = Process.GetProcesses();

        foreach (var process in allProcesses)
        {
            try
            {
                IntPtr handle = process.MainWindowHandle;

                // Проверяем, что окно видимо и не минимизировано
                if (handle != IntPtr.Zero/* && IsWindowVisible(handle) && !IsIconic(handle)*/)
                {
                    // Получаем название окна
                    int length = GetWindowTextLength(handle);
                    var sb = new StringBuilder(length + 1);
                    GetWindowText(handle, sb, sb.Capacity);

                    // Извлекаем имя процесса и путь к исполняемому файлу
                    string windowTitle = sb.ToString();
                    string executablePath = process.MainModule.FileName;
                    
                    // Получаем иконку приложения
                    Icon icon = Icon.ExtractAssociatedIcon(executablePath);

                    // Фильтруем только приложения с определенными условиями
                    string shortTitle = GetShortTitle(windowTitle);
                    if(executablePath == explorerPath)
                    {
                        visibleWindows.Add(("Проводник", executablePath, icon));
                    }
                    if (IsValidWindow(shortTitle) && !IsAlreadyAdded(visibleWindows, executablePath) && executablePath != explorerPath)
                    {
                        if (windowTitle != clockName) 
                        { 
                            visibleWindows.Add((shortTitle, executablePath, icon));
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем процессы, к которым нет доступа
            }
        }

        return visibleWindows;
    }

    // Проверка на уже добавленные окна
    private static bool IsAlreadyAdded(List<(string, string, Icon)> visibleWindows, string executablePath)
    {
        foreach (var window in visibleWindows)
        {
            // Используем item.Item2 для доступа к ExecutablePath
            if (window.Item2.Equals(executablePath, StringComparison.OrdinalIgnoreCase))
            {
                return true; // Если путь совпадает, возвращаем true
            }
        }
        return false; // Если не нашли совпадения, возвращаем false
    }

    private static string GetShortTitle(string windowTitle)
    {
        var parts = windowTitle.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[parts.Length - 1].Trim() : windowTitle.Trim();
    }

    private static bool IsValidWindow(string windowTitle)
    {
        return !string.IsNullOrWhiteSpace(windowTitle) &&
               !windowTitle.Contains("Microsoft Text Input Application") &&
               !windowTitle.ToLower().Contains("параметры") &&
               !windowTitle.ToLower().Contains("калькулятор");
    }
}