using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Управление карточками клиентов и их учётными записями.</summary>
    public partial class AdminClientsWindow : Window
    {
        private int _selectedId;

        public AdminClientsWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            var rows = Store.GetClientRows(SearchBox.Text);
            ClientsGrid.ItemsSource = rows;
            FooterText.Text = "Показано клиентов: " + rows.Count;
        }

        private void ClientsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = ClientsGrid.SelectedItem as ClientRow;
            if (row == null)
            {
                return;
            }

            _selectedId = row.Id;
            FamiliyaBox.Text = row.Familiya;
            ImyaBox.Text = row.Imya;
            TelefonBox.Text = row.Telefon;
            LoginBox.Text = row.Login == "—" ? string.Empty : row.Login;
            PasswordBox.Password = string.Empty;

            FormTitle.Text = "Клиент № " + row.Id;
            SaveButton.IsEnabled = true;
            DeleteButton.IsEnabled = true;
            PasswordHint.Visibility = Visibility.Visible;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string error;
            var id = Store.AddClient(
                FamiliyaBox.Text, ImyaBox.Text, TelefonBox.Text,
                LoginBox.Text, PasswordBox.Password, out error);

            if (id < 0)
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Клиент добавлен. Номер в базе: " + id + ".");
            ClearForm();
            Reload();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0)
            {
                Ui.Warn("Выберите клиента в таблице.");
                return;
            }

            string error;
            if (!Store.UpdateClient(_selectedId,
                FamiliyaBox.Text, ImyaBox.Text, TelefonBox.Text,
                LoginBox.Text, PasswordBox.Password, out error))
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Данные клиента сохранены.");
            Reload();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == 0)
            {
                Ui.Warn("Выберите клиента в таблице.");
                return;
            }

            if (!Ui.Confirm("Удалить выбранного клиента и его учётную запись?"))
            {
                return;
            }

            string error;
            if (!Store.DeleteClient(_selectedId, out error))
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Клиент удалён.");
            ClearForm();
            Reload();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _selectedId = 0;
            FamiliyaBox.Text = string.Empty;
            ImyaBox.Text = string.Empty;
            TelefonBox.Text = string.Empty;
            LoginBox.Text = string.Empty;
            PasswordBox.Password = string.Empty;
            FormTitle.Text = "Новый клиент";
            SaveButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            PasswordHint.Visibility = Visibility.Collapsed;
            ClientsGrid.SelectedItem = null;
            FamiliyaBox.Focus();
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
