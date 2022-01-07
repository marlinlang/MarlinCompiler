using System.CommandLine.Invocation;
using System.Globalization;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MarlinCompiler.Antlr;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.Ast;

public sealed class AstGenerator : IMarlinParserVisitor<AstNode>
{
    public CompileMessages Messages { get; } = new();
    private readonly IBuilder _builder;

    private string? _module;

    public AstGenerator(IBuilder builder)
    {
        _builder = builder;
        _module = null;
    }

    public AstNode Visit(IParseTree tree)
    {
        return tree.Accept(this);
    }

    public AstNode VisitChildren(IRuleNode node)
    {
        if (node.ChildCount == 0)
        {
            throw new NotImplementedException(
                "AstGenerator doesn't have an implementation for a rule under " + node.GetType().Name
            );
        }
        
        AstNode? result = null;
        for (int i = 0; i < node.ChildCount; i++)
        {
            IParseTree child = node.GetChild(i);
            if (child != null)
            {
                result ??= Visit(child);
            }
        }

        if (result == null)
        {
            throw new NotImplementedException($"{GetType().Name} does not support {node.GetType().Name} nodes");
        }
        
        return result;
    }

    public AstNode VisitFile(MarlinParser.FileContext context)
    {
        RootBlockNode fileNode = new();

        if (context.moduleName() != null)
        {
            VisitModuleName(context.moduleName());
        }
        
        foreach (IParseTree child in context.typeDeclaration())
        {
            fileNode.Body.Add(Visit(child));
        }
        
        return fileNode;
    }

    public AstNode VisitMemberAccess(MarlinParser.MemberAccessContext context)
    {
        AstNode? arrayIndex = context.expression() != null ? Visit(context.expression()) : null;
        
        if (context.typeName() != null)
        {
            // std::Console.WriteLine
            // memberAccess := (typeName DOT)? IDENTIFIER (LBRACKET expression RBRACKET)?
            return new MemberAccessNode(
                context,
                Visit(context.typeName()),
                new NameReferenceNode(context, context.IDENTIFIER().GetText()),
                arrayIndex
            );
        }
        else if (context.IDENTIFIER() != null)
        {
            // x
            // memberAccess := IDENTIFIER
            return new MemberAccessNode(
                context,
                null,
                new NameReferenceNode(context, context.IDENTIFIER().GetText()),
                arrayIndex
            );
        }
        else
        {
            // x.Parent.Node.Something
            // memberAccess := memberAccess DOT memberAccess
            MemberAccessNode former = (MemberAccessNode) VisitMemberAccess(context.memberAccess(0));
            MemberAccessNode latter = (MemberAccessNode) VisitMemberAccess(context.memberAccess(1));

            if (latter.Parent == null)
            {
                latter.Parent = former;
                return latter;
            }
            
            return new MemberAccessNode(
                context,
                former,
                latter,
                arrayIndex
            );
        }
    }

    public AstNode VisitTypeName(MarlinParser.TypeNameContext context)
    {
        if (context.typeName() != null)
        {
            TypeReferenceNode tRef = (TypeReferenceNode) VisitTypeName(context.typeName());
            tRef.Name += "[]";
            return tRef;
        }
        else
        {
            // ToList() is necessary, otherwise string.Join gets super confused and starts yelling
            // like a Karen on an airplane when she gets moved 1 seat to the left and isn't offered
            // free lifetime tickets with the airline for her and her family
            // because obviously string.Join NEEDS a params object[] overload that fucks every other array
            // string.Join 1, RAM 0
            return new TypeReferenceNode(context, string.Join("::", context.IDENTIFIER().ToList()));
        }
    }

    public AstNode VisitModuleName(MarlinParser.ModuleNameContext context)
    {
        TypeReferenceNode name = (TypeReferenceNode) VisitTypeName(context.typeName());
        _module = name.Name;
        return name;
    }

    public AstNode VisitClassDeclaration(MarlinParser.ClassDeclarationContext context)
    {
        MemberVisibility visibility = MemberVisibility.Internal;
        bool changedVisibility = false;
        bool isStatic = false;
        bool isSealed = false;

        string name = $"{_module}::{context.IDENTIFIER().GetText()}";

        List<TypeReferenceNode> baseClasses = new();
        foreach (MarlinParser.TypeNameContext type in context.typeName())
        {
            baseClasses.Add((TypeReferenceNode) VisitTypeName(type));
        }

        Dictionary<string, MarlinParser.ModifierContext> previousModifiers = new();
        foreach (MarlinParser.ModifierContext modifier in context.modifier())
        {
            string modifierText = modifier.GetText();
            if (previousModifiers.ContainsKey(modifierText))
            {
                Messages.Error($"Repeated modifier {modifierText}", new FileLocation(
                    _builder.CurrentFile,
                    modifier.Start.Line,
                    modifier.Start.Column
                ));
                continue;
            }

            switch (modifierText)
            {
                case "private":
                    changedVisibility = true;
                    visibility = MemberVisibility.Private;
                    break;
                case "internal":
                    changedVisibility = true;
                    visibility = MemberVisibility.Internal;
                    break;
                case "public":
                    changedVisibility = true;
                    visibility = MemberVisibility.Public;
                    break;
                case "static":
                    isStatic = true;
                    break;
                case "sealed":
                    isSealed = true;
                    break;
                default:
                    Messages.Error(
                        $"Illegal modifier for class '{modifierText}'",
                        new FileLocation(_builder, modifier.Start)
                    );
                    break;
            }
            
            previousModifiers.Add(modifierText, modifier);
        }

        if (!changedVisibility)
        {
            Messages.Warning(
                "Visibility should always be explicitly defined",
                new FileLocation(_builder, context.Start)
            );
        }
        
        ClassDeclarationNode node = new(
            context,
            name,
            isStatic,
            isSealed,
            visibility,
            baseClasses
        );

        foreach (MarlinParser.ClassMemberContext member in context.classMember())
        {
            node.TypeBody.Body.Add(VisitClassMember(member));
        }

        return node;
    }

    public AstNode VisitStructDeclaration(MarlinParser.StructDeclarationContext context)
    {
        MemberVisibility visibility = MemberVisibility.Internal;
        bool changedVisibility = false;

        string name = $"{_module}::{context.IDENTIFIER().GetText()}";

        Dictionary<string, MarlinParser.ModifierContext> previousModifiers = new();
        foreach (MarlinParser.ModifierContext modifier in context.modifier())
        {
            string modifierText = modifier.GetText();
            if (previousModifiers.ContainsKey(modifierText))
            {
                Messages.Error($"Repeated modifier {modifierText}", new FileLocation(
                    _builder.CurrentFile,
                    modifier.Start.Line,
                    modifier.Start.Column
                ));
                continue;
            }

            switch (modifierText)
            {
                case "private":
                    changedVisibility = true;
                    visibility = MemberVisibility.Private;
                    break;
                case "internal":
                    changedVisibility = true;
                    visibility = MemberVisibility.Internal;
                    break;
                case "public":
                    changedVisibility = true;
                    visibility = MemberVisibility.Public;
                    break;
                default:
                    Messages.Error(
                        $"Illegal modifier for struct '{modifierText}'",
                        new FileLocation(_builder, modifier.Start)
                    );
                    break;
            }
            
            previousModifiers.Add(modifierText, modifier);
        }

        if (!changedVisibility)
        {
            Messages.Warning(
                "Visibility should always be explicitly defined",
                new FileLocation(_builder, context.Start)
            );
        }
        
        StructDeclarationNode node = new(
            context,
            name,
            visibility
        );

        foreach (MarlinParser.StructMemberContext member in context.structMember())
        {
            node.TypeBody.Body.Add(VisitStructMember(member));
        }

        return node;
    }

    public AstNode VisitMethodDeclaration(MarlinParser.MethodDeclarationContext context)
    {
        MemberVisibility visibility = MemberVisibility.Internal;
        bool isStatic = false;
        
        Dictionary<string, MarlinParser.ModifierContext> previousModifiers = new();
        foreach (MarlinParser.ModifierContext modifier in context.modifier())
        {
            string modifierText = modifier.GetText();
            if (previousModifiers.ContainsKey(modifierText))
            {
                Messages.Error($"Repeated modifier {modifierText}", new FileLocation(
                    _builder.CurrentFile,
                    modifier.Start.Line,
                    modifier.Start.Column
                ));
                continue;
            }

            switch (modifierText)
            {
                case "private":
                    visibility = MemberVisibility.Private;
                    break;
                case "internal":
                    visibility = MemberVisibility.Internal;
                    break;
                case "public":
                    visibility = MemberVisibility.Public;
                    break;
                case "static":
                    isStatic = true;
                    break;
            }
            
            previousModifiers.Add(modifierText, modifier);
        }
        
        return new MethodDeclarationNode(
            context,
            context.IDENTIFIER().GetText(),
            isStatic,
            visibility,
            (MethodPrototypeNode) VisitMethodBody(context.methodBody()),
            (TypeReferenceNode) VisitTypeName(context.typeName())
        );
    }

    public AstNode VisitMethodCall(MarlinParser.MethodCallContext context)
    {
        return new MethodCallNode(
            context,
            (MemberAccessNode) VisitMemberAccess(context.memberAccess()),
            HandleGiveArgs(context.giveArgs())
        );
    }

    public AstNode VisitVariableDeclaration(MarlinParser.VariableDeclarationContext context)
    {
        MemberVisibility vis = MemberVisibility.Private;
        bool isStatic = false;
        bool isNative = context != null;
        
        Dictionary<string, MarlinParser.ModifierContext> previousModifiers = new();
        foreach (MarlinParser.ModifierContext modifier in context.modifier())
        {
            string modifierText = modifier.GetText();
            if (previousModifiers.ContainsKey(modifierText))
            {
                Messages.Error($"Repeated modifier {modifierText}", new FileLocation(
                    _builder.CurrentFile,
                    modifier.Start.Line,
                    modifier.Start.Column
                ));
                continue;
            }

            switch (modifierText)
            {
                case "private":
                    vis = MemberVisibility.Private;
                    break;
                case "public":
                    vis = MemberVisibility.Public;
                    break;
                case "static":
                    isStatic = true;
                    break;
                default:
                    Messages.Error($"Unsupported modifier {modifierText} for variable");
                    break;
            }
            
            previousModifiers.Add(modifierText, modifier);
        }

        TypeReferenceNode type = (TypeReferenceNode) VisitTypeName(context.typeName());
        string name = context.IDENTIFIER().GetText();
        
        return context.expression() != null
            ? new VariableDeclarationNode(context, type, name, Visit(context.expression()), isStatic, isNative, vis)
            : new VariableDeclarationNode(context, type, name, null, isStatic, isNative, vis);
    }

    public AstNode VisitLocalVariableDeclaration(MarlinParser.LocalVariableDeclarationContext context)
    {
        TypeReferenceNode type = (TypeReferenceNode) VisitTypeName(context.typeName());
        string name = context.IDENTIFIER().GetText();

        return context.expression() != null
            ? new LocalVariableDeclarationNode(context, type, name, Visit(context.expression()))
            : new LocalVariableDeclarationNode(context, type, name, null);
    }

    public AstNode VisitVariableAssignment(MarlinParser.VariableAssignmentContext context)
    {
        MemberAccessNode varNode = (MemberAccessNode) VisitMemberAccess(context.memberAccess());
        return context.expression() != null
            ? new VariableAssignmentNode(context, varNode, Visit(context.expression()))
            : new VariableAssignmentNode(context, varNode, null);
    }

    public AstNode VisitReturn(MarlinParser.ReturnContext context)
    {
        return context.expression() != null
            ? new ReturnNode(context, Visit(context.expression()))
            : new ReturnNode(context, null);
    }
    
    public AstNode VisitMethodBody(MarlinParser.MethodBodyContext context)
    {
        List<ArgumentVariableDeclarationNode> args = HandleExpectArgs(context.expectArgs());
        
        if (context.expression() != null)
        {
            // Arrow expression:
            // >  int Something() => 12;

            return new MethodPrototypeNode(
                    context,
                    args,
                    new List<AstNode>()
                    {
                        new ReturnNode(context, Visit(context.expression()))
                    }
            );
        }
        
        // Regular method body
        // >  int Something() { return 12; }

        List<AstNode> body = new();
        foreach (IRuleNode statement in context.statement())
        {
            body.Add(Visit(statement));
        }

        return new MethodPrototypeNode(
            context,
            args,
            body
        );
    }

    public AstNode VisitBooleanLiteral(MarlinParser.BooleanLiteralContext context)
    {
        return new BooleanNode(context, context.TRUE() != null);
    }

    public AstNode VisitStringLiteral(MarlinParser.StringLiteralContext context)
    {
        return new StringNode(context, context.GetText()[1..^1]);
    }

    public AstNode VisitNumberLiteral(MarlinParser.NumberLiteralContext context)
    {
        return context.INTEGER() != null
            ? new IntegerNode(context, int.Parse(context.INTEGER().GetText(), NumberStyles.Any))
            : new DoubleNode(context, double.Parse(context.DOUBLE().GetText(), NumberStyles.Any));
    }

    public AstNode VisitArrayInitializer(MarlinParser.ArrayInitializerContext context)
    {
        if (context.typeName().typeName() != null)
        {
            // nested typename means int[] as the whole typename
            // e.g. new int[][5]
            // we don't really want that though, doesn't really make sense
            Messages.Error(
                "Cannot have array type as the array type. Remove the first [] from the array initializer to fix.",
                new FileLocation(_builder, context.typeName().LBRACKET().Symbol)
            );

            return new ArrayInitializerNode(context, (TypeReferenceNode) Visit(context.typeName()), 
                new IntegerNode(null, 0),
                Array.Empty<AstNode>());
        }

        TypeReferenceNode typeRef = (TypeReferenceNode) VisitTypeName(context.typeName());

        if (context.LBRACE() == null)
        {
            // no initializing members
            return new ArrayInitializerNode(
                context,
                typeRef,
                Visit(context.expression(0)),
                Array.Empty<AstNode>()
            );
        }
        else
        {
            int elementCount = context.expression().Length;
            AstNode[] elements = new AstNode[elementCount];
            for (int i = 0; i < elementCount; i++)
            {
                elements[i] = Visit(context.expression(i));
            }

            return new ArrayInitializerNode(
                context,
                typeRef,
                new IntegerNode(null, elementCount),
                elements
            );
        }
    }

    public AstNode VisitStatement(MarlinParser.StatementContext context) => VisitChildren(context);
    public AstNode VisitExpression(MarlinParser.ExpressionContext context) => VisitChildren(context);

    public AstNode VisitModifier(MarlinParser.ModifierContext context)
        => throw new InvalidOperationException("Do not not call VisitModifier, use GetText() on the context");

    public AstNode VisitExpectArgs(MarlinParser.ExpectArgsContext context)
        => throw new InvalidOperationException("Do not call VisitExpectArgs, use HandleExpectArgs instead.");

    public AstNode VisitExpectArg(MarlinParser.ExpectArgContext context)
        => throw new InvalidOperationException("Do not call VisitExpectArg, use HandleExpectArgs instead.");

    public AstNode VisitGiveArgs(MarlinParser.GiveArgsContext context)
        => throw new InvalidOperationException("Do not call VisitGiveArgs, use HandleGiveArgs instead.");

    public AstNode VisitTypeDeclaration(MarlinParser.TypeDeclarationContext context) => VisitChildren(context);

    public AstNode VisitClassMember(MarlinParser.ClassMemberContext context) => VisitChildren(context);

    public AstNode VisitStructMember(MarlinParser.StructMemberContext context) => VisitChildren(context);

    public AstNode VisitTerminal(ITerminalNode node) => node.Accept(this);

    public AstNode VisitErrorNode(IErrorNode node)
        => throw new InvalidOperationException("Error nodes should be dealt with before AstGenerator");

    #region Utils
    private List<ArgumentVariableDeclarationNode> HandleExpectArgs(MarlinParser.ExpectArgsContext context)
    {
        Dictionary<string, (TypeReferenceNode, ParserRuleContext)> dict = new();
        List<ArgumentVariableDeclarationNode> args = new();

        foreach (MarlinParser.ExpectArgContext arg in context.expectArg())
        {
            TypeReferenceNode argType = (TypeReferenceNode) VisitTypeName(arg.typeName());
            string argName = arg.IDENTIFIER().GetText();
            if (dict.ContainsKey(argName))
            {
                Messages.Error($"Repeated argument name {argName}", new FileLocation(_builder, arg.Start));
                continue;
            }
            
            dict.Add(argName, (argType, arg));
        }

        foreach ((string name, (TypeReferenceNode type, ParserRuleContext ctx)) in dict)
        {
            args.Add(new ArgumentVariableDeclarationNode(ctx, type, name));
        }
        
        return args;
    }

    private List<AstNode> HandleGiveArgs(MarlinParser.GiveArgsContext context)
    {
        List<AstNode> args = new();

        foreach (MarlinParser.ExpressionContext expression in context.expression())
        {
            AstNode x = Visit(expression);
            args.Add(x);
        }
        
        return args;
    }
    #endregion
}