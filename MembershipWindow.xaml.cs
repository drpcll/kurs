using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Карточка действующего абонемента и история оформлений.</summary>
    public partial class MembershipWindow : Window
    {
        public MembershipWindow()
        {
            InitializeComponent();
            UserText.Text = Session.DisplayName;
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            var active = Store.GetActiveMembership(Session.KlientId);
            var history = new List<MembershipRow>();

            foreach (var membership in Store.Abonementy
                .Where(a => a.KlientId == Session.KlientId)
                .OrderByDescending(a => a.End))
            {
                var tip = Store.GetTip(membership.TipId);
                var isActive = membership.End >= DateTime.Today;
                history.Add(new MembershipRow
                {
                    Id = membership.Id,
                    KlientId = membership.KlientId,
                    Client = Session.DisplayName,
                    Type = tip != null ? tip.Nazvanie : "—",
                    StartText = Fmt.Date(membership.Start),
                    EndText = Fmt.Date(membership.End),
                    CostText = tip != null ? Fmt.Money(tip.Stoimost) : "—",
                    Status = isActive ? "Действует" : "Истёк",
                    IsActive = isActive
                });
            }

            MembershipGrid.ItemsSource = history;

            if (active != null)
            {
                var tip = Store.GetTip(active.TipId);
                var days = (active.End.Date - DateTime.Today).Days;
                TypeText.Text = tip != null ? tip.Nazvanie : "Абонемент";
                StatusText.Text = "Действует";
                PeriodText.Text = "Период: с " + Fmt.Date(active.Start) + " по " + Fmt.Date(active.End) +
                                  " · Стоимость: " + (tip != null ? Fmt.Money(tip.Stoimost) : "—");
                DaysText.Text = days.ToString();
            }
            else
            {
                TypeText.Text = "Нет действующего абонемента";
                StatusText.Text = "Оформите абонемент у администратора клуба";
                PeriodText.Text = "После оформления абонемент появится в этом разделе.";
                DaysText.Text = "0";
            }

            FooterText.Text = "Всего абонементов в истории: " + history.Count;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Store.Refresh();
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
