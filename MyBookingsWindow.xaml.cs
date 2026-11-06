using System;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Будущие записи клиента с возможностью отмены.</summary>
    public partial class MyBookingsWindow : Window
    {
        public MyBookingsWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            var rows = Store.GetClientBookings(Session.KlientId);
            BookingsGrid.ItemsSource = rows;
            CountText.Text = "Всего записей: " + rows.Count +
                             " · Отмена доступна до начала занятия";
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Store.Refresh();
            Reload();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            var lessonId = Convert.ToInt32(element.Tag);
            if (!Ui.Confirm("Отменить запись на выбранное занятие?"))
            {
                return;
            }

            string error;
            if (!Store.CancelBooking(Session.KlientId, lessonId, out error))
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Запись отменена.");
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
