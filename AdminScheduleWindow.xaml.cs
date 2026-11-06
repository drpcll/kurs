using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Управление расписанием и отметками о посещении.</summary>
    public partial class AdminScheduleWindow : Window
    {
        private int _selectedLessonId;

        public AdminScheduleWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            FromBox.Text = Fmt.Date(DateTime.Today);
            ToBox.Text = Fmt.Date(DateTime.Today.AddDays(14));

            var halls = new List<Zal> { new Zal { Id = 0, Nazvanie = "Все залы" } };
            halls.AddRange(Store.Zaly);
            HallCombo.ItemsSource = halls;
            HallCombo.SelectedValue = 0;

            Loaded += (s, e) => Reload();
        }

        private void Reload()
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

            var hallId = HallCombo.SelectedValue == null ? 0 : Convert.ToInt32(HallCombo.SelectedValue);
            var rows = Store.GetAdminSchedule(from, to, hallId == 0 ? (int?)null : hallId);
            LessonsGrid.ItemsSource = rows;
            FooterText.Text = "Занятий: " + rows.Count + " · Период: " + Fmt.Date(from) +
                              " — " + Fmt.Date(to);

            if (_selectedLessonId != 0)
            {
                foreach (var row in rows)
                {
                    if (row.Id == _selectedLessonId)
                    {
                        LessonsGrid.SelectedItem = row;
                        break;
                    }
                }
            }

            LoadBookings();
        }

        private void LoadBookings()
        {
            var row = LessonsGrid.SelectedItem as LessonRow;
            if (row == null)
            {
                _selectedLessonId = 0;
                BookingsGrid.ItemsSource = null;
                BookingsTitle.Text = "Записи на выбранное занятие";
                return;
            }

            _selectedLessonId = row.Id;
            BookingsTitle.Text = "Записи на занятие: " + row.Title + " (" + row.DateText + " " + row.TimeText + ")";
            BookingsGrid.ItemsSource = Store.GetLessonBookings(row.Id);
        }

        private void LessonsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBookings();
        }

        private void LessonsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditSelected();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LessonEditWindow(null) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                Reload();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            EditSelected();
        }

        private void EditSelected()
        {
            var row = LessonsGrid.SelectedItem as LessonRow;
            if (row == null)
            {
                Ui.Warn("Выберите занятие в таблице.");
                return;
            }

            var dialog = new LessonEditWindow(row.Id) { Owner = this };
            if (dialog.ShowDialog() == true)
            {
                Reload();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var row = LessonsGrid.SelectedItem as LessonRow;
            if (row == null)
            {
                Ui.Warn("Выберите занятие в таблице.");
                return;
            }

            if (!Ui.Confirm("Удалить занятие «" + row.Title + "» " + row.DateText + " " + row.TimeText + "?"))
            {
                return;
            }

            string error;
            if (!Store.DeleteLesson(row.Id, out error))
            {
                Ui.Error(error);
                return;
            }

            _selectedLessonId = 0;
            Ui.Info("Занятие удалено.");
            Reload();
        }

        private void MarkCame_Click(object sender, RoutedEventArgs e)
        {
            Mark("Пришёл");
        }

        private void MarkMissed_Click(object sender, RoutedEventArgs e)
        {
            Mark("Не пришёл");
        }

        private void MarkBooked_Click(object sender, RoutedEventArgs e)
        {
            Mark("Записан");
        }

        private void Mark(string status)
        {
            var row = BookingsGrid.SelectedItem as LessonBookingRow;
            if (row == null)
            {
                Ui.Warn("Выберите запись в таблице посещений.");
                return;
            }

            string error;
            if (!Store.SetVisitStatus(row.Id, status, out error))
            {
                Ui.Error(error);
                return;
            }

            LoadBookings();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            FromBox.Text = Fmt.Date(DateTime.Today);
            ToBox.Text = Fmt.Date(DateTime.Today.AddDays(14));
            HallCombo.SelectedValue = 0;
            _selectedLessonId = 0;
            Reload();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Store.Refresh();
            Reload();
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
