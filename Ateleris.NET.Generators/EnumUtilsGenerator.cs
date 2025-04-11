
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
        if (context.Node is not EnumDeclarationSyntax enumDeclaration) return null;
        var semanticModel = context.SemanticModel;
        if (semanticModel.GetDeclaredSymbol(enumDeclaration) is not INamedTypeSymbol enumSymbol) return null;

        var attributeData = enumSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString().EndsWith("GenerateEnumUtils") == true);
        if (attributeData is null) return null;

        // Extract attribute parameters
        bool useDefaultValue = false;
        string? defaultValue = null;
        string stringCaseConversion = "None";
        string comparisonCaseConversion = "None";

        // Read default behavior from attribute
        if (attributeData.ConstructorArguments.Length > 0)
        {
            if (attributeData.ConstructorArguments[0].Kind == TypedConstantKind.Enum)
            {
                // First constructor (with DefaultValueBehavior)
                int defaultBehaviorValue = (int)attributeData.ConstructorArguments[0].Value!;
                useDefaultValue = defaultBehaviorValue == 1; // UseDefaultValue = 1

                if (attributeData.ConstructorArguments.Length > 1)
                    stringCaseConversion = attributeData.ConstructorArguments[1].Value?.ToString() ?? "None";

                if (attributeData.ConstructorArguments.Length > 2)
                    comparisonCaseConversion = attributeData.ConstructorArguments[2].Value?.ToString() ?? "None";
            }
            else
            {
                // Second constructor (with default value)
                useDefaultValue = true;
                defaultValue = attributeData.ConstructorArguments[0].Value?.ToString() ?? "default";

                if (attributeData.ConstructorArguments.Length > 1)
                    stringCaseConversion = attributeData.ConstructorArguments[1].Value?.ToString() ?? "None";

                if (attributeData.ConstructorArguments.Length > 2)
                    comparisonCaseConversion = attributeData.ConstructorArguments[2].Value?.ToString() ?? "None";
            }
        }

        // Extract named parameters if any
        foreach (var namedArg in attributeData.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "DefaultBehavior":
                    useDefaultValue = (int)namedArg.Value.Value! == 1;
                    break;
                case "DefaultValue":
                    useDefaultValue = true;
                    defaultValue = namedArg.Value.Value?.ToString() ?? "default";
                    break;
                case "StringCaseConversion":
                    stringCaseConversion = namedArg.Value.Value?.ToString() ?? "None";
                    break;
                case "ComparisonCaseConversion":
                    comparisonCaseConversion = namedArg.Value.Value?.ToString() ?? "None";
                    break;
            }
        }

        var members = new List<(string Name, int Value)>();
        foreach (var member in enumSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (member.ConstantValue is int value)
            {
                members.Add((member.Name, value));
            }
        }

        return new EnumUtilInfo(
            enumSymbol.Name,
            enumSymbol.ContainingNamespace.ToDisplayString(),
            members,
            useDefaultValue,
            defaultValue,
            stringCaseConversion,
            comparisonCaseConversion);
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
        string applyStringCase = GetCaseConversionMethod(info.StringCaseConversion);
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
            string nameCased = $"\"{name}\"{applyStringCase}";
            sb.AppendLine($@"            {nameCased} => {info.EnumName}.{name},");
        }

        // Handle default case
        if (info.UseDefaultValue && info.DefaultValue != null)
        {
            sb.AppendLine($@"            _ => {info.EnumName}.{info.DefaultValue}");
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
            sb.AppendLine($@"            {info.EnumName}.{name} => ""{name}""{applyStringCase},");
        }

        // Handle default case for ToString
        sb.AppendLine($@"            _ => throw new ArgumentOutOfRangeException(nameof(value), value, ""Value is not a valid {info.EnumName}."")");

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
}
