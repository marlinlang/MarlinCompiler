﻿using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;

namespace MarlinCompiler.Frontend.SemanticAnalysis;

public sealed partial class SemanticAnalyzer
{
    public SemanticAnalyzer(ContainerNode root)
    {
        _root             = root;
        MessageCollection = new MessageCollection();
    }

    /// <summary>
    /// A collection of the analyzer messages.
    /// </summary>
    public MessageCollection MessageCollection { get; }

    /// <summary>
    /// The file root.
    /// </summary>
    private readonly ContainerNode _root;

    /// <summary>
    /// The current analyzer pass.
    /// </summary>
    private Pass _pass;

    /// <summary>
    /// Starts analyzing the program.
    /// </summary>
    public void Analyze()
    {
        PushScope("<MARLIN_PROGRAM>");
        foreach (Pass pass in Enum.GetValues<Pass>())
        {
            _pass = pass;
            Visit(_root);
        }
        PopScope();
    }
}