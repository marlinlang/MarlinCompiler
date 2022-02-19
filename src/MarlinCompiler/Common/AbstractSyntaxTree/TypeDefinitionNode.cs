﻿using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A type definition. Do not use directly - use inheritors.
/// </summary>
public class TypeDefinitionNode : ContainerNode
{
    /// <summary>
    /// The name of the type without the module path.
    /// E.g., Console instead of std::Console
    /// </summary>
    public string LocalName { get; }
    
    /// <summary>
    /// The name of the module the type is located within.
    /// </summary>
    public string ModuleName { get; }

    /// <summary>
    /// The accessibility of the type.
    /// </summary>
    public GetAccessibility Accessibility { get; }
    
    public TypeDefinitionNode(string name, string module, GetAccessibility accessibility)
    {
        LocalName = name;
        ModuleName = module;
        Accessibility = accessibility;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
        => throw new InvalidOperationException($"Cannot visit TypeDefinitions directly. Use subclasses.");
    
}