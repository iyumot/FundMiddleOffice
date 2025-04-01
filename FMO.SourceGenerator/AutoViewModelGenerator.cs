﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace FMO.SourceGenerator
{
    [Generator]
    public class AutoChangeableViewModelGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 查找所有带有 AutoChangeableViewModelAttribute 的类
            IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
               .CreateSyntaxProvider(
                    predicate: (s, _) => IsSyntaxTargetForGeneration(s),
                    transform: (ctx, _) => GetSemanticTargetForGeneration(ctx))
               .Where(m => m != null);

            // 获取编译对象
            IncrementalValueProvider<Compilation> compilation = context.CompilationProvider;

            // 结合编译对象和类声明
            var combined = compilation.Combine(classDeclarations.Collect());

            // 生成代码
            context.RegisterSourceOutput(combined, (spc, source) =>
            {
                Execute(source.Item1, source.Item2, spc);
            });
        }

        private bool IsSyntaxTargetForGeneration(SyntaxNode node)
        {
            return node is ClassDeclarationSyntax classDeclaration &&
                   classDeclaration.AttributeLists.Count > 0;
        }

        private ClassDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            foreach (var attributeList in classDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (context.SemanticModel.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeSymbol &&
                        attributeSymbol.ContainingType.Name == "AutoChangeableViewModelAttribute")
                    {
                        return classDeclaration;
                    }
                }
            }
            return null;
        }

        private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                return;
            }

            var attributeSymbol = compilation.GetTypeByMetadataName("FMO.Shared.AutoChangeableViewModelAttribute");
            if (attributeSymbol == null)
            {
                return;
            }

            foreach (var classDeclaration in classes)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                foreach (var attributeList in classDeclaration.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        if (model.GetSymbolInfo(attribute).Symbol is IMethodSymbol attributeMethodSymbol &&
                            attributeMethodSymbol.ContainingType.Equals(attributeSymbol, SymbolEqualityComparer.Default))
                        {
                            var typeArgument = attribute.ArgumentList.Arguments[0].Expression;
                            if (typeArgument is TypeOfExpressionSyntax typeOfExpression)
                            {
                                var targetTypeSyntax = typeOfExpression.Type;
                                var targetTypeSymbol = model.GetTypeInfo(targetTypeSyntax).Type;

                                if (targetTypeSymbol != null)
                                {
                                    var sourceCode = GenerateSourceCode(classSymbol, targetTypeSymbol);
                                    context.AddSource($"{classSymbol.Name}.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string GenerateSourceCode(INamedTypeSymbol classSymbol, ITypeSymbol targetTypeSymbol)
        {
            var source = new StringBuilder();
            source.AppendLine("// <auto-generated/>");
            source.AppendLine($"using System;");
            source.AppendLine($"using System.ComponentModel;");

            source.AppendLine($"namespace {classSymbol.ContainingNamespace.ToDisplayString()}");
            source.AppendLine("{");
            source.AppendLine($"    public partial class {classSymbol.Name} : INotifyPropertyChanged, IEquatable<{classSymbol.Name}>");
            source.AppendLine("    {");
            source.AppendLine("        public event PropertyChangedEventHandler PropertyChanged;");

            var properties = targetTypeSymbol.GetMembers().OfType<IPropertySymbol>();

            // 构造函数
            source.AppendLine($"        public {classSymbol.Name}({targetTypeSymbol.ToDisplayString()} instance)");
            source.AppendLine("        {");
            source.AppendLine("             if(instance is not null)");
            source.AppendLine("                 {");
            foreach (var property in properties)
                source.AppendLine($"            _{property.Name.ToCamelCase()} = instance.{property.Name};");

            source.AppendLine("                 }");
            source.AppendLine("        }");


            // 构造函数
            source.AppendLine($"        public {classSymbol.Name}()");
            source.AppendLine("        {");
            source.AppendLine("        }");

            foreach (var property in properties)
            {
                string pname = property.Name.ToCamelCase();
                string type = property.Type.ToDisplayString();
                if (type != "bool" && !type.EndsWith("?")) type += "?";

                source.AppendLine($"        private {type} _{property.Name.ToCamelCase()};");
                source.AppendLine($"        public {type} {property.Name}");
                source.AppendLine("        {");
                source.AppendLine($"            get => _{property.Name.ToCamelCase()};");
                source.AppendLine($"            set");
                source.AppendLine("            {");
                source.AppendLine($"                if (!EqualityComparer<{type}>.Default.Equals(_{property.Name.ToCamelCase()}, value))");
                source.AppendLine("                {");
                source.AppendLine($"                   _{property.Name.ToCamelCase()} = value;");
                source.AppendLine($"                   OnPropertyChanged(nameof({property.Name}));");
                source.AppendLine("                }");
                source.AppendLine("            }");
                source.AppendLine("        }");

            }

            // 实现 IEquatable<T> 接口的 Equals 方法
            source.AppendLine($"        public bool Equals({classSymbol.Name} other)");
            source.AppendLine("        {");
            source.AppendLine("            if (other is null) return false;");
            source.AppendLine("            if (ReferenceEquals(this, other)) return true;");
            if (properties.Any())
            {
                var firstProperty = properties.First();
                var type = firstProperty.Type.ToDisplayString();
                if (type != "bool" && !type.EndsWith("?")) type += "?";

                source.AppendLine($"            return EqualityComparer<{type}>.Default.Equals(_{firstProperty.Name.ToCamelCase()}, other._{firstProperty.Name.ToCamelCase()})");
                foreach (var property in properties.Skip(1))
                {
                    type = property.Type.ToDisplayString();
                    if (type != "bool" && !type.EndsWith("?")) type += "?";

                    source.AppendLine($"                && EqualityComparer<{type}>.Default.Equals(_{property.Name.ToCamelCase()}, other._{property.Name.ToCamelCase()})");
                }
                source.AppendLine(";");
            }
            else
            {
                source.AppendLine("            return true;");
            }
            source.AppendLine("        }");

            source.AppendLine("        protected virtual void OnPropertyChanged(string propertyName)");
            source.AppendLine("        {");
            source.AppendLine("            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
            source.AppendLine("        }");

            source.AppendLine($"        public {targetTypeSymbol.ToDisplayString()} Build()");
            source.AppendLine("        {");
            source.AppendLine($"            var result = new {targetTypeSymbol.ToDisplayString()}();");
            foreach (var property in properties)
            {
                var type = property.Type.ToDisplayString();
                if(type != "bool")
                    source.AppendLine($"            result.{property.Name} = _{property.Name.ToCamelCase()}??default;");
                else
                    source.AppendLine($"            result.{property.Name} = _{property.Name.ToCamelCase()};");
            }
            source.AppendLine("            return result;");
            source.AppendLine("        }");

          


            source.AppendLine("    }");
            source.AppendLine("}");

            return source.ToString();
        }
    }

    public static class StringExtensions
    {
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
            {
                return input;
            }

            char[] chars = input.ToCharArray();
            chars[0] = char.ToLowerInvariant(chars[0]);
            return new string(chars);
        }
    }
}