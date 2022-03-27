# The Marlin Compiler

---

Marlin is a compiled, statically/strongly typed & no-undefined-behaviours programming language designed for general purpose use. The language focuses itself on being as explicit as possible.

This is the repository of the official compiler for the language.

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