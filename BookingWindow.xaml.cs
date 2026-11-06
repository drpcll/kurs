using System;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Форма подтверждения записи на конкретное занятие.</summary>
    public partial class BookingWindow : Window
    {
        private readonly int _lessonId;
        private bool _canBook;

        public BookingWindow(int lessonId)
        {
            _lessonId = lessonId;
            InitializeComponent();
            Loaded += (s, e) => Reload();
        }

        private void Reload()
        {
            ErrorText.Visibility = Visibility.Collapsed;
            WarningBorder.Visibility = Visibility.Collapsed;
            _canBook = false;
            BookButton.IsEnabled = false;

            var lesson = Store.GetLesson(_lessonId);
            if (lesson == null)
            {
                TitleText.Text = "Занятие не найдено";
                WhenText.Text = "Возможно, занятие было удалено администратором.";
                WarningText.Text = "Выберите другое занятие в расписании.";
                WarningBorder.Visibility = Visibility.Visible;
                return;
            }

            var trainer = Store.GetTrainer(lesson.TrenerId);
            var hall = Store.GetHall(lesson.ZalId);
            var booked = 0;
            foreach (var item in Store.Poseshcheniya)
            {
                if (item.ZanyatieId == lesson.Id)
                {
                    booked++;
                }
            }
            var capacity = hall != null ? hall.Vmestimost : 0;
            var free = Math.Max(0, capacity - booked);

            TitleText.Text = lesson.Nazvanie;
            WhenText.Text = Fmt.DateLong(lesson.Data) + " (" + Fmt.Weekday(lesson.Data) +
                            ") в " + lesson.Vremya;
            TrainerText.Text = trainer != null ? trainer.Familiya : "—";
            HallText.Text = hall != null ? hall.Nazvanie : "—";
            SeatsText.Text = free + " из " + capacity;

            var membership = Store.GetActiveMembership(Session.KlientId);
            if (membership == null)
            {
                MembershipText.Text = "нет действующего абонемента";
                ShowWarning("Для записи нужен действующий абонемент. Обратитесь к администратору клуба.");
                return;
            }

            var tip = Store.GetTip(membership.TipId);
            MembershipText.Text = (tip != null ? tip.Nazvanie : "абонемент") +
                                  " до " + Fmt.Date(membership.End);

            var already = false;
            foreach (var item in Store.Poseshcheniya)
            {
                if (item.ZanyatieId == lesson.Id && item.KlientId == Session.KlientId)
                {
                    already = true;
                    break;
                }
            }

            if (already)
            {
                ShowWarning("Вы уже записаны на это занятие. Отменить запись можно в разделе «Мои записи».");
                BookButton.Content = "Вы записаны";
                return;
            }

            var start = lesson.Data.Date + Store.ParseTime(lesson.Vremya);
            if (start <= DateTime.Now)
            {
                ShowWarning("Занятие уже началось или прошло. Запись на прошедшие занятия запрещена.");
                return;
            }

            if (free <= 0)
            {
                ShowWarning("Свободных мест на занятии нет.");
                BookButton.Content = "Мест нет";
                return;
            }

            _canBook = true;
            BookButton.IsEnabled = true;
            BookButton.Content = "Записаться";
        }

        private void ShowWarning(string text)
        {
            WarningText.Text = text;
            WarningBorder.Visibility = Visibility.Visible;
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            if (!_canBook)
            {
                return;
            }

            string error;
            if (!Store.Book(Session.KlientId, _lessonId, out error))
            {
                ErrorText.Text = error;
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
