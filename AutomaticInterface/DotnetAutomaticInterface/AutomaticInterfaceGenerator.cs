using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetAutomaticInterface;

[Generator]
public class AutomaticInterfaceGenerator : IIncrementalGenerator
{
    public const string DefaultAttributeName = "GenerateAutomaticInterface";
    public const string IgnoreAutomaticInterfaceAttributeName = "IgnoreAutomaticInterface";
    public const string NamespaceParameterName = "namespaceName";
    public const string InterfaceParameterName = "interfaceName";
    public const string AsInternalParameterName = "asInternal";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterDefaultAttribute();
        context.RegisterIgnoreAttribute();

        var classes = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"DotnetAutomaticInterface.{DefaultAttributeName}Attribute",
            (node, _) => node is ClassDeclarationSyntax,
            (ctx, _) => new EquatableModel((ITypeSymbol)ctx.TargetSymbol, ctx.TargetNode)
        );

        context.RegisterSourceOutput(classes, GenerateCode);
    }

    private static void GenerateCode(SourceProductionContext context, EquatableModel equatableModel)
    {
        var type = equatableModel.TypeSymbol;

        Log($"Trigger for: {type.Name}");

        var typeNamespace = type.ContainingNamespace.IsGlobalNamespace
            ? $"${Guid.NewGuid()}"
            : $"{type.ContainingNamespace}";

        var code = Builder.BuildInterfaceFor(equatableModel);

        var hintName = $"{typeNamespace}.I{type.Name}";
        context.AddSource(hintName, code);
    }

    /// <summary>
    ///     Couldnt get it to disable when running tests so currently just disabled manually...
    /// </summary>
    /// <param name="message"></param>
    /// <param name="member"></param>
    /// <param name="file"></param>
    /// <param name="line"></param>
    public static void Log(
        string message,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    )
    {
        // #if DEBUG
        //         var timestamp = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        //         var source = $"{Path.GetFileName(file)}:{line}";
        //
        //         Debug.WriteLine($"[{timestamp}] [Generator] [{source}] [{member}] {message}");
        // #endif
    }
}
