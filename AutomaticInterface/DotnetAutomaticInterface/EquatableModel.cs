using System;
using Microsoft.CodeAnalysis;

namespace DotnetAutomaticInterface;

/// <summary>
///     This is just a container to enable Roselyn to check whether a Class changed and needs to retrigger src gen.
///     Easiest approach is here to just compare the entire Syntax. This is more than we need, as we e.g. don't care
///     about implementation or private members. But at least it guarantees recompilation on change. But only for that
///     file, and not all files with that CodeGen Attribute as before! So while we do some expensive comparisons, we
///     avoid arguably more expensive IO of writing to a file.
/// </summary>
/// <param name="typeSymbol">The cls symbol</param>
/// <param name="classSyntax">The cls syntax</param>
public sealed class EquatableModel(ITypeSymbol typeSymbol, SyntaxNode classSyntax)
    : IEquatable<EquatableModel>
{
    public ITypeSymbol TypeSymbol { get; } = typeSymbol;

    public SyntaxNode ClassSyntax { get; } = classSyntax;

    public bool Equals(EquatableModel? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // AutomaticInterfaceGenerator.Log(ClassSyntax.ToString());
        return ClassSyntax.IsEquivalentTo(other.ClassSyntax);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is EquatableModel other && Equals(other);
    }

    public override int GetHashCode()
    {
        return ClassSyntax.GetHashCode();
    }
}
