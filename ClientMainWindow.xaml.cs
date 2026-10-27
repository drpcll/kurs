using System;
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

            MembershipText.Text =
                "Абонемент «Месячный безлимит» действует до " +
                DemoData.FormatDate(DateTime.Today.AddDays(10)) + " г.";

            LessonsList.ItemsSource = DemoData.Lessons();
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

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            var lesson = (sender as FrameworkElement)?.Tag as string ?? "занятие";
            MessageBox.Show(
                "Запись на занятие «" + lesson + "» добавлена в список " +
                "(демонстрационный режим).",
                "Фитнес-клуб",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
