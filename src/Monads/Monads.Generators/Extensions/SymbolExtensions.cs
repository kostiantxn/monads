using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Monads.Generators.Extensions;

/// <summary>
///     Extensions for <see cref="ISymbol"/>.
/// </summary>
internal static class SymbolExtensions
{
    public static string Definition(this INamedTypeSymbol type) =>
        new StringBuilder().AppendDefinition(type).ToString();

    public static bool Implements(this INamedTypeSymbol type, string @interface) =>
        type.AllInterfaces.Any(x => x.ToString() == @interface);
}
