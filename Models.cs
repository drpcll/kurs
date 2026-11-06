using System;
using System.Collections.Generic;
using System.Globalization;

namespace FitnessClub
{
    // =====================================================================
    //  Сущности базы данных (соответствуют таблицам KURS_*)
    // =====================================================================

    public class Klient
    {
        public int Id { get; set; }
        public string Familiya { get; set; }
        public string Imya { get; set; }
        public string Telefon { get; set; }

        public override string ToString()
        {
            return (Familiya + " " + Imya).Trim();
        }
    }

    public class Polzovatel
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        public string Login { get; set; }
        public string Parol { get; set; }
        public string Rol { get; set; }
    }

    public class Administrator
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Parol { get; set; }
        public string Familiya { get; set; }
    }

    public class TipAbonementa
    {
        public int Id { get; set; }
        public string Nazvanie { get; set; }
        public decimal Stoimost { get; set; }
        public int Srok { get; set; }

        public override string ToString()
        {
            return Nazvanie + " · " + Srok + " дн. · " + Fmt.Money(Stoimost);
        }
    }

    public class Abonement
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        public int TipId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class Spetsializatsiya
    {
        public int Id { get; set; }
        public string Nazvanie { get; set; }

        public override string ToString()
        {
            return Nazvanie;
        }
    }

    public class Trener
    {
        public int Id { get; set; }
        public string Familiya { get; set; }
        public int SpetsId { get; set; }
        public int Stazh { get; set; }

        public override string ToString()
        {
            return Familiya;
        }
    }

    public class Zal
    {
        public int Id { get; set; }
        public string Nazvanie { get; set; }
        public int Vmestimost { get; set; }

        public override string ToString()
        {
            return Nazvanie;
        }
    }

    public class Zanyatie
    {
        public int Id { get; set; }
        public int TrenerId { get; set; }
        public int ZalId { get; set; }
        public DateTime Data { get; set; }
        public string Vremya { get; set; }   // "HH:mm"
        public string Nazvanie { get; set; }
    }

    public class Poseshchenie
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        public int ZanyatieId { get; set; }
        public DateTime Data { get; set; }
        public string Otmetka { get; set; }  // Записан / Пришёл / Не пришёл
    }

    /// <summary>Корневой объект автономного хранилища.</summary>
    public class LocalData
    {
        public List<Klient> Klients { get; set; }
        public List<Polzovatel> Polzovateli { get; set; }
        public List<Administrator> Administratory { get; set; }
        public List<TipAbonementa> Tipy { get; set; }
        public List<Abonement> Abonementy { get; set; }
        public List<Spetsializatsiya> Spetsializatsii { get; set; }
        public List<Trener> Trenery { get; set; }
        public List<Zal> Zaly { get; set; }
        public List<Zanyatie> Zanyatiya { get; set; }
        public List<Poseshchenie> Poseshcheniya { get; set; }
    }

    /// <summary>Текущая сессия пользователя.</summary>
    public static class Session
    {
        public static string Role { get; set; }
        public static int KlientId { get; set; }
        public static int AdminId { get; set; }
        public static string DisplayName { get; set; }
        public static string Login { get; set; }

        public static bool IsAdmin { get { return Role == "Admin"; } }
        public static bool IsClient { get { return Role == "Client"; } }
    }

    // =====================================================================
    //  Строки для таблиц и списков интерфейса
    // =====================================================================

    public class LessonRow
    {
        public int Id { get; set; }
        public string When { get; set; }
        public string DateText { get; set; }
        public string TimeText { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public string Hall { get; set; }
        public string Seats { get; set; }
        public int FreeSeats { get; set; }
        public bool IsBooked { get; set; }
        public bool CanBook { get; set; }
        public string ButtonText { get; set; }
    }

    public class ClientRow
    {
        public int Id { get; set; }
        public string Familiya { get; set; }
        public string Imya { get; set; }
        public string Telefon { get; set; }
        public string Abonement { get; set; }
        public string Status { get; set; }
        public string Login { get; set; }
        public string Fio { get { return (Familiya + " " + Imya).Trim(); } }

        public override string ToString()
        {
            return Fio;
        }
    }

    public class BookingRow
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string DateText { get; set; }
        public string TimeText { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public string Hall { get; set; }
        public string Status { get; set; }
        public bool CanCancel { get; set; }
    }

    public class MembershipRow
    {
        public int Id { get; set; }
        public int KlientId { get; set; }
        public string Client { get; set; }
        public string Type { get; set; }
        public string StartText { get; set; }
        public string EndText { get; set; }
        public string CostText { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class VisitRow
    {
        public int Id { get; set; }
        public string DateText { get; set; }
        public string TimeText { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public string Hall { get; set; }
        public string Mark { get; set; }
    }

    public class LessonBookingRow
    {
        public int Id { get; set; }
        public string Client { get; set; }
        public string Phone { get; set; }
        public string Mark { get; set; }
    }

    public class AttendanceRow
    {
        public string DateText { get; set; }
        public string TimeText { get; set; }
        public string Title { get; set; }
        public string Trainer { get; set; }
        public string Hall { get; set; }
        public int Booked { get; set; }
        public int Came { get; set; }
        public int Missed { get; set; }
    }

    public class SalesRow
    {
        public string DateText { get; set; }
        public string Client { get; set; }
        public string Type { get; set; }
        public string AmountText { get; set; }
    }

    /// <summary>Вспомогательные операции форматирования.</summary>
    public static class Fmt
    {
        private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");

        public static string Date(DateTime date)
        {
            return date.ToString("dd.MM.yyyy", Ru);
        }

        public static string DateLong(DateTime date)
        {
            return date.ToString("d MMMM yyyy", Ru);
        }

        public static string Weekday(DateTime date)
        {
            return date.ToString("ddd", Ru);
        }

        public static string Money(decimal value)
        {
            return value.ToString("N0", Ru) + " ₽";
        }

        public static string MoneyPlain(decimal value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
