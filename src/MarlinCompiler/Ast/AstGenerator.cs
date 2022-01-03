using System.CommandLine.Invocation;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MarlinCompiler.Antlr;
using MarlinCompiler.Compilation;

namespace MarlinCompiler.Ast;

public sealed class AstGenerator : MarlinParserBaseVisitor<AstNode>
{
    // FYI: MarlinParserBaseVisitor calls VisitChildren on all non-overriden rules
    // If a rule is missing, it's either that I'm an idiot OR that it just needs
    // to visit the children of the rule, e.g. a file

    public CompileMessages Messages { get; } = new();
    private readonly IBuilder _builder;

    private string _module;

    public AstGenerator(IBuilder builder)
    {
        _builder = builder;
        _module = null;
    }

    public override RootBlockNode VisitFile(MarlinParser.FileContext context)
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

    public override MemberAccessNode VisitMemberAccess(MarlinParser.MemberAccessContext context)
    {
        AstNode arrayIndex = context.expression() != null ? Visit(context.expression()) : null;
        
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
            MemberAccessNode former = VisitMemberAccess(context.memberAccess(0));
            MemberAccessNode latter = VisitMemberAccess(context.memberAccess(1));

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

    public override TypeReferenceNode VisitTypeName(MarlinParser.TypeNameContext context)
    {
        // ToList() is necessary, otherwise string.Join gets super confused and starts yelling
        // like a Karen on an airplane when she gets moved 1 seat to the left and isn't offered
        // free lifetime tickets with the airline for her and her family
        // because obviously string.Join NEEDS a params object[] overload that fucks every other array
        // string.Join 1, RAM 0
        return new TypeReferenceNode(context, string.Join("::", context.IDENTIFIER().ToList()));
    }

    public override TypeReferenceNode VisitModuleName(MarlinParser.ModuleNameContext context)
    {
        TypeReferenceNode name = VisitTypeName(context.typeName());
        _module = name.Name;
        return name;
    }

    public override ClassDeclarationNode VisitClassDeclaration(MarlinParser.ClassDeclarationContext context)
    {
        MemberVisibility visibility = MemberVisibility.Internal;
        bool changedVisibility = false;
        bool isStatic = false;
        bool isSealed = false;

        string name = $"{_module}::{context.IDENTIFIER().GetText()}";

        List<TypeReferenceNode> baseClasses = new();
        foreach (MarlinParser.TypeNameContext type in context.typeName())
        {
            baseClasses.Add(VisitTypeName(type));
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
            node.TypeBody.Body.Add(Visit(member));
        }

        return node;
    }

    public override MethodDeclarationNode VisitMethodDeclaration(MarlinParser.MethodDeclarationContext context)
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
            VisitMethodBody(context.methodBody())
        );
    }

    public override MethodCallNode VisitMethodCall(MarlinParser.MethodCallContext context)
    {
        return new MethodCallNode(
            context,
            VisitMemberAccess(context.memberAccess()),
            HandleGiveArgs(context.giveArgs())
        );
    }

    public override VariableDeclarationNode VisitVariableDeclaration(MarlinParser.VariableDeclarationContext context)
    {
        MemberVisibility vis = MemberVisibility.Private;
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

        TypeReferenceNode type = VisitTypeName(context.typeName());
        string name = context.IDENTIFIER().GetText();
        
        return context.expression() != null
            ? new VariableDeclarationNode(context, type, name, Visit(context.expression()), isStatic, vis)
            : new VariableDeclarationNode(context, type, name, null, isStatic, vis);
    }

    public override AstNode VisitLocalVariableDeclaration(MarlinParser.LocalVariableDeclarationContext context)
    {
        TypeReferenceNode type = VisitTypeName(context.typeName());
        string name = context.IDENTIFIER().GetText();

        return context.expression() != null
            ? new LocalVariableDeclarationNode(context, type, name, Visit(context.expression()))
            : new LocalVariableDeclarationNode(context, type, name, null);
    }

    public override AstNode VisitVariableAssignment(MarlinParser.VariableAssignmentContext context)
    {
        MemberAccessNode varNode = VisitMemberAccess(context.memberAccess());
        return context.expression() != null
            ? new VariableAssignmentNode(context, varNode, Visit(context.expression()))
            : new VariableAssignmentNode(context, varNode, null);
    }

    public override ReturnNode VisitReturn(MarlinParser.ReturnContext context)
    {
        return context.expression() != null
            ? new ReturnNode(context, Visit(context.expression()))
            : new ReturnNode(context, null);
    }
    
    public override MethodPrototypeNode VisitMethodBody(MarlinParser.MethodBodyContext context)
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
        foreach (MarlinParser.StatementContext statement in context.statement())
        {
            body.Add(Visit(statement.children[0]));
        }

        return new MethodPrototypeNode(
            context,
            args,
            body
        );
    }

    public override BooleanNode VisitBooleanLiteral(MarlinParser.BooleanLiteralContext context)
    {
        return new BooleanNode(context, context.TRUE() != null);
    }

    public override StringNode VisitStringLiteral(MarlinParser.StringLiteralContext context)
    {
        return new StringNode(context, "");
    }
    
    #region Utils
    private List<ArgumentVariableDeclarationNode> HandleExpectArgs(MarlinParser.ExpectArgsContext context)
    {
        Dictionary<string, (TypeReferenceNode, ParserRuleContext)> dict = new();
        List<ArgumentVariableDeclarationNode> args = new();

        foreach (MarlinParser.ExpectArgContext arg in context.expectArg())
        {
            TypeReferenceNode argType = VisitTypeName(arg.typeName());
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
            args.Add(Visit(expression));
        }
        
        return args;
    }
    #endregion
}