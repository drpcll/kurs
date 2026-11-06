using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Главное окно клиента: абонемент, разделы и ближайшие занятия.</summary>
    public partial class ClientMainWindow : Window
    {
        public ClientMainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            if (!Session.IsClient)
            {
                return;
            }

            var client = Store.GetClient(Session.KlientId);
            if (client == null)
            {
                Ui.Error("Клиент не найден. Выполните вход повторно.");
                return;
            }

            UserText.Text = client.Familiya + " " + client.Imya;
            GreetingText.Text = "Здравствуйте, " + client.Imya + "!";

            var membership = Store.GetActiveMembership(client.Id);
            if (membership != null)
            {
                var tip = Store.GetTip(membership.TipId);
                var days = (membership.End.Date - DateTime.Today).Days;
                MembershipText.Text = "Абонемент «" + (tip != null ? tip.Nazvanie : "—") +
                                      "» действует до " + Fmt.DateLong(membership.End) +
                                      " г. Осталось дней: " + days + ".";
            }
            else
            {
                MembershipText.Text = "Действующего абонемента нет. Оформите абонемент у администратора клуба.";
            }

            var lessons = Store.GetSchedule(DateTime.Today, DateTime.Today.AddDays(14), null, client.Id)
                .Take(6)
                .ToList();
            LessonsList.ItemsSource = lessons;
            LessonsList.Visibility = lessons.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            FooterText.Text = lessons.Count == 0 ? "Ближайших занятий нет." : string.Empty;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Ui.Close(this);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Ui.Logout();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Store.Refresh();
            Reload();
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            var section = (sender as FrameworkElement)?.Tag as string;
            switch (section)
            {
                case "Schedule":
                    OpenChild(new ClientScheduleWindow());
                    break;
                case "Bookings":
                    OpenChild(new MyBookingsWindow());
                    break;
                case "Membership":
                    OpenChild(new MembershipWindow());
                    break;
                case "History":
                    OpenChild(new VisitHistoryWindow());
                    break;
            }
        }

        private void OpenSchedule_Click(object sender, RoutedEventArgs e)
        {
            OpenChild(new ClientScheduleWindow());
        }

        private void OpenChild(Window window)
        {
            window.Owner = this;
            window.ShowDialog();
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
    }
}
