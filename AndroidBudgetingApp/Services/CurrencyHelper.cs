using System;
using System.Collections.Generic;

namespace AndroidBudgetingApp.Services;

public static class CurrencyHelper
{
    public static string CurrentCurrency { get; set; } = "PHP";

    private static readonly Dictionary<string, (string Symbol, decimal Rate)> Currencies = new()
    {
        { "PHP", ("₱", 1m) },
        { "USD", ("$", 58m) },
        { "CNY", ("¥", 8m) },
        { "JPY", ("¥", 0.38m) },
        { "KRW", ("₩", 0.043m) }
    };

    public static string Format(decimal amountInPhp)
    {
        if (!Currencies.TryGetValue(CurrentCurrency, out var curr))
            curr = Currencies["PHP"];

        var converted = amountInPhp / curr.Rate;
        return $"{curr.Symbol}{Math.Abs(converted):N2}";
    }

    public static string FormatPlain(decimal amountInPhp)
    {
        if (!Currencies.TryGetValue(CurrentCurrency, out var curr))
            curr = Currencies["PHP"];

        var converted = amountInPhp / curr.Rate;
        return Math.Abs(converted).ToString("G");
    }

    public static decimal ParseToPhp(decimal amountInCurrentCurrency)
    {
        if (!Currencies.TryGetValue(CurrentCurrency, out var curr))
            curr = Currencies["PHP"];

        return amountInCurrentCurrency * curr.Rate;
    }
}
