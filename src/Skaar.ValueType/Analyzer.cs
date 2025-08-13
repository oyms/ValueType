using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Skaar.ValueType;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor InvalidVisibility = new(
        id: "VALUETYPE001",
        title: "Invalid Visibility",
        messageFormat:
        $"Structs decorated with the [{Generator.AttributeName}] attribute must be public or internal.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);    
    
    private static readonly DiagnosticDescriptor IsNestedType = new(
        id: "VALUETYPE002",
        title: "Nested type",
        messageFormat:
        $"Structs decorated with the [{Generator.AttributeName}] cannot be nested in another type.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);   
    
    private static readonly DiagnosticDescriptor IsNotPartialType = new(
        id: "VALUETYPE003",
        title: "Not partial",
        messageFormat:
        $"Structs decorated with the [{Generator.AttributeName}] must be partial.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);    
    
    private static readonly DiagnosticDescriptor IsRecordType = new(
        id: "VALUETYPE004",
        title: "Record struct",
        messageFormat:
        $"Record structs decorated with the [{Generator.AttributeName}] are not supported.",
        category: "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [InvalidVisibility, IsNestedType, IsNotPartialType, IsRecordType];

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(AnalyzeDecoratedSymbol(IsPublicOrPrivate), SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeDecoratedSymbol(IsNested), SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeDecoratedSymbol(IsNotPartial), SyntaxKind.StructDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeDecoratedSymbol(IsRecordStruct), SyntaxKind.RecordStructDeclaration);
    }

    private void IsRecordStruct(SyntaxNodeAnalysisContext context, ISymbol symbol, TypeDeclarationSyntax node)
    {
        context.ReportDiagnostic(Diagnostic.Create(
            IsRecordType,
            node.Identifier.GetLocation()
        ));
    }
    
    private void IsPublicOrPrivate(SyntaxNodeAnalysisContext context, ISymbol symbol, TypeDeclarationSyntax node)
    {
        var isPublicOrInternal = symbol.DeclaredAccessibility == Accessibility.Public ||
                                 symbol.DeclaredAccessibility == Accessibility.Internal;
        var isFileScoped = node.Modifiers.Any(m => m.IsKind(SyntaxKind.FileKeyword));
        if (!isPublicOrInternal || isFileScoped)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidVisibility,
                node.Identifier.GetLocation()
            ));
        }
    }
    private void IsNested(SyntaxNodeAnalysisContext context, ISymbol symbol, TypeDeclarationSyntax node)
    {
        var isNested = symbol.ContainingType != null;
        if (isNested)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                IsNestedType,
                node.Identifier.GetLocation()
            ));
        }
    }
        
    private void IsNotPartial(SyntaxNodeAnalysisContext context, ISymbol symbol, TypeDeclarationSyntax node)
    {
        var isPartial = node.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
        if (!isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                IsNotPartialType,
                node.Identifier.GetLocation()
            ));
        }
    }
    
    
    private Action<SyntaxNodeAnalysisContext> AnalyzeDecoratedSymbol(Action<SyntaxNodeAnalysisContext, ISymbol, TypeDeclarationSyntax> action)
    {
        return ctx =>
        {
            var typeDeclaration = (TypeDeclarationSyntax)ctx.Node;

            if (!typeDeclaration.AttributeLists.Any())
                return;

            var model = ctx.SemanticModel;
            var symbol = ModelExtensions.GetDeclaredSymbol(model, typeDeclaration);
            if (symbol == null)
                return;

            var hasAttribute = symbol.GetAttributes()
                .Any(attr => attr.AttributeClass?.ToDisplayString() ==
                             $"{Generator.AttributeNamespace}.{Generator.AttributeName}");
            if (!hasAttribute)
                return;
            action.Invoke(ctx, symbol, typeDeclaration);
        };
    }

    private void AnalyzeDecoratedStruct(SyntaxNodeAnalysisContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;

        if (!typeDeclaration.AttributeLists.Any())
            return;

        var model = context.SemanticModel;
        var symbol = ModelExtensions.GetDeclaredSymbol(model, typeDeclaration);
        if (symbol == null)
            return;

        var hasAttribute = symbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() ==
                         $"{Generator.AttributeNamespace}.{Generator.AttributeName}");
        if (!hasAttribute)
            return;

        var isPublicOrInternal = symbol.DeclaredAccessibility == Accessibility.Public ||
                                 symbol.DeclaredAccessibility == Accessibility.Internal;

        var isPartial = typeDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        if (!isPublicOrInternal || !isPartial)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidVisibility,
                typeDeclaration.Identifier.GetLocation()
            ));
        }
    }
}