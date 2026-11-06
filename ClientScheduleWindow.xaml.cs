using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Полное расписание занятий с фильтрами и записью.</summary>
    public partial class ClientScheduleWindow : Window
    {
        public ClientScheduleWindow()
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
            var rows = Store.GetSchedule(from, to, hallId == 0 ? (int?)null : hallId, Session.KlientId);
            LessonsGrid.ItemsSource = rows;
            CountText.Text = "Найдено занятий: " + rows.Count +
                             " · Период: " + Fmt.Date(from) + " — " + Fmt.Date(to);
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
            Reload();
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            var id = Convert.ToInt32(element.Tag);
            var booking = new BookingWindow(id) { Owner = this };
            if (booking.ShowDialog() == true)
            {
                Reload();
            }
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
