using System;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Главное окно администратора: сводные показатели и список клиентов.</summary>
    public partial class AdminMainWindow : Window
    {
        public AdminMainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            UserText.Text = Session.DisplayName;
            ClientCountText.Text = Store.CountClients.ToString();
            MembershipCountText.Text = Store.CountActiveMemberships.ToString();
            LessonCountText.Text = Store.CountLessons.ToString();
            TrainerCountText.Text = Store.CountTrainers.ToString();

            var rows = Store.GetClientRows(SearchBox.Text);
            ClientsGrid.ItemsSource = rows;
            FooterText.Text = "Показано клиентов: " + rows.Count;
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

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            Reload();
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Reload();
            }
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            var section = (sender as FrameworkElement)?.Tag as string;
            Window window = null;

            switch (section)
            {
                case "Clients":
                    window = new AdminClientsWindow();
                    break;
                case "Memberships":
                    window = new AdminMembershipsWindow();
                    break;
                case "Schedule":
                    window = new AdminScheduleWindow();
                    break;
                case "Reports":
                    window = new ReportsWindow();
                    break;
            }

            if (window == null)
            {
                return;
            }

            window.Owner = this;
            window.ShowDialog();
            Reload();
        }
    }
}
