﻿using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A reference to a type.
/// </summary>
public sealed class TypeReferenceNode : Node
{
    public string FullName { get; set; }

    public TypeReferenceNode(string fullName)
    {
        FullName = fullName;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.TypeReference(this);
    }
}