using System;

namespace RequeueRelief;

[AttributeUsage(AttributeTargets.Field)]
public class ConfigDescriptionAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}

[AttributeUsage(AttributeTargets.Field)]
public class ConfigValidRangeAttribute : Attribute
{
    public ConfigValidRangeAttribute(long min, long max)
    {
        MinLong = min;
        MaxLong = max;
        MinDouble = min;
        MaxDouble = max;
    }

    public ConfigValidRangeAttribute(ulong min, ulong max)
    {
        MinLong = (long) min;
        MaxLong = (long) max;
        MinDouble = min;
        MaxDouble = max;
    }

    public ConfigValidRangeAttribute(double min, double max)
    {
        MinLong = (long) min;
        MaxLong = (long) max;
        MinDouble = min;
        MaxDouble = max;
    }

    public long MinLong { get; }
    public long MaxLong { get; }

    public ulong MinULong => (ulong)MinLong;
    public ulong MaxULong => (ulong) MaxLong;

    public double MinDouble { get; }
    public double MaxDouble { get; }
}

[AttributeUsage(AttributeTargets.Field)]
public class ConfigUnitsAttribute(string singular, string plural) : Attribute
{
    public string Singular { get; } = singular;
    public string Plural { get; } = plural;

    public string Resolve(long value) => value == 1 ? Singular : Plural;
    public string Resolve(ulong value) => value == 1 ? Singular : Plural;
    // ReSharper disable once CompareOfFloatsByEqualityOperator
    public string Resolve(double value) => value == 1.0 ? Singular : Plural;
}