using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VibeShoot.Models.Entities;

namespace VibeShoot.Services
{
    public static class BookingRules
    {
        public const int MaxBookingsPerDay = 2;
        public static readonly TimeSpan PrepInterval = TimeSpan.FromHours(2);
        public static readonly TimeSpan OpeningTime = TimeSpan.FromHours(8);
        public static readonly TimeSpan ClosingTime = TimeSpan.FromHours(20);
        public const decimal DownPaymentRate = 0.5m;

        public static decimal DownPaymentFor(decimal price) => Math.Round(price * DownPaymentRate, 2);

        /// <summary>Returns an error message if the requested slot clashes with existing sessions that day, otherwise null.</summary>
        public static string? CheckSlot(TimeSpan start, TimeSpan end, IReadOnlyCollection<Booking> sameDayActive)
        {
            if (start < OpeningTime || end > ClosingTime)
                return "Sessions must start and finish within operating hours (8:00 AM – 8:00 PM).";

            if (sameDayActive.Count >= MaxBookingsPerDay)
                return $"Sorry! This date has reached the maximum of {MaxBookingsPerDay} bookings.";

            foreach (var b in sameDayActive)
            {
                bool clear = start >= b.EndTime + PrepInterval || end + PrepInterval <= b.StartTime;
                if (!clear)
                    return $"That time is too close to another session ({b.TimeRange}). We need a 2-hour preparation interval between shoots.";
            }
            return null;
        }

        /// <summary>Normalises a PH mobile number to 09XXXXXXXXX; returns null if it is not valid.</summary>
        public static string? NormalizeMobile(string? input)
        {
            var digits = Regex.Replace(input ?? "", @"\D", "");
            if (digits.StartsWith("63") && digits.Length == 12) digits = "0" + digits[2..];
            return Regex.IsMatch(digits, @"^09\d{9}$") ? digits : null;
        }

        public static string FormatMobile(string digits) =>
            digits.Length == 11 ? $"{digits[..4]}-{digits[4..7]}-{digits[7..]}" : digits;

        /// <summary>GCash reference numbers are 13 digits; accept 8-20 digits to allow other e-wallets/banks.</summary>
        public static string? NormalizeReference(string? input)
        {
            var digits = Regex.Replace(input ?? "", @"\s", "");
            return Regex.IsMatch(digits, @"^\d{8,20}$") ? digits : null;
        }
    }
}
