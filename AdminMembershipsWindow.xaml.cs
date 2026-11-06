using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Оформление и продление абонементов.</summary>
    public partial class AdminMembershipsWindow : Window
    {
        public AdminMembershipsWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            StartBox.Text = Fmt.Date(DateTime.Today);
            StatusCombo.SelectedIndex = 0;
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            var selectedClient = ClientCombo.SelectedValue;
            var selectedType = TypeCombo.SelectedValue;

            ClientCombo.ItemsSource = Store.GetClientRows(string.Empty);
            TypeCombo.ItemsSource = Store.Tipy;

            if (selectedClient != null)
            {
                ClientCombo.SelectedValue = selectedClient;
            }
            else if (ClientCombo.Items.Count > 0)
            {
                ClientCombo.SelectedIndex = 0;
            }

            if (selectedType != null)
            {
                TypeCombo.SelectedValue = selectedType;
            }
            else if (TypeCombo.Items.Count > 0)
            {
                TypeCombo.SelectedIndex = 0;
            }

            var status = "Все";
            var item = StatusCombo.SelectedItem as ComboBoxItem;
            if (item != null && item.Content != null)
            {
                status = item.Content.ToString();
            }

            var rows = Store.GetMembershipRows(SearchBox.Text, status);
            MembershipGrid.ItemsSource = rows;
            FooterText.Text = "Показано абонементов: " + rows.Count;
        }

        private void Issue_Click(object sender, RoutedEventArgs e)
        {
            if (ClientCombo.SelectedValue == null || TypeCombo.SelectedValue == null)
            {
                Ui.Warn("Выберите клиента и тип абонемента.");
                return;
            }

            DateTime start;
            if (!Ui.ParseDate(StartBox.Text, out start))
            {
                Ui.Error("Дата начала указана неверно. Формат: дд.мм.гггг");
                return;
            }

            string error;
            if (!Store.AddMembership(
                Convert.ToInt32(ClientCombo.SelectedValue),
                Convert.ToInt32(TypeCombo.SelectedValue),
                start, out error))
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Абонемент оформлен.");
            Reload();
        }

        private void Extend_Click(object sender, RoutedEventArgs e)
        {
            var row = MembershipGrid.SelectedItem as MembershipRow;
            if (row == null)
            {
                Ui.Warn("Выберите абонемент в таблице.");
                return;
            }

            if (!Ui.Confirm("Продлить абонемент клиента «" + row.Client + "» на срок его типа?"))
            {
                return;
            }

            string error;
            if (!Store.ExtendMembership(row.Id, out error))
            {
                Ui.Error(error);
                return;
            }

            Ui.Info("Абонемент продлён.");
            Reload();
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            StatusCombo.SelectedIndex = 0;
            Reload();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Store.Refresh();
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
