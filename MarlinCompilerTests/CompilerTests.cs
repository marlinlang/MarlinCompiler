using System;
using System.Linq;
using MarlinCompiler.Common;
using MarlinCompiler.Common.AbstractSyntaxTree;
using MarlinCompiler.Common.Symbols;
using MarlinCompiler.Common.Symbols.Kinds;
using MarlinCompiler.Frontend.Lexing;
using MarlinCompiler.Frontend.Parsing;
using NUnit.Framework;

namespace MarlinCompilerTests;

public class CompilerModulesTests
{
    [Test]
    public void Lexing()
    {
        FileLocation location = new("test.marlin");
        Lexer.Token[] expected =
        {
            new(TokenType.Modifier, "public", location),
            new(TokenType.Modifier, "static", location),
            new(TokenType.Void, "void", location),
            new(TokenType.Identifier, "Main", location),
            new(TokenType.LeftParen, "(", location),
            new(TokenType.RightParen, ")", location),
            new(TokenType.LeftBrace, "{", location),
            new(TokenType.Identifier, "std", location),
            new(TokenType.DoubleColon, "::", location),
            new(TokenType.Identifier, "Int32", location),
            new(TokenType.Identifier, "number", location),
            new(TokenType.Assign, "=", location),
            new(TokenType.String, "string", location),
            new(TokenType.Semicolon, ";", location),
            new(TokenType.RightBrace, "}", location),
        };

        Lexer lexer = new(
            $@"
                    public static void Main() {{
                        std::Int32 number = ""string"";
                    }}",
            "<test>"
        );
        Lexer.Token[] tokens = lexer.Lex();

        Assert.AreEqual(lexer.MessageCollection.Count(), 0, "Lexer had errors.");
        Assert.AreEqual(expected.Length, tokens.Length, "Count mismatch between expected and given tokens.");

        for (int i = 0; i < tokens.Length; ++i)
        {
            Assert.AreEqual(expected[i], tokens[i], $"Token {i + 1} does not match expected token.");
        }
    }

    [Test]
    public void SimpleParsing()
    {
        Lexer lexer = new(
            $@"
                    module app;
                    public class Program {{
                        public static void Main() {{
                            std::Int32 number = 1;
                        }}
                    }}",
            "<test>"
        );

        Lexer.Token[] tokens = lexer.Lex();
        Assert.AreEqual(lexer.MessageCollection.Count(), 0, "Lexer had errors.");

        Parser parser = new(new Tokens(tokens), "<test>");
        Node? rootNode = parser.Parse();
        Assert.NotNull(rootNode);
        Assert.AreEqual(parser.MessageCollection.Count(), 0, "Parser had errors.");

        if (rootNode is CompilationUnitNode container)
        {
            if (container.Children.Count != 1)
            {
                Assert.Fail($"Root node has more or less than one child. ({container.Children.Count})");
                return;
            }

            if (container.Children[0] is not ClassTypeDefinitionNode classType)
            {
                Assert.Fail("Expected one class in root node.");
                return;
            }

            if (classType.Children.Count != 1)
            {
                Assert.Fail($"Class node has more or less than one child. ({classType.Children.Count})");
                return;
            }

            if (classType.Children[0] is not MethodDeclarationNode method)
            {
                Assert.Fail("Expected one method in class node.");
                return;
            }

            if (method.Children.Count != 1)
            {
                Assert.Fail($"Method node has more or less than one child. ({method.Children.Count})");
                return;
            }

            if (method.Children[0] is not LocalVariableDeclarationNode)
            {
                Assert.Fail($"Method node has one child that is not a local var decl.");
                return;
            }

            Assert.Pass();
        }
        else
        {
            Assert.Fail("Root node is not a compilation unit node.");
        }
    }

    [Test]
    public void Symbols()
    {
        SymbolTable root = new(null);

        ModuleSymbol modSym = new(new CompilationUnitNode("test", Array.Empty<(string, FileLocation)>()));
        SymbolTable modSymTbl = new(root, modSym);

        root.AddSymbol(modSymTbl);

        ClassTypeSymbol clsSym = new(
            new ClassTypeDefinitionNode(
                "Program",
                modSym.Name,
                GetAccessibility.Internal,
                false,
                null,
                Array.Empty<string>()
            )
        );

        SymbolTable clsSymTbl = new(modSymTbl, clsSym);
        
        modSymTbl.AddSymbol(clsSymTbl);
        
        modSymTbl.LookupSymbol<ClassTypeSymbol>(x => x is ClassTypeSymbol cls && cls.TypeName == clsSym.TypeName);
        Assert.Catch(() => modSymTbl.LookupSymbol<ClassTypeSymbol>(_ => false));
    }
}