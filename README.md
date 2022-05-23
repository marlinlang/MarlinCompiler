# The Marlin Compiler
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/marlinlang/MarlinCompiler/.NET)
![GitHub License](https://img.shields.io/github/license/marlinlang/MarlinCompiler)
![GitHub top language](https://img.shields.io/github/languages/top/marlinlang/MarlinCompiler)


Marlin is a compiled, statically/strongly typed & no-undefined-behaviours programming language designed for general
purpose use. The language focuses itself on being as explicit as possible.

This is the repository of the official compiler for the language.

***This project is in WIP stages!***

---

### Visit the [project homepage](https://marlinlang.github.io/)!

#### We also have a channel in the [PLTD discord](https://discord.gg/4Kjt3ZE) - #marlin.

---

## Hello, world!

```csharp
using std;

module app;

public static class Program {
    public static void Main() {
        std::Console.PrintLine("Hello, world!");
    }
}
```

More examples can be found in the `examples/` directory of the repo.

## Roadmap

- [X] Lexing/parsing
- [ ] Semantic analysis - **current**
- [ ] Code generation
- [ ] Code optimization
- [ ] Documentation
- [ ] LSP (Language Server Protocol) implementation
- [ ] IDE integrations (vscode, sublime, etc)
- [ ] Unit tests
- [ ] Code coverage