using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace FitnessClub
{
    /// <summary>
    /// Единая точка доступа к данным. Основной источник — MS SQL Server
    /// (user95_db, таблицы KURS_*). Если сервер недоступен, приложение
    /// автоматически переходит к автономному хранилищу и хранит данные в XML,
    /// чтобы все функции оставались работоспособными.
    /// </summary>
    public static class Store
    {
        public const string DbServer = "stud-mssql.sttec.yar.ru,38325";
        public const string DbName = "user95_db";
        public const string DbUser = "user95_db";
        public const string DbPassword = "user95";

        private const string ConnectionString =
            "Data Source=" + DbServer + ";Initial Catalog=" + DbName +
            ";User ID=" + DbUser + ";Password=" + DbPassword +
            ";Connect Timeout=6;TrustServerCertificate=True;";

        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");
        private static readonly string[] Marks = { "Записан", "Пришёл", "Не пришёл" };

        public static bool IsOnline { get; private set; }
        public static string StatusText { get; private set; }
        public static string LastError { get; private set; }

        public static List<Klient> Klients { get; private set; }
        public static List<Polzovatel> Polzovateli { get; private set; }
        public static List<Administrator> Administratory { get; private set; }
        public static List<TipAbonementa> Tipy { get; private set; }
        public static List<Abonement> Abonementy { get; private set; }
        public static List<Spetsializatsiya> Spetsializatsii { get; private set; }
        public static List<Trener> Trenery { get; private set; }
        public static List<Zal> Zaly { get; private set; }
        public static List<Zanyatie> Zanyatiya { get; private set; }
        public static List<Poseshchenie> Poseshcheniya { get; private set; }

        private static string LocalPath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "FitnessClub", "localdata.xml");
            }
        }

        // =================================================================
        //  Инициализация и загрузка
        // =================================================================

        public static void Initialize()
        {
            try
            {
                LoadFromSql();
                IsOnline = true;
                LastError = null;
                StatusText = "SQL Server: " + DbName + " — подключено";
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                LoadLocal();
                IsOnline = false;
                StatusText = "Автономное хранилище — нет связи с SQL Server";
            }
        }

        /// <summary>Повторная попытка подключения к серверу.</summary>
        public static bool RetryConnection()
        {
            try
            {
                LoadFromSql();
                IsOnline = true;
                LastError = null;
                StatusText = "SQL Server: " + DbName + " — подключено";
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                IsOnline = false;
                StatusText = "Автономное хранилище — нет связи с SQL Server";
                return false;
            }
        }

        public static void Refresh()
        {
            if (IsOnline)
            {
                try
                {
                    LoadFromSql();
                    return;
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    IsOnline = false;
                    StatusText = "Автономное хранилище — нет связи с SQL Server";
                }
            }
            LoadLocal();
        }

        private static SqlConnection OpenSql()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        private static void LoadFromSql()
        {
            using (var connection = OpenSql())
            {
                Klients = Read(connection,
                    "SELECT Kod_klienta, Familiya, Imya, Telefon FROM dbo.KURS_Klient ORDER BY Kod_klienta",
                    r => new Klient
                    {
                        Id = r.GetInt32(0),
                        Familiya = r.GetString(1),
                        Imya = r.GetString(2),
                        Telefon = r.GetString(3)
                    });

                Polzovateli = Read(connection,
                    "SELECT Kod_polzovatelya, Kod_klienta, Login, Parol, Rol FROM dbo.KURS_Polzovatel ORDER BY Kod_polzovatelya",
                    r => new Polzovatel
                    {
                        Id = r.GetInt32(0),
                        KlientId = r.GetInt32(1),
                        Login = r.GetString(2),
                        Parol = r.GetString(3),
                        Rol = r.GetString(4)
                    });

                Administratory = Read(connection,
                    "SELECT Kod_admina, Login, Parol, Familiya FROM dbo.KURS_Administrator ORDER BY Kod_admina",
                    r => new Administrator
                    {
                        Id = r.GetInt32(0),
                        Login = r.GetString(1),
                        Parol = r.GetString(2),
                        Familiya = r.GetString(3)
                    });

                Tipy = Read(connection,
                    "SELECT Kod_tipa, Nazvanie, Stoimost, Srok_deystviya FROM dbo.KURS_TipAbonementa ORDER BY Kod_tipa",
                    r => new TipAbonementa
                    {
                        Id = r.GetInt32(0),
                        Nazvanie = r.GetString(1),
                        Stoimost = r.GetDecimal(2),
                        Srok = r.GetInt32(3)
                    });

                Abonementy = Read(connection,
                    "SELECT Kod_abonementa, Kod_klienta, Kod_tipa, Data_nachala, Data_okonchaniya FROM dbo.KURS_Abonement ORDER BY Kod_abonementa",
                    r => new Abonement
                    {
                        Id = r.GetInt32(0),
                        KlientId = r.GetInt32(1),
                        TipId = r.GetInt32(2),
                        Start = r.GetDateTime(3).Date,
                        End = r.GetDateTime(4).Date
                    });

                Spetsializatsii = Read(connection,
                    "SELECT Kod_spetsializatsii, Nazvanie FROM dbo.KURS_Spetsializatsiya ORDER BY Kod_spetsializatsii",
                    r => new Spetsializatsiya { Id = r.GetInt32(0), Nazvanie = r.GetString(1) });

                Trenery = Read(connection,
                    "SELECT Kod_trenera, Familiya, Kod_spetsializatsii, Stazh FROM dbo.KURS_Trener ORDER BY Kod_trenera",
                    r => new Trener
                    {
                        Id = r.GetInt32(0),
                        Familiya = r.GetString(1),
                        SpetsId = r.GetInt32(2),
                        Stazh = r.GetInt32(3)
                    });

                Zaly = Read(connection,
                    "SELECT Kod_zala, Nazvanie, Vmestimost FROM dbo.KURS_Zal ORDER BY Kod_zala",
                    r => new Zal
                    {
                        Id = r.GetInt32(0),
                        Nazvanie = r.GetString(1),
                        Vmestimost = r.GetInt32(2)
                    });

                Zanyatiya = Read(connection,
                    "SELECT Kod_zanyatiya, Kod_trenera, Kod_zala, Data, Vremya, Nazvanie FROM dbo.KURS_Zanyatie ORDER BY Data, Vremya",
                    r => new Zanyatie
                    {
                        Id = r.GetInt32(0),
                        TrenerId = r.GetInt32(1),
                        ZalId = r.GetInt32(2),
                        Data = r.GetDateTime(3).Date,
                        Vremya = r.GetTimeSpan(4).ToString(@"hh\:mm"),
                        Nazvanie = r.GetString(5)
                    });

                Poseshcheniya = Read(connection,
                    "SELECT Kod_poseshcheniya, Kod_klienta, Kod_zanyatiya, Data, Otmetka FROM dbo.KURS_Poseshchenie ORDER BY Kod_poseshcheniya",
                    r => new Poseshchenie
                    {
                        Id = r.GetInt32(0),
                        KlientId = r.GetInt32(1),
                        ZanyatieId = r.GetInt32(2),
                        Data = r.GetDateTime(3).Date,
                        Otmetka = r.GetString(4)
                    });
            }
        }

        private static List<T> Read<T>(SqlConnection connection, string sql, Func<SqlDataReader, T> map)
        {
            var list = new List<T>();
            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(map(reader));
                }
            }
            return list;
        }

        // =================================================================
        //  Автономное хранилище
        // =================================================================

        private static void LoadLocal()
        {
            try
            {
                if (File.Exists(LocalPath))
                {
                    var serializer = new XmlSerializer(typeof(LocalData));
                    using (var stream = File.OpenRead(LocalPath))
                    {
                        var data = (LocalData)serializer.Deserialize(stream);
                        ApplyLocal(data);
                        EnsureSampleRecords();
                        return;
                    }
                }
            }
            catch
            {
                // повреждённый файл — пересоздаём начальный набор данных
            }

            Seed();
            EnsureSampleRecords();
        }

        private static void ApplyLocal(LocalData data)
        {
            Klients = data.Klients ?? new List<Klient>();
            Polzovateli = data.Polzovateli ?? new List<Polzovatel>();
            Administratory = data.Administratory ?? new List<Administrator>();
            Tipy = data.Tipy ?? new List<TipAbonementa>();
            Abonementy = data.Abonementy ?? new List<Abonement>();
            Spetsializatsii = data.Spetsializatsii ?? new List<Spetsializatsiya>();
            Trenery = data.Trenery ?? new List<Trener>();
            Zaly = data.Zaly ?? new List<Zal>();
            Zanyatiya = data.Zanyatiya ?? new List<Zanyatie>();
            Poseshcheniya = data.Poseshcheniya ?? new List<Poseshchenie>();
        }

        private static void SaveLocal()
        {
            try
            {
                var data = new LocalData
                {
                    Klients = Klients,
                    Polzovateli = Polzovateli,
                    Administratory = Administratory,
                    Tipy = Tipy,
                    Abonementy = Abonementy,
                    Spetsializatsii = Spetsializatsii,
                    Trenery = Trenery,
                    Zaly = Zaly,
                    Zanyatiya = Zanyatiya,
                    Poseshcheniya = Poseshcheniya
                };

                var dir = Path.GetDirectoryName(LocalPath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var serializer = new XmlSerializer(typeof(LocalData));
                using (var stream = File.Create(LocalPath))
                {
                    serializer.Serialize(stream, data);
                }
            }
            catch
            {
                // сохранение не должно блокировать работу интерфейса
            }
        }

        private static void Seed()
        {
            var today = DateTime.Today;

            Klients = new List<Klient>
            {
                new Klient { Id = 1,  Familiya = "Иванов",    Imya = "Алексей",   Telefon = "+79001234501" },
                new Klient { Id = 2,  Familiya = "Петрова",   Imya = "Мария",     Telefon = "+79001234502" },
                new Klient { Id = 3,  Familiya = "Сидоров",   Imya = "Дмитрий",   Telefon = "+79001234503" },
                new Klient { Id = 4,  Familiya = "Кузнецова", Imya = "Анна",      Telefon = "+79001234504" },
                new Klient { Id = 5,  Familiya = "Смирнов",   Imya = "Игорь",     Telefon = "+79001234505" },
                new Klient { Id = 6,  Familiya = "Волкова",   Imya = "Екатерина", Telefon = "+79001234506" },
                new Klient { Id = 7,  Familiya = "Морозов",   Imya = "Павел",     Telefon = "+79001234507" },
                new Klient { Id = 8,  Familiya = "Лебедева",  Imya = "Ольга",     Telefon = "+79001234508" },
                new Klient { Id = 9,  Familiya = "Новиков",   Imya = "Сергей",    Telefon = "+79001234509" },
                new Klient { Id = 10, Familiya = "Соколова",  Imya = "Виктория",  Telefon = "+79001234510" }
            };

            Polzovateli = new List<Polzovatel>();
            string[] logins = { "ivanov", "petrova", "sidorov", "kuznetsova", "smirnov",
                                "volkova", "morozov", "lebedeva", "novikov", "sokolova" };
            for (int i = 0; i < logins.Length; i++)
            {
                Polzovateli.Add(new Polzovatel
                {
                    Id = i + 1,
                    KlientId = i + 1,
                    Login = logins[i],
                    Parol = "client123",
                    Rol = "Client"
                });
            }

            Administratory = new List<Administrator>
            {
                new Administrator { Id = 1, Login = "admin",   Parol = "admin123",   Familiya = "Смирнова" },
                new Administrator { Id = 2, Login = "manager", Parol = "manager123", Familiya = "Орлова" }
            };

            Tipy = new List<TipAbonementa>
            {
                new TipAbonementa { Id = 1, Nazvanie = "Разовое посещение",  Stoimost = 500m,   Srok = 1 },
                new TipAbonementa { Id = 2, Nazvanie = "Месячный безлимит",  Stoimost = 4500m,  Srok = 30 },
                new TipAbonementa { Id = 3, Nazvanie = "Годовой безлимит",   Stoimost = 39000m, Srok = 365 },
                new TipAbonementa { Id = 4, Nazvanie = "Студенческий месяц", Stoimost = 3200m,  Srok = 30 },
                new TipAbonementa { Id = 5, Nazvanie = "Месяц: утро",        Stoimost = 3500m,  Srok = 30 }
            };

            Spetsializatsii = new List<Spetsializatsiya>
            {
                new Spetsializatsiya { Id = 1, Nazvanie = "Силовой тренинг" },
                new Spetsializatsiya { Id = 2, Nazvanie = "Йога и стретчинг" },
                new Spetsializatsiya { Id = 3, Nazvanie = "Кардиотренировки" },
                new Spetsializatsiya { Id = 4, Nazvanie = "Функциональный тренинг" },
                new Spetsializatsiya { Id = 5, Nazvanie = "Плавание" }
            };

            Zaly = new List<Zal>
            {
                new Zal { Id = 1, Nazvanie = "Тренажёрный зал",        Vmestimost = 30 },
                new Zal { Id = 2, Nazvanie = "Зал групповых программ", Vmestimost = 20 },
                new Zal { Id = 3, Nazvanie = "Бассейн",                Vmestimost = 25 }
            };

            Trenery = new List<Trener>
            {
                new Trener { Id = 1, Familiya = "Иванова",  SpetsId = 2, Stazh = 6 },
                new Trener { Id = 2, Familiya = "Петров",   SpetsId = 1, Stazh = 12 },
                new Trener { Id = 3, Familiya = "Смирнова", SpetsId = 3, Stazh = 4 },
                new Trener { Id = 4, Familiya = "Кузнецов", SpetsId = 4, Stazh = 8 },
                new Trener { Id = 5, Familiya = "Волкова",  SpetsId = 5, Stazh = 10 },
                new Trener { Id = 6, Familiya = "Морозов",  SpetsId = 1, Stazh = 15 },
                new Trener { Id = 7, Familiya = "Лебедева", SpetsId = 2, Stazh = 3 },
                new Trener { Id = 8, Familiya = "Соколов",  SpetsId = 4, Stazh = 7 }
            };

            Abonementy = new List<Abonement>
            {
                new Abonement { Id = 1,  KlientId = 1,  TipId = 2, Start = today.AddDays(-20),  End = today.AddDays(10) },
                new Abonement { Id = 2,  KlientId = 2,  TipId = 2, Start = today.AddDays(-10),  End = today.AddDays(20) },
                new Abonement { Id = 3,  KlientId = 3,  TipId = 4, Start = today.AddDays(-25),  End = today.AddDays(5) },
                new Abonement { Id = 4,  KlientId = 4,  TipId = 3, Start = today.AddDays(-120), End = today.AddDays(245) },
                new Abonement { Id = 5,  KlientId = 5,  TipId = 1, Start = today,               End = today },
                new Abonement { Id = 6,  KlientId = 6,  TipId = 2, Start = today.AddDays(-5),   End = today.AddDays(25) },
                new Abonement { Id = 7,  KlientId = 7,  TipId = 5, Start = today.AddDays(-40),  End = today.AddDays(-10) },
                new Abonement { Id = 8,  KlientId = 8,  TipId = 2, Start = today.AddDays(-15),  End = today.AddDays(15) },
                new Abonement { Id = 9,  KlientId = 9,  TipId = 4, Start = today.AddDays(-3),   End = today.AddDays(27) },
                new Abonement { Id = 10, KlientId = 10, TipId = 2, Start = today.AddDays(-30),  End = today },
                new Abonement { Id = 11, KlientId = 1,  TipId = 5, Start = today.AddDays(-60),  End = today.AddDays(-30) },
                new Abonement { Id = 12, KlientId = 6,  TipId = 1, Start = today.AddDays(-50),  End = today.AddDays(-50) }
            };

            Zanyatiya = new List<Zanyatie>
            {
                new Zanyatie { Id = 1,  TrenerId = 2, ZalId = 1, Data = today.AddDays(1), Vremya = "09:00", Nazvanie = "Силовая тренировка" },
                new Zanyatie { Id = 2,  TrenerId = 7, ZalId = 2, Data = today.AddDays(1), Vremya = "10:30", Nazvanie = "Хатха-йога" },
                new Zanyatie { Id = 3,  TrenerId = 3, ZalId = 1, Data = today.AddDays(1), Vremya = "18:00", Nazvanie = "Интервальный кардио-тренинг" },
                new Zanyatie { Id = 4,  TrenerId = 5, ZalId = 3, Data = today.AddDays(1), Vremya = "19:00", Nazvanie = "Плавание для начинающих" },
                new Zanyatie { Id = 5,  TrenerId = 4, ZalId = 2, Data = today.AddDays(2), Vremya = "11:00", Nazvanie = "Функциональный тренинг" },
                new Zanyatie { Id = 6,  TrenerId = 6, ZalId = 1, Data = today.AddDays(2), Vremya = "18:30", Nazvanie = "Кроссфит" },
                new Zanyatie { Id = 7,  TrenerId = 1, ZalId = 2, Data = today.AddDays(2), Vremya = "20:00", Nazvanie = "Стретчинг" },
                new Zanyatie { Id = 8,  TrenerId = 2, ZalId = 1, Data = today.AddDays(3), Vremya = "09:00", Nazvanie = "Силовая тренировка" },
                new Zanyatie { Id = 9,  TrenerId = 8, ZalId = 2, Data = today.AddDays(3), Vremya = "17:00", Nazvanie = "Круговая тренировка" },
                new Zanyatie { Id = 10, TrenerId = 5, ZalId = 3, Data = today.AddDays(3), Vremya = "18:00", Nazvanie = "Аквааэробика" },
                new Zanyatie { Id = 11, TrenerId = 3, ZalId = 1, Data = today.AddDays(4), Vremya = "10:00", Nazvanie = "Кардио-микс" },
                new Zanyatie { Id = 12, TrenerId = 7, ZalId = 2, Data = today.AddDays(4), Vremya = "12:00", Nazvanie = "Йога для беременных" },
                new Zanyatie { Id = 13, TrenerId = 4, ZalId = 2, Data = today.AddDays(5), Vremya = "19:30", Nazvanie = "Функциональный тренинг" },
                new Zanyatie { Id = 14, TrenerId = 6, ZalId = 1, Data = today.AddDays(6), Vremya = "18:00", Nazvanie = "Силовой практикум" }
            };

            Poseshcheniya = new List<Poseshchenie>();
            int[,] seedBookings =
            {
                { 1, 1 }, { 2, 1 }, { 3, 1 }, { 4, 2 }, { 6, 2 }, { 1, 3 }, { 5, 3 },
                { 7, 4 }, { 9, 4 }, { 2, 5 }, { 8, 5 }, { 3, 6 }, { 6, 6 }, { 4, 7 },
                { 10, 7 }, { 1, 8 }, { 5, 9 }, { 7, 10 }, { 9, 11 }, { 2, 12 },
                { 8, 13 }, { 10, 14 }
            };
            for (int i = 0; i < seedBookings.GetLength(0); i++)
            {
                Poseshcheniya.Add(new Poseshchenie
                {
                    Id = i + 1,
                    KlientId = seedBookings[i, 0],
                    ZanyatieId = seedBookings[i, 1],
                    Data = today,
                    Otmetka = "Записан"
                });
            }
        }

        private static void EnsureSampleRecords()
        {
            var today = DateTime.Today;

            // Для демонстрации кабинетов у нескольких клиентов должны быть
            // действующие абонементы и будущие занятия.
            foreach (var clientId in new[] { 1, 2, 3, 4, 5, 6 })
            {
                var activeMembership = Abonementy.FirstOrDefault(a =>
                    a.KlientId == clientId && a.Start.Date <= today && a.End.Date >= today);
                if (activeMembership == null)
                {
                    Abonementy.Add(new Abonement
                    {
                        Id = Abonementy.Count == 0 ? 1 : Abonementy.Max(a => a.Id) + 1,
                        KlientId = clientId,
                        TipId = clientId % 2 == 0 ? 2 : 4,
                        Start = today.AddDays(-7),
                        End = today.AddDays(30)
                    });
                }
            }

            // Дополнительные прошедшие занятия нужны для истории посещений
            // клиента и отчётов администратора.
            if (!Zanyatiya.Any(z => z.Id == 15))
            {
                Zanyatiya.Add(new Zanyatie
                {
                    Id = 15, TrenerId = 2, ZalId = 1, Data = today.AddDays(-3),
                    Vremya = "09:00", Nazvanie = "Утренняя силовая тренировка"
                });
            }
            if (!Zanyatiya.Any(z => z.Id == 16))
            {
                Zanyatiya.Add(new Zanyatie
                {
                    Id = 16, TrenerId = 7, ZalId = 2, Data = today.AddDays(-2),
                    Vremya = "18:30", Nazvanie = "Вечерняя йога"
                });
            }
            if (!Zanyatiya.Any(z => z.Id == 17))
            {
                Zanyatiya.Add(new Zanyatie
                {
                    Id = 17, TrenerId = 4, ZalId = 2, Data = today.AddDays(-1),
                    Vremya = "19:00", Nazvanie = "Функциональная тренировка"
                });
            }

            AddSampleBooking(1, 2, "Записан");
            AddSampleBooking(1, 4, "Записан");
            AddSampleBooking(1, 6, "Записан");
            AddSampleBooking(2, 5, "Записан");
            AddSampleBooking(2, 8, "Записан");
            AddSampleBooking(3, 9, "Записан");
            AddSampleBooking(3, 10, "Записан");
            AddSampleBooking(4, 11, "Записан");
            AddSampleBooking(4, 12, "Записан");
            AddSampleBooking(5, 1, "Записан");
            AddSampleBooking(6, 3, "Записан");
            AddSampleBooking(1, 15, "Пришёл");
            AddSampleBooking(2, 16, "Пришёл");
            AddSampleBooking(3, 17, "Не пришёл");
            AddSampleBooking(5, 15, "Пришёл");
            SaveLocal();
        }

        private static void AddSampleBooking(int clientId, int lessonId, string mark)
        {
            if (Poseshcheniya.Any(p => p.KlientId == clientId && p.ZanyatieId == lessonId))
            {
                return;
            }

            Poseshcheniya.Add(new Poseshchenie
            {
                Id = Poseshcheniya.Count == 0 ? 1 : Poseshcheniya.Max(p => p.Id) + 1,
                KlientId = clientId,
                ZanyatieId = lessonId,
                Data = DateTime.Today,
                Otmetka = mark
            });
        }

        // =================================================================
        //  Авторизация и регистрация
        // =================================================================

        public static bool Login(string login, string password, out string error)
        {
            error = null;
            login = (login ?? string.Empty).Trim();

            var user = Polzovateli.FirstOrDefault(u =>
                string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase) &&
                u.Parol == password);

            if (user != null)
            {
                var client = GetClient(user.KlientId);
                if (client == null)
                {
                    error = "Учётная запись не связана с клиентом";
                    return false;
                }

                Session.Role = "Client";
                Session.KlientId = client.Id;
                Session.AdminId = 0;
                Session.Login = user.Login;
                Session.DisplayName = client.Familiya + " " + client.Imya;
                return true;
            }

            var admin = Administratory.FirstOrDefault(a =>
                string.Equals(a.Login, login, StringComparison.OrdinalIgnoreCase) &&
                a.Parol == password);

            if (admin != null)
            {
                Session.Role = "Admin";
                Session.KlientId = 0;
                Session.AdminId = admin.Id;
                Session.Login = admin.Login;
                Session.DisplayName = admin.Familiya;
                return true;
            }

            error = "Неверный логин или пароль";
            return false;
        }

        public static void Logout()
        {
            Session.Role = null;
            Session.KlientId = 0;
            Session.AdminId = 0;
            Session.Login = null;
            Session.DisplayName = null;
        }

        public static int RegisterClient(string familiya, string imya, string telefon,
                                         string login, string password, out string error)
        {
            return AddClient(familiya, imya, telefon, login, password, out error);
        }

        // =================================================================
        //  Клиенты
        // =================================================================

        public static Klient GetClient(int id)
        {
            return Klients.FirstOrDefault(k => k.Id == id);
        }

        public static Polzovatel GetAccount(int clientId)
        {
            return Polzovateli.FirstOrDefault(p => p.KlientId == clientId);
        }

        public static List<ClientRow> GetClientRows(string search)
        {
            var query = Klients.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(k =>
                    (k.Familiya + " " + k.Imya).ToLowerInvariant().Contains(s) ||
                    (k.Telefon ?? string.Empty).Contains(s) ||
                    ((GetAccount(k.Id) != null ? GetAccount(k.Id).Login : string.Empty))
                        .ToLowerInvariant().Contains(s));
            }

            var today = DateTime.Today;
            return query
                .OrderBy(k => k.Familiya).ThenBy(k => k.Imya)
                .Select(k =>
                {
                    var membership = Abonementy
                        .Where(a => a.KlientId == k.Id)
                        .OrderByDescending(a => a.End)
                        .FirstOrDefault();
                    var account = GetAccount(k.Id);
                    var active = membership != null && membership.End >= today;
                    var type = membership != null ? GetTip(membership.TipId) : null;

                    return new ClientRow
                    {
                        Id = k.Id,
                        Familiya = k.Familiya,
                        Imya = k.Imya,
                        Telefon = k.Telefon,
                        Login = account != null ? account.Login : "—",
                        Abonement = type != null ? type.Nazvanie : "—",
                        Status = membership == null ? "Нет абонемента" : (active ? "Действует" : "Истёк")
                    };
                })
                .ToList();
        }

        public static int AddClient(string familiya, string imya, string telefon,
                                    string login, string password, out string error)
        {
            error = ValidatePerson(familiya, imya, telefon, login, password, 0);
            if (error != null)
            {
                return -1;
            }

            familiya = familiya.Trim();
            imya = imya.Trim();
            telefon = NormalizePhone(telefon);
            login = login.Trim();

            int newId;
            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var check = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.KURS_Klient WHERE Telefon=@t;", connection, transaction))
                        {
                            check.Parameters.AddWithValue("@t", telefon);
                            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                            {
                                transaction.Rollback();
                                error = "Клиент с таким телефоном уже существует";
                                return -1;
                            }
                        }

                        using (var check = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.KURS_Polzovatel WHERE Login=@l;", connection, transaction))
                        {
                            check.Parameters.AddWithValue("@l", login);
                            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                            {
                                transaction.Rollback();
                                error = "Такой логин уже занят";
                                return -1;
                            }
                        }

                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.KURS_Klient (Familiya, Imya, Telefon) " +
                            "VALUES (@f, @i, @t); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@f", familiya);
                            cmd.Parameters.AddWithValue("@i", imya);
                            cmd.Parameters.AddWithValue("@t", telefon);
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.KURS_Polzovatel (Kod_klienta, Login, Parol, Rol) " +
                            "VALUES (@k, @l, @p, 'Client');", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@k", newId);
                            cmd.Parameters.AddWithValue("@l", login);
                            cmd.Parameters.AddWithValue("@p", password);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось сохранить клиента: " + ex.Message;
                    return -1;
                }
            }
            else
            {
                newId = Klients.Count == 0 ? 1 : Klients.Max(k => k.Id) + 1;
            }

            Klients.Add(new Klient { Id = newId, Familiya = familiya, Imya = imya, Telefon = telefon });
            Polzovateli.Add(new Polzovatel
            {
                Id = Polzovateli.Count == 0 ? 1 : Polzovateli.Max(p => p.Id) + 1,
                KlientId = newId,
                Login = login,
                Parol = password,
                Rol = "Client"
            });

            if (!IsOnline)
            {
                SaveLocal();
            }
            return newId;
        }

        public static bool UpdateClient(int clientId, string familiya, string imya, string telefon,
                                        string login, string password, out string error)
        {
            error = ValidatePerson(familiya, imya, telefon, login, password, clientId, true);
            if (error != null)
            {
                return false;
            }

            var client = GetClient(clientId);
            var account = GetAccount(clientId);
            if (client == null || account == null)
            {
                error = "Клиент не найден";
                return false;
            }

            familiya = familiya.Trim();
            imya = imya.Trim();
            telefon = NormalizePhone(telefon);
            login = login.Trim();

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var check = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.KURS_Klient WHERE Telefon=@t AND Kod_klienta<>@id;",
                            connection, transaction))
                        {
                            check.Parameters.AddWithValue("@t", telefon);
                            check.Parameters.AddWithValue("@id", clientId);
                            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                            {
                                transaction.Rollback();
                                error = "Клиент с таким телефоном уже существует";
                                return false;
                            }
                        }

                        using (var check = new SqlCommand(
                            "SELECT COUNT(*) FROM dbo.KURS_Polzovatel WHERE Login=@l AND Kod_klienta<>@id;",
                            connection, transaction))
                        {
                            check.Parameters.AddWithValue("@l", login);
                            check.Parameters.AddWithValue("@id", clientId);
                            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                            {
                                transaction.Rollback();
                                error = "Такой логин уже занят";
                                return false;
                            }
                        }

                        using (var cmd = new SqlCommand(
                            "UPDATE dbo.KURS_Klient SET Familiya=@f, Imya=@i, Telefon=@t " +
                            "WHERE Kod_klienta=@id;", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@f", familiya);
                            cmd.Parameters.AddWithValue("@i", imya);
                            cmd.Parameters.AddWithValue("@t", telefon);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            cmd.ExecuteNonQuery();
                        }

                        var sql = "UPDATE dbo.KURS_Polzovatel SET Login=@l" +
                                  (string.IsNullOrEmpty(password) ? string.Empty : ", Parol=@p") +
                                  " WHERE Kod_klienta=@id;";
                        using (var cmd = new SqlCommand(sql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@l", login);
                            cmd.Parameters.AddWithValue("@id", clientId);
                            if (!string.IsNullOrEmpty(password))
                            {
                                cmd.Parameters.AddWithValue("@p", password);
                            }
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось изменить клиента: " + ex.Message;
                    return false;
                }
            }

            client.Familiya = familiya;
            client.Imya = imya;
            client.Telefon = telefon;
            account.Login = login;
            if (!string.IsNullOrEmpty(password))
            {
                account.Parol = password;
            }

            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static bool DeleteClient(int clientId, out string error)
        {
            error = null;
            var client = GetClient(clientId);
            if (client == null)
            {
                error = "Клиент не найден";
                return false;
            }

            if (Abonementy.Any(a => a.KlientId == clientId) ||
                Poseshcheniya.Any(p => p.KlientId == clientId))
            {
                error = "Нельзя удалить клиента: есть абонементы или записи на занятия. " +
                        "Сначала удалите связанные записи.";
                return false;
            }

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var cmd = new SqlCommand(
                            "DELETE FROM dbo.KURS_Polzovatel WHERE Kod_klienta=@id;", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", clientId);
                            cmd.ExecuteNonQuery();
                        }
                        using (var cmd = new SqlCommand(
                            "DELETE FROM dbo.KURS_Klient WHERE Kod_klienta=@id;", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", clientId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось удалить клиента: " + ex.Message;
                    return false;
                }
            }

            Polzovateli.RemoveAll(p => p.KlientId == clientId);
            Klients.RemoveAll(k => k.Id == clientId);
            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        private static string ValidatePerson(string familiya, string imya, string telefon,
                                             string login, string password, int excludeClientId,
                                             bool passwordOptional = false)
        {
            if (string.IsNullOrWhiteSpace(familiya))
                return "Укажите фамилию";
            if (familiya.Trim().Length > 50)
                return "Фамилия длиннее 50 символов";
            if (string.IsNullOrWhiteSpace(imya))
                return "Укажите имя";
            if (imya.Trim().Length > 50)
                return "Имя длиннее 50 символов";

            var phone = NormalizePhone(telefon);
            if (phone == null)
                return "Телефон должен быть в формате +7XXXXXXXXXX (11 цифр)";

            if (Klients.Any(k => k.Telefon == phone && k.Id != excludeClientId))
                return "Клиент с таким телефоном уже существует";

            if (string.IsNullOrWhiteSpace(login))
                return "Укажите логин";
            login = login.Trim();
            if (login.Length < 3 || login.Length > 50)
                return "Логин должен содержать от 3 до 50 символов";
            if (login.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '.' || ch == '_' || ch == '-')))
                return "В логине допустимы только буквы, цифры, точка, дефис и подчёркивание";
            if (Polzovateli.Any(p => string.Equals(p.Login, login, StringComparison.OrdinalIgnoreCase) &&
                                     p.KlientId != excludeClientId))
                return "Такой логин уже занят";

            if (!passwordOptional || !string.IsNullOrEmpty(password))
            {
                if (string.IsNullOrEmpty(password) || password.Length < 5)
                    return "Пароль должен содержать не менее 5 символов";
                if (password.Length > 100)
                    return "Пароль длиннее 100 символов";
            }

            return null;
        }

        public static string NormalizePhone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length == 11 && (digits[0] == '7' || digits[0] == '8'))
            {
                return "+7" + digits.Substring(1);
            }
            if (digits.Length == 10)
            {
                return "+7" + digits;
            }
            return null;
        }

        // =================================================================
        //  Абонементы
        // =================================================================

        public static TipAbonementa GetTip(int id)
        {
            return Tipy.FirstOrDefault(t => t.Id == id);
        }

        public static Abonement GetActiveMembership(int clientId)
        {
            var today = DateTime.Today;
            return Abonementy
                .Where(a => a.KlientId == clientId && a.End >= today)
                .OrderByDescending(a => a.End)
                .FirstOrDefault();
        }

        public static List<MembershipRow> GetMembershipRows(string search, string status)
        {
            var today = DateTime.Today;
            var query = Abonementy.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLowerInvariant();
                query = query.Where(a =>
                {
                    var k = GetClient(a.KlientId);
                    var t = GetTip(a.TipId);
                    return (k != null && (k.Familiya + " " + k.Imya).ToLowerInvariant().Contains(s)) ||
                           (t != null && t.Nazvanie.ToLowerInvariant().Contains(s));
                });
            }

            if (status == "Действует")
                query = query.Where(a => a.End >= today);
            else if (status == "Истёк")
                query = query.Where(a => a.End < today);

            return query
                .OrderByDescending(a => a.End)
                .Select(a =>
                {
                    var k = GetClient(a.KlientId);
                    var t = GetTip(a.TipId);
                    var active = a.End >= today;
                    return new MembershipRow
                    {
                        Id = a.Id,
                        KlientId = a.KlientId,
                        Client = k != null ? k.Familiya + " " + k.Imya : "—",
                        Type = t != null ? t.Nazvanie : "—",
                        StartText = Fmt.Date(a.Start),
                        EndText = Fmt.Date(a.End),
                        CostText = t != null ? Fmt.Money(t.Stoimost) : "—",
                        Status = active ? "Действует" : "Истёк",
                        IsActive = active
                    };
                })
                .ToList();
        }

        public static bool AddMembership(int clientId, int tipId, DateTime start, out string error)
        {
            error = null;
            var client = GetClient(clientId);
            var tip = GetTip(tipId);
            if (client == null || tip == null)
            {
                error = "Выберите клиента и тип абонемента";
                return false;
            }

            var end = start.AddDays(Math.Max(1, tip.Srok) - 1);
            if (Abonementy.Any(a => a.KlientId == clientId && a.End >= start && a.Start <= end))
            {
                error = "У клиента уже есть абонемент на этот период";
                return false;
            }

            int newId;
            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "INSERT INTO dbo.KURS_Abonement (Kod_klienta, Kod_tipa, Data_nachala, Data_okonchaniya) " +
                        "VALUES (@k, @t, @s, @e); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection))
                    {
                        cmd.Parameters.AddWithValue("@k", clientId);
                        cmd.Parameters.AddWithValue("@t", tipId);
                        cmd.Parameters.AddWithValue("@s", start.Date);
                        cmd.Parameters.AddWithValue("@e", end.Date);
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось оформить абонемент: " + ex.Message;
                    return false;
                }
            }
            else
            {
                newId = Abonementy.Count == 0 ? 1 : Abonementy.Max(a => a.Id) + 1;
            }

            Abonementy.Add(new Abonement
            {
                Id = newId,
                KlientId = clientId,
                TipId = tipId,
                Start = start.Date,
                End = end.Date
            });

            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static bool ExtendMembership(int abonementId, out string error)
        {
            error = null;
            var membership = Abonementy.FirstOrDefault(a => a.Id == abonementId);
            if (membership == null)
            {
                error = "Абонемент не найден";
                return false;
            }

            var tip = GetTip(membership.TipId);
            if (tip == null)
            {
                error = "Тип абонемента не найден";
                return false;
            }

            var baseDate = membership.End >= DateTime.Today ? membership.End : DateTime.Today;
            var newEnd = baseDate.AddDays(Math.Max(1, tip.Srok));

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.KURS_Abonement SET Data_okonchaniya=@e WHERE Kod_abonementa=@id;", connection))
                    {
                        cmd.Parameters.AddWithValue("@e", newEnd.Date);
                        cmd.Parameters.AddWithValue("@id", abonementId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось продлить абонемент: " + ex.Message;
                    return false;
                }
            }

            membership.End = newEnd.Date;
            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        // =================================================================
        //  Расписание и запись
        // =================================================================

        public static Trener GetTrainer(int id)
        {
            return Trenery.FirstOrDefault(t => t.Id == id);
        }

        public static Zal GetHall(int id)
        {
            return Zaly.FirstOrDefault(z => z.Id == id);
        }

        public static Zanyatie GetLesson(int id)
        {
            return Zanyatiya.FirstOrDefault(z => z.Id == id);
        }

        public static List<LessonRow> GetSchedule(DateTime from, DateTime to, int? hallId, int? clientId)
        {
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            return Zanyatiya
                .Where(z => z.Data.Date >= from.Date && z.Data.Date <= to.Date)
                .Where(z => !hallId.HasValue || z.ZalId == hallId.Value)
                .OrderBy(z => z.Data).ThenBy(z => z.Vremya)
                .Select(z => BuildLessonRow(z, clientId))
                .ToList();
        }

        private static LessonRow BuildLessonRow(Zanyatie lesson, int? clientId)
        {
            var trainer = GetTrainer(lesson.TrenerId);
            var hall = GetHall(lesson.ZalId);
            var booked = Poseshcheniya.Count(p => p.ZanyatieId == lesson.Id);
            var capacity = hall != null ? hall.Vmestimost : 0;
            var free = Math.Max(0, capacity - booked);
            var isBooked = clientId.HasValue &&
                           Poseshcheniya.Any(p => p.ZanyatieId == lesson.Id && p.KlientId == clientId.Value);
            var start = lesson.Data.Date + ParseTime(lesson.Vremya);
            var future = start > DateTime.Now;

            return new LessonRow
            {
                Id = lesson.Id,
                When = lesson.Data.ToString("dd.MM, ddd", Ru) + " · " + lesson.Vremya,
                DateText = Fmt.Date(lesson.Data),
                TimeText = lesson.Vremya,
                Title = lesson.Nazvanie,
                Trainer = trainer != null ? trainer.Familiya : "—",
                Hall = hall != null ? hall.Nazvanie : "—",
                Seats = booked + " из " + capacity,
                FreeSeats = free,
                IsBooked = isBooked,
                CanBook = future && !isBooked && free > 0,
                ButtonText = isBooked ? "Записан" : (free <= 0 ? "Мест нет" : "Записаться")
            };
        }

        public static bool Book(int clientId, int lessonId, out string error)
        {
            error = null;
            var lesson = GetLesson(lessonId);
            if (lesson == null)
            {
                error = "Занятие не найдено";
                return false;
            }

            var start = lesson.Data.Date + ParseTime(lesson.Vremya);
            if (start <= DateTime.Now)
            {
                error = "Запись на прошедшее занятие запрещена";
                return false;
            }

            var membership = GetActiveMembership(clientId);
            if (membership == null)
            {
                error = "Нет действующего абонемента. Обратитесь к администратору клуба.";
                return false;
            }

            if (Poseshcheniya.Any(p => p.ZanyatieId == lessonId && p.KlientId == clientId))
            {
                error = "Вы уже записаны на это занятие";
                return false;
            }

            var hall = GetHall(lesson.ZalId);
            var booked = Poseshcheniya.Count(p => p.ZanyatieId == lessonId);
            if (hall != null && booked >= hall.Vmestimost)
            {
                error = "Свободных мест на занятии нет";
                return false;
            }

            int newId;
            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "INSERT INTO dbo.KURS_Poseshchenie (Kod_klienta, Kod_zanyatiya, Data, Otmetka) " +
                        "VALUES (@k, @z, @d, N'Записан'); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection))
                    {
                        cmd.Parameters.AddWithValue("@k", clientId);
                        cmd.Parameters.AddWithValue("@z", lessonId);
                        cmd.Parameters.AddWithValue("@d", DateTime.Today);
                        newId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось записаться: " + ex.Message;
                    return false;
                }
            }
            else
            {
                newId = Poseshcheniya.Count == 0 ? 1 : Poseshcheniya.Max(p => p.Id) + 1;
            }

            Poseshcheniya.Add(new Poseshchenie
            {
                Id = newId,
                KlientId = clientId,
                ZanyatieId = lessonId,
                Data = DateTime.Today,
                Otmetka = "Записан"
            });

            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static bool CancelBooking(int clientId, int lessonId, out string error)
        {
            error = null;
            var booking = Poseshcheniya.FirstOrDefault(p =>
                p.KlientId == clientId && p.ZanyatieId == lessonId);
            if (booking == null)
            {
                error = "Запись не найдена";
                return false;
            }

            var lesson = GetLesson(lessonId);
            if (lesson == null)
            {
                error = "Занятие не найдено";
                return false;
            }

            if (lesson.Data.Date + ParseTime(lesson.Vremya) <= DateTime.Now)
            {
                error = "Нельзя отменить запись на прошедшее занятие";
                return false;
            }

            if (booking.Otmetka != "Записан")
            {
                error = "Посещение уже отмечено, отмена невозможна";
                return false;
            }

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.KURS_Poseshchenie WHERE Kod_poseshcheniya=@id;", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", booking.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось отменить запись: " + ex.Message;
                    return false;
                }
            }

            Poseshcheniya.Remove(booking);
            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static List<BookingRow> GetClientBookings(int clientId)
        {
            var today = DateTime.Today;
            var result = new List<BookingRow>();

            foreach (var booking in Poseshcheniya.Where(p => p.KlientId == clientId))
            {
                var lesson = GetLesson(booking.ZanyatieId);
                if (lesson == null || lesson.Data.Date < today)
                {
                    continue;
                }

                var trainer = GetTrainer(lesson.TrenerId);
                var hall = GetHall(lesson.ZalId);
                var future = lesson.Data.Date + ParseTime(lesson.Vremya) > DateTime.Now;

                result.Add(new BookingRow
                {
                    Id = booking.Id,
                    LessonId = lesson.Id,
                    DateText = Fmt.Date(lesson.Data),
                    TimeText = lesson.Vremya,
                    Title = lesson.Nazvanie,
                    Trainer = trainer != null ? trainer.Familiya : "—",
                    Hall = hall != null ? hall.Nazvanie : "—",
                    Status = booking.Otmetka,
                    CanCancel = future && booking.Otmetka == "Записан"
                });
            }

            return result.OrderBy(r => r.DateText).ThenBy(r => r.TimeText).ToList();
        }

        public static List<VisitRow> GetClientHistory(int clientId, DateTime from, DateTime to)
        {
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            var result = new List<VisitRow>();
            foreach (var booking in Poseshcheniya.Where(p => p.KlientId == clientId))
            {
                var lesson = GetLesson(booking.ZanyatieId);
                if (lesson == null || lesson.Data.Date < from.Date || lesson.Data.Date > to.Date)
                {
                    continue;
                }

                var trainer = GetTrainer(lesson.TrenerId);
                var hall = GetHall(lesson.ZalId);
                result.Add(new VisitRow
                {
                    Id = booking.Id,
                    DateText = Fmt.Date(lesson.Data),
                    TimeText = lesson.Vremya,
                    Title = lesson.Nazvanie,
                    Trainer = trainer != null ? trainer.Familiya : "—",
                    Hall = hall != null ? hall.Nazvanie : "—",
                    Mark = booking.Otmetka
                });
            }

            return result.OrderByDescending(r => r.DateText).ThenByDescending(r => r.TimeText).ToList();
        }

        public static List<LessonRow> GetAdminSchedule(DateTime from, DateTime to, int? hallId)
        {
            return GetSchedule(from, to, hallId, null);
        }

        public static bool SaveLesson(int? id, string title, int trainerId, int hallId,
                                      DateTime date, string time, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(title))
            {
                error = "Укажите название занятия";
                return false;
            }
            if (title.Trim().Length > 100)
            {
                error = "Название длиннее 100 символов";
                return false;
            }
            if (GetTrainer(trainerId) == null)
            {
                error = "Выберите тренера";
                return false;
            }
            if (GetHall(hallId) == null)
            {
                error = "Выберите зал";
                return false;
            }

            TimeSpan parsed;
            if (!TimeSpan.TryParseExact(time, @"hh\:mm", CultureInfo.InvariantCulture, out parsed) &&
                !TimeSpan.TryParse(time, CultureInfo.InvariantCulture, out parsed))
            {
                error = "Время должно быть в формате ЧЧ:ММ";
                return false;
            }
            if (parsed < TimeSpan.Zero || parsed >= TimeSpan.FromDays(1))
            {
                error = "Некорректное время занятия";
                return false;
            }

            var normalizedTime = parsed.ToString(@"hh\:mm");
            var sameTime = Zanyatiya.Where(z => z.Data.Date == date.Date &&
                                                z.Vremya == normalizedTime &&
                                                (!id.HasValue || z.Id != id.Value));
            if (sameTime.Any(z => z.ZalId == hallId))
            {
                error = "В выбранном зале уже есть занятие в это время";
                return false;
            }
            if (sameTime.Any(z => z.TrenerId == trainerId))
            {
                error = "У выбранного тренера уже есть занятие в это время";
                return false;
            }

            if (id.HasValue)
            {
                var lesson = GetLesson(id.Value);
                if (lesson == null)
                {
                    error = "Занятие не найдено";
                    return false;
                }

                if (IsOnline)
                {
                    try
                    {
                        using (var connection = OpenSql())
                        using (var cmd = new SqlCommand(
                            "UPDATE dbo.KURS_Zanyatie SET Kod_trenera=@tr, Kod_zala=@z, Data=@d, " +
                            "Vremya=@v, Nazvanie=@n WHERE Kod_zanyatiya=@id;", connection))
                        {
                            cmd.Parameters.AddWithValue("@tr", trainerId);
                            cmd.Parameters.AddWithValue("@z", hallId);
                            cmd.Parameters.AddWithValue("@d", date.Date);
                            cmd.Parameters.AddWithValue("@v", parsed);
                            cmd.Parameters.AddWithValue("@n", title.Trim());
                            cmd.Parameters.AddWithValue("@id", id.Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        error = "Не удалось изменить занятие: " + ex.Message;
                        return false;
                    }
                }

                lesson.TrenerId = trainerId;
                lesson.ZalId = hallId;
                lesson.Data = date.Date;
                lesson.Vremya = normalizedTime;
                lesson.Nazvanie = title.Trim();
            }
            else
            {
                int newId;
                if (IsOnline)
                {
                    try
                    {
                        using (var connection = OpenSql())
                        using (var cmd = new SqlCommand(
                            "INSERT INTO dbo.KURS_Zanyatie (Kod_trenera, Kod_zala, Data, Vremya, Nazvanie) " +
                            "VALUES (@tr, @z, @d, @v, @n); SELECT CAST(SCOPE_IDENTITY() AS INT);", connection))
                        {
                            cmd.Parameters.AddWithValue("@tr", trainerId);
                            cmd.Parameters.AddWithValue("@z", hallId);
                            cmd.Parameters.AddWithValue("@d", date.Date);
                            cmd.Parameters.AddWithValue("@v", parsed);
                            cmd.Parameters.AddWithValue("@n", title.Trim());
                            newId = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }
                    catch (Exception ex)
                    {
                        error = "Не удалось добавить занятие: " + ex.Message;
                        return false;
                    }
                }
                else
                {
                    newId = Zanyatiya.Count == 0 ? 1 : Zanyatiya.Max(z => z.Id) + 1;
                }

                Zanyatiya.Add(new Zanyatie
                {
                    Id = newId,
                    TrenerId = trainerId,
                    ZalId = hallId,
                    Data = date.Date,
                    Vremya = normalizedTime,
                    Nazvanie = title.Trim()
                });
            }

            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static bool DeleteLesson(int lessonId, out string error)
        {
            error = null;
            if (Poseshcheniya.Any(p => p.ZanyatieId == lessonId))
            {
                error = "Нельзя удалить занятие: на него записаны клиенты";
                return false;
            }

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "DELETE FROM dbo.KURS_Zanyatie WHERE Kod_zanyatiya=@id;", connection))
                    {
                        cmd.Parameters.AddWithValue("@id", lessonId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось удалить занятие: " + ex.Message;
                    return false;
                }
            }

            Zanyatiya.RemoveAll(z => z.Id == lessonId);
            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        public static List<LessonBookingRow> GetLessonBookings(int lessonId)
        {
            var result = new List<LessonBookingRow>();
            foreach (var booking in Poseshcheniya.Where(p => p.ZanyatieId == lessonId))
            {
                var client = GetClient(booking.KlientId);
                result.Add(new LessonBookingRow
                {
                    Id = booking.Id,
                    Client = client != null ? client.Familiya + " " + client.Imya : "—",
                    Phone = client != null ? client.Telefon : "—",
                    Mark = booking.Otmetka
                });
            }
            return result.OrderBy(r => r.Client).ToList();
        }

        public static bool SetVisitStatus(int posesId, string status, out string error)
        {
            error = null;
            if (!Marks.Contains(status))
            {
                error = "Недопустимая отметка";
                return false;
            }

            var booking = Poseshcheniya.FirstOrDefault(p => p.Id == posesId);
            if (booking == null)
            {
                error = "Запись не найдена";
                return false;
            }

            if (IsOnline)
            {
                try
                {
                    using (var connection = OpenSql())
                    using (var cmd = new SqlCommand(
                        "UPDATE dbo.KURS_Poseshchenie SET Otmetka=@o WHERE Kod_poseshcheniya=@id;", connection))
                    {
                        cmd.Parameters.AddWithValue("@o", status);
                        cmd.Parameters.AddWithValue("@id", posesId);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    error = "Не удалось сохранить отметку: " + ex.Message;
                    return false;
                }
            }

            booking.Otmetka = status;
            if (!IsOnline)
            {
                SaveLocal();
            }
            return true;
        }

        // =================================================================
        //  Отчёты и показатели
        // =================================================================

        public static int CountClients { get { return Klients.Count; } }

        public static int CountActiveMemberships
        {
            get
            {
                var today = DateTime.Today;
                return Abonementy.Count(a => a.End >= today);
            }
        }

        public static int CountLessons { get { return Zanyatiya.Count; } }
        public static int CountTrainers { get { return Trenery.Count; } }

        public static List<AttendanceRow> AttendanceReport(DateTime from, DateTime to)
        {
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            var result = new List<AttendanceRow>();
            foreach (var lesson in Zanyatiya
                .Where(z => z.Data.Date >= from.Date && z.Data.Date <= to.Date)
                .OrderBy(z => z.Data).ThenBy(z => z.Vremya))
            {
                var bookings = Poseshcheniya.Where(p => p.ZanyatieId == lesson.Id).ToList();
                var trainer = GetTrainer(lesson.TrenerId);
                var hall = GetHall(lesson.ZalId);
                result.Add(new AttendanceRow
                {
                    DateText = Fmt.Date(lesson.Data),
                    TimeText = lesson.Vremya,
                    Title = lesson.Nazvanie,
                    Trainer = trainer != null ? trainer.Familiya : "—",
                    Hall = hall != null ? hall.Nazvanie : "—",
                    Booked = bookings.Count,
                    Came = bookings.Count(p => p.Otmetka == "Пришёл"),
                    Missed = bookings.Count(p => p.Otmetka == "Не пришёл")
                });
            }
            return result;
        }

        public static List<SalesRow> SalesReport(DateTime from, DateTime to)
        {
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            var result = new List<SalesRow>();
            foreach (var membership in Abonementy
                .Where(a => a.Start.Date >= from.Date && a.Start.Date <= to.Date)
                .OrderBy(a => a.Start))
            {
                var client = GetClient(membership.KlientId);
                var tip = GetTip(membership.TipId);
                result.Add(new SalesRow
                {
                    DateText = Fmt.Date(membership.Start),
                    Client = client != null ? client.Familiya + " " + client.Imya : "—",
                    Type = tip != null ? tip.Nazvanie : "—",
                    AmountText = tip != null ? Fmt.Money(tip.Stoimost) : "—"
                });
            }
            return result;
        }

        public static decimal SalesTotal(DateTime from, DateTime to)
        {
            if (to < from)
            {
                var tmp = from;
                from = to;
                to = tmp;
            }

            return Abonementy
                .Where(a => a.Start.Date >= from.Date && a.Start.Date <= to.Date)
                .Select(a => GetTip(a.TipId))
                .Where(t => t != null)
                .Sum(t => t.Stoimost);
        }

        public static TimeSpan ParseTime(string value)
        {
            TimeSpan result;
            if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out result) ||
                TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }

        public static List<Zanyatie> GetLessonsForPeriod(DateTime from, DateTime to)
        {
            return Zanyatiya
                .Where(z => z.Data.Date >= from.Date && z.Data.Date <= to.Date)
                .OrderBy(z => z.Data).ThenBy(z => z.Vremya)
                .ToList();
        }
    }
}
