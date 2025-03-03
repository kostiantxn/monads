using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Monads.Generators.Extensions;

/// <summary>
///     Extensions for <see cref="StringBuilder"/>.
/// </summary>
internal static class StringBuilderExtensions
{
#pragma warning disable RS1035
    private static readonly string NewLine = Environment.NewLine;
#pragma warning restore RS1035

    private static readonly string[] NewLines = ["\r\n", "\r", "\n"];

    public static StringBuilder AppendDefinition(this StringBuilder builder, INamedTypeSymbol type)
    {
        if (type.DeclaredAccessibility is not Accessibility.NotApplicable)
        {
            builder
                .Append(SyntaxFacts.GetText(type.DeclaredAccessibility))
                .Append(' ');
        }

        if (type.IsReadOnly)
            builder.Append("readonly ");

        if (type.IsStatic)
            builder.Append("static ");

        builder.Append("partial ");

        if (type.IsRecord)
            builder.Append("record ");

        if (type.TypeKind is TypeKind.Struct)
            builder.Append("struct ");
        else if (type.TypeKind is TypeKind.Class)
            builder.Append("class ");
        else if (type.TypeKind is TypeKind.Interface)
            builder.Append("interface ");

        builder.Append(type.Name);

        if (type.TypeParameters.Length > 0)
        {
            builder
                .Append('<')
                .Append(string.Join(", ", type.TypeParameters))
                .Append('>');
        }

        return builder;
    }

    public static StringBuilder AppendContainingType(this StringBuilder builder, INamedTypeSymbol? type, string contents, string indent = "    ")
    {
        return builder.Append(Recurse(type, contents, indent));

        static string Recurse(INamedTypeSymbol? type, string contents, string indent)
        {
            if (type is null)
                return contents;

            contents = 
                $$"""
                {{type.Definition()}}
                {

                """ +
                string.Join(NewLine, contents.Split(NewLines, StringSplitOptions.None).Select(x => indent + x)) +
                """

                }
                """;

            return Recurse(type.ContainingType, contents, indent);
        }
    }
}