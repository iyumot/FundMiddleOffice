using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[Generator]
public class VerifyRulesInitGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 监控 VerifyRules 类的声明
        var verifyRulesClass = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsVerifyRulesClass(s),
                transform: static (ctx, _) => GetVerifyRulesProperties(ctx))
            .Where(static result => result != null && result.Count > 0);

        // 注册源代码生成
        context.RegisterSourceOutput(verifyRulesClass,
            static (spc, properties) => GenerateSource(properties, spc));
    }

    private static bool IsVerifyRulesClass(SyntaxNode node)
    {
        // 精确匹配 VerifyRules 类名
        return node is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.Identifier.Text == "VerifyRules";
    }

    private static List<PropertyInfo> GetVerifyRulesProperties(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var compilation = semanticModel.Compilation;

        var properties = new List<PropertyInfo>();

        // 获取 IDataValidation 接口（可选）
        var iDataValidationInterface = compilation.GetTypeByMetadataName("IDataValidation");
        if (iDataValidationInterface == null)
        {
            iDataValidationInterface = FindIDataValidationInterface(compilation);
        }

        // 获取 VerifyRule 基类（可选）
        var verifyRuleBase = compilation.GetTypeByMetadataName("VerifyRule");

        // 遍历类中的所有属性
        foreach (var member in classDeclaration.Members)
        {
            if (member is PropertyDeclarationSyntax property)
            {
                // 检查是否为静态属性
                if (property.Modifiers.Any(m => m.Text == "static"))
                {
                    // 获取属性名称
                    var propertyName = property.Identifier.Text;

                    // 获取属性类型
                    var propertyType = property.Type;
                    var typeSymbol = semanticModel.GetSymbolInfo(propertyType).Symbol as INamedTypeSymbol;

                    if (typeSymbol != null)
                    {
                        var propertyInfo = new PropertyInfo
                        {
                            Name = propertyName,
                            TypeSymbol = typeSymbol
                        };

                        // 检查是否需要 Init 方法
                        if (iDataValidationInterface != null &&
                            ImplementsInterface(typeSymbol, iDataValidationInterface) &&
                            HasInitMethod(typeSymbol))
                        {
                            propertyInfo.NeedsInit = true;
                        }

                        // 检查是否为 VerifyRule 的子类并获取 Related 类型
                        if (typeSymbol.Name.EndsWith("Rule")) // 简化检查
                        {
                            propertyInfo.IsVerifyRule = true;
                            var relatedTypes = GetRelatedTypesFromClassDeclaration(property, semanticModel, typeSymbol);
                            propertyInfo.RelatedTypes = relatedTypes;
                        }

                        properties.Add(propertyInfo);
                    }
                }
            }
        }

        return properties;
    }

    private static INamedTypeSymbol? FindIDataValidationInterface(Compilation compilation)
    {
        foreach (var tree in compilation.SyntaxTrees)
        {
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var interfaceDeclarations = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>();

            foreach (var interfaceDeclaration in interfaceDeclarations)
            {
                if (interfaceDeclaration.Identifier.Text == "IDataValidation")
                {
                    var symbol = model.GetDeclaredSymbol(interfaceDeclaration);
                    if (symbol != null)
                    {
                        return symbol;
                    }
                }
            }
        }
        return null;
    }

    private static bool ImplementsInterface(INamedTypeSymbol typeSymbol, INamedTypeSymbol interfaceSymbol)
    {
        // 检查类型或其基类是否实现了接口
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.AllInterfaces.Any(i =>
                SymbolEqualityComparer.Default.Equals(i, interfaceSymbol)))
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }

    private static bool HasInitMethod(INamedTypeSymbol typeSymbol)
    {
        // 检查类型及其基类是否有无参的 Init 方法
        var currentType = typeSymbol;
        while (currentType != null)
        {
            var initMethods = currentType.GetMembers("Init")
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == 0 && m.ReturnsVoid)
                .ToList();

            if (initMethods.Any())
            {
                return true;
            }
            currentType = currentType.BaseType;
        }
        return false;
    }

    private static List<string> GetRelatedTypesFromClassDeclaration(PropertyDeclarationSyntax property, SemanticModel semanticModel, INamedTypeSymbol typeSymbol)
    {
        var relatedTypes = new List<string>();

        try
        {
            // 查找该类型的声明语法
            var declaringSyntaxes = typeSymbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<ClassDeclarationSyntax>()
                .ToList();

            foreach (var declaringSyntax in declaringSyntaxes)
            {
                if (declaringSyntax != null)
                {
                    // 在类声明中查找 Related 属性
                    foreach (var member in declaringSyntax.Members)
                    {
                        if (member is PropertyDeclarationSyntax propDecl &&
                            propDecl.Identifier.Text == "Related")
                        {
                            // 直接调试输出表达式类型
                            // Console.WriteLine($"DEBUG: Found Related property with initializer: {propDecl.Initializer?.Value?.GetType().Name}");

                            // 尝试解析 [typeof(DailyValue)] 格式 - 最直接的方式
                            if (propDecl.Initializer?.Value != null)
                            {
                                var initializer = propDecl.Initializer.Value.ToString();
                                // Console.WriteLine($"DEBUG: Initializer text: {initializer}");

                                // 解析 [typeof(DailyValue)] 这种格式
                                if (initializer.Contains("typeof"))
                                {
                                    // 简单的文本解析
                                    var types = ExtractTypesFromInitializerText(initializer);
                                    relatedTypes.AddRange(types);
                                }
                            }

                            // 如果上面的方法失败，尝试语义分析
                            //if (relatedTypes.Count == 0)
                            //{
                            //    relatedTypes.AddRange(ParseRelatedTypes(propDecl, semanticModel));
                            //}

                            if (relatedTypes.Count > 0)
                            {
                                return relatedTypes; // 找到就返回
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 忽略解析错误，使用备用方案
            // Console.WriteLine($"DEBUG: Exception in parsing: {ex.Message}");
        }

        // 备用方案：基于命名约定
        if (relatedTypes.Count == 0)
        {
            if (typeSymbol.Name.Contains("Daily") || typeSymbol.Name.Contains("Clear"))
            {
                relatedTypes.Add("DailyValue");
            }
        }

        return relatedTypes;
    }

    private static List<string> ExtractTypesFromInitializerText(string initializerText)
    {
        var types = new List<string>();

        try
        {
            // 解析 [typeof(DailyValue)] 或 [typeof(DailyValue), typeof(OtherType)] 格式
            // 移除方括号
            var content = initializerText.Trim();
            if (content.StartsWith("[") && content.EndsWith("]"))
            {
                content = content.Substring(1, content.Length - 2);
            }

            // 分割多个 typeof 调用
            var typeCalls = content.Split(new[] { "typeof" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var typeCall in typeCalls)
            {
                var cleanCall = typeCall.Trim();
                if (cleanCall.StartsWith("(") && cleanCall.Contains(")"))
                {
                    // 提取 typeof(X) 中的 X
                    var start = cleanCall.IndexOf('(') + 1;
                    var end = cleanCall.IndexOf(')', start);
                    if (end > start)
                    {
                        var typeName = cleanCall.Substring(start, end - start).Trim();
                        if (!string.IsNullOrEmpty(typeName) && typeName != "(" && typeName != ")")
                        {
                            types.Add(typeName);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略文本解析错误
        }

        return types;
    }
    private static List<string> ExtractTypeNamesFromInitializer(ExpressionSyntax initializer, SemanticModel semanticModel)
    {
        var typeNames = new List<string>();

        try
        {
            // 处理集合表达式 [typeof(DailyValue)]
            if (initializer is CollectionExpressionSyntax collectionExpr)
            {
                foreach (var element in collectionExpr.Elements)
                {
                    if (element is ExpressionElementSyntax exprElement)
                    {
                        if (TryExtractTypeFromExpression(exprElement.Expression, semanticModel, out var typeName))
                        {
                            typeNames.Add(typeName);
                        }
                    }
                }
            }
            // 处理数组创建表达式 new Type[] { typeof(DailyValue) }
            else if (initializer is ArrayCreationExpressionSyntax arrayCreation)
            {
                if (arrayCreation.Initializer?.Expressions != null)
                {
                    foreach (var expr in arrayCreation.Initializer.Expressions)
                    {
                        if (TryExtractTypeFromExpression(expr, semanticModel, out var typeName))
                        {
                            typeNames.Add(typeName);
                        }
                    }
                }
            }
            // 处理隐式数组创建表达式 new[] { typeof(DailyValue) }
            else if (initializer is ImplicitArrayCreationExpressionSyntax implicitArray)
            {
                if (implicitArray.Initializer?.Expressions != null)
                {
                    foreach (var expr in implicitArray.Initializer.Expressions)
                    {
                        if (TryExtractTypeFromExpression(expr, semanticModel, out var typeName))
                        {
                            typeNames.Add(typeName);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return typeNames;
    }

    private static bool TryExtractTypeFromExpression(ExpressionSyntax expression, SemanticModel semanticModel, out string typeName)
    {
        typeName = string.Empty;

        try
        {
            if (expression is InvocationExpressionSyntax invocation &&
                invocation.Expression is IdentifierNameSyntax identifier &&
                identifier.Identifier.Text == "typeof" &&
                invocation.ArgumentList.Arguments.Count == 1)
            {
                var arg = invocation.ArgumentList.Arguments[0].Expression;
                var typeInfo = semanticModel.GetTypeInfo(arg);
                if (typeInfo.Type != null)
                {
                    typeName = typeInfo.Type.ToDisplayString();
                    return true;
                }
            }
        }
        catch
        {
            // 忽略错误
        }

        return false;
    }
     
    private static void GenerateSource(List<PropertyInfo> properties, SourceProductionContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using FMO.Models;");
        sb.AppendLine("namespace FMO.Utilities;");
        sb.AppendLine("public static partial class VerifyRules");
        sb.AppendLine("{");

        // 生成 InitAll 方法
        if (properties.Any(p => p.NeedsInit))
        {
            sb.AppendLine("    public static void InitAll()");
            sb.AppendLine("    {");

            foreach (var property in properties.Where(p => p.NeedsInit).Distinct().OrderBy(p => p.Name))
            {
                sb.AppendLine($"        VerifyRules.{property.Name}.Init();");
            }

            sb.AppendLine("    }");
            sb.AppendLine();
        }

        // 调试信息
        //sb.AppendLine("    // DEBUG INFO:");
        //sb.AppendLine("    // Properties found: " + properties.Count);
        //foreach (var prop in properties)
        //{
        //    sb.AppendLine($"    // {prop.Name} - IsVerifyRule: {prop.IsVerifyRule}, RelatedTypes: [{string.Join(", ", prop.RelatedTypes)}]");
        //}

        // 收集所有唯一的 Related 类型
        var allRelatedTypes = new HashSet<string>();
        foreach (var property in properties.Where(p => p.IsVerifyRule))
        {
            foreach (var relatedType in property.RelatedTypes)
            {
                if (!string.IsNullOrEmpty(relatedType))
                {
                    allRelatedTypes.Add(relatedType);
                }
            }
        }

        sb.AppendLine("    // Related types found: " + allRelatedTypes.Count);
        foreach (var type in allRelatedTypes)
        {
            sb.AppendLine($"    // Related type: {type}");
        }

        // 为每个 Related 类型生成具体的 OnEntityArrival 方法
        //bool hasGeneratedSpecificMethods = false;
        foreach (var relatedType in allRelatedTypes.OrderBy(t => t))
        {
            var typeName = ExtractTypeName(relatedType);
            if (!string.IsNullOrEmpty(typeName))
            {
                sb.AppendLine($"    public static void OnEntityArrival(IEnumerable<{typeName}> entities)");
                sb.AppendLine("    {");

                // 调用所有 Related 包含此类型的规则
                foreach (var property in properties.Where(p => p.IsVerifyRule).OrderBy(p => p.Name))
                {
                    if (property.RelatedTypes.Contains(relatedType))
                    {
                        sb.AppendLine($"        {property.Name}.OnEntityArrival(entities);");
                    }
                }

                sb.AppendLine("    }");
                sb.AppendLine();
                //hasGeneratedSpecificMethods = true;
            }
        }



        // 泛型版本作为后备
        //sb.AppendLine("    public static partial void OnEntityArrival<T>(IEnumerable<T> entities)");
        //sb.AppendLine("    {");
        //sb.AppendLine("        // Fallback for unknown types");
        //sb.AppendLine("    }");

        sb.AppendLine("}");

        context.AddSource("VerifyRules.InitAll.g.cs", sb.ToString());
    }

    private static string ExtractTypeName(string fullTypeName)
    {
        // 移除可能的全局命名空间前缀
        if (fullTypeName.StartsWith("global::"))
        {
            return fullTypeName.Substring(8);
        }
        return fullTypeName;
    }

    private class PropertyInfo
    {
        public string Name { get; set; } = string.Empty;
        public INamedTypeSymbol TypeSymbol { get; set; } = null!;
        public bool NeedsInit { get; set; }
        public bool IsVerifyRule { get; set; }
        public List<string> RelatedTypes { get; set; } = new List<string>();
    }
}