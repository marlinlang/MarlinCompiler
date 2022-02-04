﻿using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

public class ConstructorDeclarationNode : ContainerNode
{
    public GetAccessibility Accessibility { get; }
    public VariableNode[] Args { get; }

    public ConstructorDeclarationNode(GetAccessibility accessibility, VariableNode[] args)
    {
        Accessibility = accessibility;
        Args = args;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor)
    {
        return visitor.ConstructorDeclaration(this);
    }
}