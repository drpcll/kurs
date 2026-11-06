using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace FitnessClub
{
    /// <summary>Окно авторизации.</summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                LoginBox.Focus();
            };
        }

        private static string Shorten(string text, int length)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= length)
            {
                return text;
            }
            return text.Substring(0, length) + "…";
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
            Close();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            TryLogin();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var registration = new RegisterWindow { Owner = this };
            if (registration.ShowDialog() == true)
            {
                var client = new ClientMainWindow();
                client.Show();
                Close();
            }
        }

        private void LoginBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryLogin();
            }
        }

        private void TryLogin()
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var login = (LoginBox.Text ?? string.Empty).Trim();
            var password = PasswordBox.Password ?? string.Empty;

            if (login.Length == 0 || password.Length == 0)
            {
                ShowError("Заполните все поля");
                return;
            }

            string error;
            if (!Store.Login(login, password, out error))
            {
                ShowError(error);
                return;
            }

            Window next = Session.IsAdmin ? (Window)new AdminMainWindow() : new ClientMainWindow();
            next.Show();
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
