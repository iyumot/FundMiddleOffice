﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace FMO.AutoViewModelGenerator;

[Generator]
public class AutoChangeableViewModelGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) => GetTargetClass(ctx))
            .Where(static c => c is not null);

        context.RegisterSourceOutput(provider, static (ctx, source) => GenerateCode(ctx, source!));
    }

    private static ClassWithAttributeData? GetTargetClass(GeneratorSyntaxContext context)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;

        if (classSymbol is null) return null;

        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is "AutoChangeableViewModelAttribute" or "AutoChangeableViewModel")
            {
                var targetType = (INamedTypeSymbol)attribute.ConstructorArguments[0].Value!;
                return new ClassWithAttributeData(classSyntax, classSymbol, targetType);
            }
        }

        return null;
    }

    private static void GenerateCode(SourceProductionContext context, ClassWithAttributeData data)
    {
        var (classSyntax, classSymbol, targetType) = data;

        var targetTypeName = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var properties = targetType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && !p.IsStatic)
            .ToList();

        var source = $$"""
            // <auto-generated/>
            #nullable enable
            using System.ComponentModel;
            using System.Runtime.CompilerServices;

            namespace {{classSymbol.ContainingNamespace.ToDisplayString()}}
            {
                public partial class {{classSymbol.Name}} : INotifyPropertyChanged
                {
                    private readonly {{targetTypeName}} _target;

                    public {{classSymbol.Name}}({{targetTypeName}} target)
                    {
                        _target = target;
                    }

                    public event PropertyChangedEventHandler? PropertyChanged;

                    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }

            
                    public {{targetTypeName}} Build() => _target;
            {{GetPropertiesCode(properties)}}
                }
            }
            """;

        context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
    }


    private static string GetPropertiesCode(IEnumerable<IPropertySymbol> properties)
    {
        var sb = new StringBuilder();

        foreach (var prop in properties)
        {
            var propType = prop.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var propName = prop.Name;

            sb.AppendLine($$"""
                        public {{propType}} {{propName}}
                        {
                            get => _target.{{propName}};
                            set
                            {
                                if (!EqualityComparer<{{propType}}>.Default.Equals(_target.{{propName}}, value))
                                {
                                    _target.{{propName}} = value;
                                    OnPropertyChanged();
                                }
                            }
                        }
                """);
        }

        return sb.ToString();
    }

    private sealed record ClassWithAttributeData(ClassDeclarationSyntax Syntax, INamedTypeSymbol ClassSymbol, INamedTypeSymbol TargetType);
}