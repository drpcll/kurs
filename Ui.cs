using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace FitnessClub
{
    /// <summary>Общие операции интерфейса: сообщения, навигация, разбор дат, экспорт.</summary>
    public static class Ui
    {
        private const string Caption = "Фитнес-клуб";

        public static void Info(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void Error(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void Warn(string text)
        {
            MessageBox.Show(text, Caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static bool Confirm(string text)
        {
            return MessageBox.Show(text, Caption, MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool ParseDate(string text, out DateTime date)
        {
            var formats = new[] { "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yy", "yyyy-MM-dd", "dd/MM/yyyy" };
            return DateTime.TryParseExact((text ?? string.Empty).Trim(), formats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }

        public static void Logout()
        {
            Store.Logout();
            var login = new LoginWindow();
            login.Show();
            foreach (var window in Application.Current.Windows.Cast<Window>().ToList())
            {
                if (window != login)
                {
                    window.Close();
                }
            }
        }

        public static void Close(Window window)
        {
            if (window.Owner != null)
            {
                window.Close();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        public static string Csv(string value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        public static bool SaveCsv(string defaultName, IEnumerable<string> lines)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Экспорт отчёта",
                Filter = "CSV-файл (*.csv)|*.csv|Все файлы (*.*)|*.*",
                FileName = defaultName,
                DefaultExt = ".csv"
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            try
            {
                File.WriteAllLines(dialog.FileName, lines, new UTF8Encoding(true));
                Info("Отчёт сохранён:\n" + dialog.FileName);
                return true;
            }
            catch (Exception ex)
            {
                Error("Не удалось сохранить файл: " + ex.Message);
                return false;
            }
        }
    }
}
