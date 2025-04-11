namespace Ateleris.NET.Shared.Attributes;

using System;

[System.AttributeUsage(System.AttributeTargets.Enum)]
public class GenerateEnumUtils : System.Attribute
{
    public enum DefaultValueBehavior
    {
        ThrowException,
        UseDefaultValue
    }

    public enum CaseConversion
    {
        None,
        ToUpper,
        ToLower
    }

    public DefaultValueBehavior DefaultBehavior { get; }
    public object? DefaultValue { get; }
    public CaseConversion StringCaseConversion { get; }
    public CaseConversion ComparisonCaseConversion { get; }

    public GenerateEnumUtils(
        DefaultValueBehavior defaultBehavior = DefaultValueBehavior.ThrowException,
        CaseConversion stringCaseConversion = CaseConversion.ToUpper,
        CaseConversion comparisonCaseConversion = CaseConversion.None)
    {
        DefaultBehavior = defaultBehavior;
        DefaultValue = null;
        StringCaseConversion = stringCaseConversion;
        ComparisonCaseConversion = comparisonCaseConversion;
    }

    public GenerateEnumUtils(
        object defaultValue,
        CaseConversion stringCaseConversion = CaseConversion.ToUpper,
        CaseConversion comparisonCaseConversion = CaseConversion.None)
    {
        DefaultBehavior = DefaultValueBehavior.UseDefaultValue;
        DefaultValue = defaultValue;
        StringCaseConversion = stringCaseConversion;
        ComparisonCaseConversion = comparisonCaseConversion;
    }
}
