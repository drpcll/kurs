using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Регистрация нового клиента и создание учётной записи.</summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => FamiliyaBox.Focus();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var familiya = (FamiliyaBox.Text ?? string.Empty).Trim();
            var imya = (ImyaBox.Text ?? string.Empty).Trim();
            var telefon = (TelefonBox.Text ?? string.Empty).Trim();
            var login = (LoginBox.Text ?? string.Empty).Trim();
            var password = PasswordBox.Password ?? string.Empty;
            var password2 = Password2Box.Password ?? string.Empty;

            if (familiya.Length == 0 || imya.Length == 0 || telefon.Length == 0 ||
                login.Length == 0 || password.Length == 0 || password2.Length == 0)
            {
                ShowError("Заполните все поля");
                return;
            }

            if (password != password2)
            {
                ShowError("Пароли не совпадают");
                return;
            }

            string error;
            var clientId = Store.RegisterClient(familiya, imya, telefon, login, password, out error);
            if (clientId < 0)
            {
                ShowError(error);
                return;
            }

            if (!Store.Login(login, password, out error))
            {
                ShowError(error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
