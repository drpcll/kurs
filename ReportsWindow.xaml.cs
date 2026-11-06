using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Отчёты о посещаемости и продажах с экспортом в CSV.</summary>
    public partial class ReportsWindow : Window
    {
        private List<AttendanceRow> _attendance = new List<AttendanceRow>();
        private List<SalesRow> _sales = new List<SalesRow>();

        public ReportsWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            FromBox.Text = Fmt.Date(DateTime.Today.AddDays(-30));
            ToBox.Text = Fmt.Date(DateTime.Today.AddDays(30));
            TypeCombo.SelectedIndex = 0;
            Loaded += (s, e) => Build();
        }

        private bool IsAttendance
        {
            get
            {
                var item = TypeCombo.SelectedItem as ComboBoxItem;
                return item == null || item.Content == null ||
                       item.Content.ToString().StartsWith("Посещаемость");
            }
        }

        private void Build()
        {
            DateTime from;
            DateTime to;
            if (!Ui.ParseDate(FromBox.Text, out from))
            {
                Ui.Error("Дата начала указана неверно. Формат: дд.мм.гггг");
                return;
            }
            if (!Ui.ParseDate(ToBox.Text, out to))
            {
                Ui.Error("Дата окончания указана неверно. Формат: дд.мм.гггг");
                return;
            }

            SummaryPeriod.Text = Fmt.Date(from) + " — " + Fmt.Date(to);

            if (IsAttendance)
            {
                _attendance = Store.AttendanceReport(from, to);
                AttendanceGrid.ItemsSource = _attendance;
                AttendanceGrid.Visibility = Visibility.Visible;
                SalesGrid.Visibility = Visibility.Collapsed;

                SummaryLabel2.Text = "Записано / пришло";
                SummaryValue2.Text = _attendance.Sum(r => r.Booked) + " / " + _attendance.Sum(r => r.Came);
                SummaryLabel3.Text = "Занятий";
                SummaryValue3.Text = _attendance.Count.ToString();
                FooterText.Text = "Отчёт о посещаемости за период";
            }
            else
            {
                _sales = Store.SalesReport(from, to);
                SalesGrid.ItemsSource = _sales;
                SalesGrid.Visibility = Visibility.Visible;
                AttendanceGrid.Visibility = Visibility.Collapsed;

                SummaryLabel2.Text = "Продано абонементов";
                SummaryValue2.Text = _sales.Count.ToString();
                SummaryLabel3.Text = "Выручка";
                SummaryValue3.Text = Fmt.Money(Store.SalesTotal(from, to));
                FooterText.Text = "Отчёт о продажах за период";
            }
        }

        private void Build_Click(object sender, RoutedEventArgs e)
        {
            Build();
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var lines = new List<string>();
            string name;

            if (IsAttendance)
            {
                name = "Отчёт_посещаемость_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
                lines.Add("Дата;Время;Занятие;Тренер;Зал;Записано;Пришёл;Не пришёл");
                foreach (var row in _attendance)
                {
                    lines.Add(string.Join(";", new[]
                    {
                        Ui.Csv(row.DateText), Ui.Csv(row.TimeText), Ui.Csv(row.Title),
                        Ui.Csv(row.Trainer), Ui.Csv(row.Hall),
                        row.Booked.ToString(), row.Came.ToString(), row.Missed.ToString()
                    }));
                }
            }
            else
            {
                name = "Отчёт_продажи_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
                lines.Add("Дата;Клиент;Тип абонемента;Сумма");
                foreach (var row in _sales)
                {
                    lines.Add(string.Join(";", new[]
                    {
                        Ui.Csv(row.DateText), Ui.Csv(row.Client),
                        Ui.Csv(row.Type), Ui.Csv(row.AmountText)
                    }));
                }
            }

            Ui.SaveCsv(name, lines);
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
