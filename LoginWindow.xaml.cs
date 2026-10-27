using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Окно авторизации. Демонстрационные учётные записи: admin/admin и client/client.</summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => LoginBox.Focus();
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
            MessageBox.Show(
                "Регистрация нового клиента (демонстрационный режим).",
                "Фитнес-клуб",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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

            if (login == "admin" && password == "admin")
            {
                Open(new AdminMainWindow());
                return;
            }

            if (login == "client" && password == "client")
            {
                Open(new ClientMainWindow());
                return;
            }

            ShowError("Неверный логин или пароль");
        }

        private void Open(Window window)
        {
            window.Show();
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
