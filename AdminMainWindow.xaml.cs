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
            ClientsGrid.ItemsSource = DemoData.Clients();
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
            Application.Current.Shutdown();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        private void Tile_Click(object sender, RoutedEventArgs e)
        {
            var section = (sender as FrameworkElement)?.Tag as string ?? "Раздел";
            MessageBox.Show(
                "Раздел «" + section + "» открывается в отдельной форме " +
                "(демонстрационный режим).",
                "Фитнес-клуб",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
