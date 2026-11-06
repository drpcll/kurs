using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>История посещений клиента за период.</summary>
    public partial class VisitHistoryWindow : Window
    {
        public VisitHistoryWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            FromBox.Text = Fmt.Date(DateTime.Today.AddDays(-30));
            ToBox.Text = Fmt.Date(DateTime.Today.AddDays(30));
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            DateTime from;
            DateTime to;
            if (!Ui.ParseDate(FromBox.Text, out from))
            {
                Ui.Error("Дата начала указана неверно. Формат: дд.мм.гггг");
                return;
            }
            if (!Ui.ParseDate(ToBox.Text, out to))
            {
                Ui.Error("Дата окончания указана неверно. Формат: дд.мм.гггг");
                return;
            }

            var rows = Store.GetClientHistory(Session.KlientId, from, to);
            HistoryGrid.ItemsSource = rows;

            SummaryText.Text = "Всего записей: " + rows.Count +
                               " · Пришёл: " + rows.Count(r => r.Mark == "Пришёл") +
                               " · Не пришёл: " + rows.Count(r => r.Mark == "Не пришёл") +
                               " · Записан: " + rows.Count(r => r.Mark == "Записан") +
                               " · Период: " + Fmt.Date(from) + " — " + Fmt.Date(to);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            Reload();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            FromBox.Text = Fmt.Date(DateTime.Today.AddDays(-30));
            ToBox.Text = Fmt.Date(DateTime.Today.AddDays(30));
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
