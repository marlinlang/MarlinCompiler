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
        /*
         TODO Fix this test
        Lexer.Token[] expected =
        {
            new(TokenType.Modifier, "public", default),
            new(TokenType.Modifier, "static", default),
            new(TokenType.Void, "void", default),
            new(TokenType.Identifier, "Main", default),
            new(TokenType.LeftParen, "(", default),
            new(TokenType.RightParen, ")", default),
            new(TokenType.LeftBrace, "{", default),
            new(TokenType.Identifier, "std", default),
            new(TokenType.DoubleColon, "::", default),
            new(TokenType.Identifier, "Int32", default),
            new(TokenType.Identifier, "number", default),
            new(TokenType.Assign, "=", default),
            new(TokenType.String, "string", default),
            new(TokenType.Semicolon, ";", default),
            new(TokenType.RightBrace, "}", default),
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
        */
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