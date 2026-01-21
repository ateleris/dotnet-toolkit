using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Ateleris.NET.Generators;

public readonly record struct EnumUtilInfo(
    string EnumName,
    string EnumNamespace,
    List<(string Name, int Value)> EnumMembers,
    bool UseDefaultValue,
    string? DefaultValue,
    string StringCaseConversion,
    string ComparisonCaseConversion);

[Generator]
public class EnumUtilGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<EnumUtilInfo?> enums = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetEnumInfo(ctx))
            .Where(static m => m is not null);
        context.RegisterSourceOutput(enums, static (spc, source) => Execute(source, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is EnumDeclarationSyntax m && m.AttributeLists.Count > 0;

    private static EnumUtilInfo? GetEnumInfo(GeneratorSyntaxContext context)
    {
        var enumDeclaration = (EnumDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;

        if (semanticModel.GetDeclaredSymbol(enumDeclaration) is not INamedTypeSymbol enumSymbol)
        {
            return null;
        }

        var generateEnumUtilsAttribute = enumSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name == "GenerateEnumUtils" &&
                                   attr.AttributeClass.ContainingNamespace.ToDisplayString() == "Ateleris.NET.Shared.Attributes");

        if (generateEnumUtilsAttribute is null)
        {
            return null;
        }

        bool useDefaultValue = false;
        string? defaultValue = null;
        string stringCaseConversion = "ToUpper";
        string comparisonCaseConversion = "None";

        if (generateEnumUtilsAttribute.ConstructorArguments.Length > 0)
        {
            var firstArg = generateEnumUtilsAttribute.ConstructorArguments[0];

            if (firstArg.Type?.Name == "DefaultValueBehavior")
            {
                useDefaultValue = (int)firstArg.Value! == 1;

                if (generateEnumUtilsAttribute.ConstructorArguments.Length > 1)
                {
                    var stringCaseArg = generateEnumUtilsAttribute.ConstructorArguments[1];
                    stringCaseConversion = GetCaseConversionString((int)stringCaseArg.Value!);
                }

                if (generateEnumUtilsAttribute.ConstructorArguments.Length > 2)
                {
                    var comparisonCaseArg = generateEnumUtilsAttribute.ConstructorArguments[2];
                    comparisonCaseConversion = GetCaseConversionString((int)comparisonCaseArg.Value!);
                }
            }
            else
            {
                useDefaultValue = true;
                defaultValue = firstArg.Value?.ToString();

                if (generateEnumUtilsAttribute.ConstructorArguments.Length > 1)
                {
                    var stringCaseArg = generateEnumUtilsAttribute.ConstructorArguments[1];
                    stringCaseConversion = GetCaseConversionString((int)stringCaseArg.Value!);
                }

                if (generateEnumUtilsAttribute.ConstructorArguments.Length > 2)
                {
                    var comparisonCaseArg = generateEnumUtilsAttribute.ConstructorArguments[2];
                    comparisonCaseConversion = GetCaseConversionString((int)comparisonCaseArg.Value!);
                }
            }
        }

        var enumMembers = new List<(string Name, int Value)>();
        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.IsStatic && member.HasConstantValue)
            {
                var value = Convert.ToInt32(member.ConstantValue);
                enumMembers.Add((member.Name, value));
            }
        }

        var enumNamespace = enumSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : enumSymbol.ContainingNamespace.ToDisplayString();

        return new EnumUtilInfo(
            EnumName: enumSymbol.Name,
            EnumNamespace: enumNamespace,
            EnumMembers: enumMembers,
            UseDefaultValue: useDefaultValue,
            DefaultValue: defaultValue,
            StringCaseConversion: stringCaseConversion,
            ComparisonCaseConversion: comparisonCaseConversion
        );
    }

    private static string GetCaseConversionString(int value)
    {
        return value switch
        {
            0 => "None",
            1 => "ToUpper",
            2 => "ToLower",
            _ => "None"
        };
    }

    private static void Execute(EnumUtilInfo? enumInfo, SourceProductionContext context)
    {
        if (enumInfo is { } info)
        {
            string result = GenerateEnumUtils(info);
            context.AddSource($"{info.EnumName}Utils.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static string GenerateEnumUtils(EnumUtilInfo info)
    {
        string applyComparisonCase = GetCaseConversionMethod(info.ComparisonCaseConversion);

        var sb = new StringBuilder();
        sb.AppendLine($@"// <auto-generated/>
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace {info.EnumNamespace};
public static class {info.EnumName}Utils
{{
    public static {info.EnumName} FromString(string value)
    {{
        string comparedValue = value{applyComparisonCase};
        return comparedValue switch
        {{");

        foreach (var (name, _) in info.EnumMembers)
        {
            string nameCased = ApplyCaseConversion(name, info.StringCaseConversion);
            sb.AppendLine($@"            ""{nameCased}"" => {info.EnumName}.{name},");
        }

        if (info.UseDefaultValue && info.DefaultValue != null)
        {
            sb.AppendLine($@"            _ => ({info.EnumName}){info.DefaultValue}");
        }
        else
        {
            sb.AppendLine($@"            _ => throw new ArgumentOutOfRangeException(nameof(value), value, ""Value is not a valid {info.EnumName} name."")");
        }

        sb.AppendLine($@"        }};
    }}

    public static string ToString({info.EnumName} value)
    {{
        return value switch
        {{");

        foreach (var (name, _) in info.EnumMembers)
        {
            string nameCased = ApplyCaseConversion(name, info.StringCaseConversion);
            sb.AppendLine($@"            {info.EnumName}.{name} => ""{nameCased}"",");
        }

        if (info.UseDefaultValue && info.DefaultValue != null)
        {
            var (Name, Value) = info.EnumMembers.FirstOrDefault(m => m.Value.ToString() == info.DefaultValue);
            if (Name != null)
            {
                string nameCased = ApplyCaseConversion(Name, info.StringCaseConversion);
                sb.AppendLine($@"            _ => ""{nameCased}""");
            }
            else
            {
                sb.AppendLine($@"            _ => throw new ArgumentOutOfRangeException(nameof(value), value, ""Value is not a valid {info.EnumName}."")");
            }
        }
        else
        {
            sb.AppendLine($@"            _ => throw new ArgumentOutOfRangeException(nameof(value), value, ""Value is not a valid {info.EnumName}."")");
        }

        sb.AppendLine($@"        }};
    }}
}}

public class {info.EnumName}Converter : JsonConverter<{info.EnumName}>
{{
    public override {info.EnumName} Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) => {info.EnumName}Utils.FromString(reader.GetString() ?? string.Empty);

    public override void Write(
        Utf8JsonWriter writer,
        {info.EnumName} value,
        JsonSerializerOptions options) => writer.WriteStringValue({info.EnumName}Utils.ToString(value));
}}");

        return sb.ToString();
    }

    private static string GetCaseConversionMethod(string caseConversion)
    {
        return caseConversion switch
        {
            "ToUpper" => ".ToUpper()",
            "ToLower" => ".ToLower()",
            _ => ""
        };
    }

    private static string ApplyCaseConversion(string input, string caseConversion)
    {
        return caseConversion switch
        {
            "ToUpper" => input.ToUpper(),
            "ToLower" => input.ToLower(),
            _ => input
        };
    }
}
