using System.Globalization;

namespace Nro.Tests.Helpers;

/// <summary>
/// A culture that differs from the invariant one in everything that could leak into number and date text: minus/plus signs, decimal and grouping separators, native
/// digits, date/time separators, AM/PM and month/day names. (The solution runs with InvariantGlobalization, so real cultures such as vi-VN do not exist; this one is built by hand.)
/// Code that formats numbers or dates with the current culture would produce different text under it.
/// </summary>
internal static class HostileCulture
{
    public static CultureInfo Create()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();

        NumberFormatInfo number = culture.NumberFormat;
        number.NegativeSign = "~";
        number.PositiveSign = "#";
        number.NumberDecimalSeparator = ",";
        number.NumberGroupSeparator = ".";
        number.NumberGroupSizes = [1];
        number.NativeDigits = ["٠", "١", "٢", "٣", "٤", "٥", "٦", "٧", "٨", "٩"];
        number.DigitSubstitution = DigitShapes.NativeNational;

        DateTimeFormatInfo date = culture.DateTimeFormat;
        date.DateSeparator = ".";
        date.TimeSeparator = "-";
        date.AMDesignator = "SA";
        date.PMDesignator = "CH";
        date.MonthNames = ["Thang 1", "Thang 2", "Thang 3", "Thang 4", "Thang 5", "Thang 6", "Thang 7", "Thang 8", "Thang 9", "Thang 10", "Thang 11", "Thang 12", string.Empty];
        date.AbbreviatedMonthNames = ["T1", "T2", "T3", "T4", "T5", "T6", "T7", "T8", "T9", "T10", "T11", "T12", string.Empty];
        date.DayNames = ["Chu nhat", "Thu hai", "Thu ba", "Thu tu", "Thu nam", "Thu sau", "Thu bay"];
        date.AbbreviatedDayNames = ["CN", "T2", "T3", "T4", "T5", "T6", "T7"];
        return culture;
    }

    /// <summary>Runs <paramref name="action"/> with the hostile culture as current culture and UI culture, then restores the previous ones.</summary>
    public static void Run(Action action)
    {
        CultureInfo saved = CultureInfo.CurrentCulture;
        CultureInfo savedUi = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo hostile = Create();
            CultureInfo.CurrentCulture = hostile;
            CultureInfo.CurrentUICulture = hostile;
            action();
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
            CultureInfo.CurrentUICulture = savedUi;
        }
    }
}
