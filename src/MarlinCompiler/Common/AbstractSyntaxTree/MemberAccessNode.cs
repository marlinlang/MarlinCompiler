﻿using MarlinCompiler.Common.Visitors;

namespace MarlinCompiler.Common.AbstractSyntaxTree;

/// <summary>
/// A node which references a parent and tries to get a member within it.
/// </summary>
public sealed class MemberAccessNode : Node
{
    public Node Parent { get; }
    public Node Member { get; }

    public MemberAccessNode(Node parent, Node member)
    {
        Parent = parent;
        Member = member;
    }

    public override T AcceptVisitor<T>(IAstVisitor<T> visitor) => visitor.MemberAccess(this);
}