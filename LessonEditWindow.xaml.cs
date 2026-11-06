using System;
using System.Windows;
using System.Windows.Input;

namespace FitnessClub
{
    /// <summary>Добавление и редактирование занятия.</summary>
    public partial class LessonEditWindow : Window
    {
        private readonly int? _lessonId;

        public LessonEditWindow(int? lessonId)
        {
            _lessonId = lessonId;
            InitializeComponent();

            TrainerCombo.ItemsSource = Store.Trenery;
            HallCombo.ItemsSource = Store.Zaly;

            if (lessonId.HasValue)
            {
                FormTitle.Text = "Редактирование занятия";
                var lesson = Store.GetLesson(lessonId.Value);
                if (lesson != null)
                {
                    TitleBox.Text = lesson.Nazvanie;
                    TrainerCombo.SelectedValue = lesson.TrenerId;
                    HallCombo.SelectedValue = lesson.ZalId;
                    DateBox.Text = Fmt.Date(lesson.Data);
                    TimeBox.Text = lesson.Vremya;
                }
            }
            else
            {
                FormTitle.Text = "Добавление занятия";
                DateBox.Text = Fmt.Date(DateTime.Today.AddDays(1));
                TimeBox.Text = "18:00";
                if (TrainerCombo.Items.Count > 0)
                {
                    TrainerCombo.SelectedIndex = 0;
                }
                if (HallCombo.Items.Count > 0)
                {
                    HallCombo.SelectedIndex = 0;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            if (TrainerCombo.SelectedValue == null || HallCombo.SelectedValue == null)
            {
                ShowError("Выберите тренера и зал");
                return;
            }

            DateTime date;
            if (!Ui.ParseDate(DateBox.Text, out date))
            {
                ShowError("Дата указана неверно. Формат: дд.мм.гггг");
                return;
            }

            string error;
            if (!Store.SaveLesson(
                _lessonId,
                TitleBox.Text,
                Convert.ToInt32(TrainerCombo.SelectedValue),
                Convert.ToInt32(HallCombo.SelectedValue),
                date,
                TimeBox.Text,
                out error))
            {
                ShowError(error);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void ShowError(string text)
        {
            ErrorText.Text = text;
            ErrorText.Visibility = Visibility.Visible;
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
