using System;
using System.Collections.Generic;
using System.Globalization;

namespace FitnessClub
{
    /// <summary>Строка списка ближайших занятий.</summary>
    public class LessonRow
    {
        public string When { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public string Hall { get; set; }
        public string Seats { get; set; }
    }

    /// <summary>Строка таблицы клиентов (панель администратора).</summary>
    public class ClientRow
    {
        public string Familiya { get; set; }
        public string Imya { get; set; }
        public string Telefon { get; set; }
        public string Abonement { get; set; }
        public string Status { get; set; }
    }

    /// <summary>Демонстрационные данные, совпадающие с тестовым набором базы данных.</summary>
    public static class DemoData
    {
        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

        public static string FormatDate(DateTime date)
        {
            return date.ToString("d MMMM yyyy", Ru);
        }

        public static List<LessonRow> Lessons()
        {
            var today = DateTime.Today;
            return new List<LessonRow>
            {
                new LessonRow
                {
                    When = today.AddDays(1).ToString("dd.MM, ddd", Ru) + " · 09:00",
                    Title = "Силовая тренировка",
                    Trainer = "Петров",
                    Hall = "Тренажёрный зал",
                    Seats = "18 из 30"
                },
                new LessonRow
                {
                    When = today.AddDays(1).ToString("dd.MM, ddd", Ru) + " · 18:00",
                    Title = "Интервальный кардио-тренинг",
                    Trainer = "Смирнова",
                    Hall = "Тренажёрный зал",
                    Seats = "25 из 30"
                },
                new LessonRow
                {
                    When = today.AddDays(2).ToString("dd.MM, ddd", Ru) + " · 11:00",
                    Title = "Функциональный тренинг",
                    Trainer = "Кузнецов",
                    Hall = "Зал групповых программ",
                    Seats = "17 из 20"
                },
                new LessonRow
                {
                    When = today.AddDays(2).ToString("dd.MM, ddd", Ru) + " · 20:00",
                    Title = "Стретчинг",
                    Trainer = "Иванова",
                    Hall = "Зал групповых программ",
                    Seats = "12 из 20"
                },
                new LessonRow
                {
                    When = today.AddDays(3).ToString("dd.MM, ddd", Ru) + " · 18:00",
                    Title = "Аквааэробика",
                    Trainer = "Волкова",
                    Hall = "Бассейн",
                    Seats = "15 из 25"
                }
            };
        }

        public static List<ClientRow> Clients()
        {
            return new List<ClientRow>
            {
                new ClientRow { Familiya = "Иванов", Imya = "Алексей", Telefon = "+7 900 123-45-01",
                    Abonement = "Месячный безлимит", Status = "Действует" },
                new ClientRow { Familiya = "Петрова", Imya = "Мария", Telefon = "+7 900 123-45-02",
                    Abonement = "Месячный безлимит", Status = "Действует" },
                new ClientRow { Familiya = "Сидоров", Imya = "Дмитрий", Telefon = "+7 900 123-45-03",
                    Abonement = "Студенческий месяц", Status = "Действует" },
                new ClientRow { Familiya = "Кузнецова", Imya = "Анна", Telefon = "+7 900 123-45-04",
                    Abonement = "Годовой безлимит", Status = "Действует" },
                new ClientRow { Familiya = "Смирнов", Imya = "Игорь", Telefon = "+7 900 123-45-05",
                    Abonement = "Разовое посещение", Status = "Действует" },
                new ClientRow { Familiya = "Волкова", Imya = "Екатерина", Telefon = "+7 900 123-45-06",
                    Abonement = "Месячный безлимит", Status = "Действует" },
                new ClientRow { Familiya = "Морозов", Imya = "Павел", Telefon = "+7 900 123-45-07",
                    Abonement = "Месяц: утро", Status = "Истёк" },
                new ClientRow { Familiya = "Лебедева", Imya = "Ольга", Telefon = "+7 900 123-45-08",
                    Abonement = "Месячный безлимит", Status = "Действует" },
                new ClientRow { Familiya = "Новиков", Imya = "Сергей", Telefon = "+7 900 123-45-09",
                    Abonement = "Студенческий месяц", Status = "Действует" },
                new ClientRow { Familiya = "Соколова", Imya = "Виктория", Telefon = "+7 900 123-45-10",
                    Abonement = "Месячный безлимит", Status = "Истёк" }
            };
        }
    }
}
