# Layer 6 — DSLs

> **Status:** informative (this revision); names and shapes are not yet pinned.
> **Depends on:** Layer 0, Layer 1, Layer 2, Layer 4 (for `#r`-style imports), Layer 5 (for `LoadProject`).
> **Consumed by:** end users authoring pipelines.

This document specifies the obligations a **Domain-Specific Language** takes on when it claims to author `gib` pipelines, and sketches the C# DSL as the reference example. It does not pin a single canonical authoring format; the system is designed to accommodate many DSLs side by side (Layer 5 §3.7's `LoadProject` dispatches by file extension).

This revision is **informative** for any specific DSL surface, but the DSL conformance contract in §1 is **normative**: any DSL that claims to author `gib` pipelines MUST satisfy it.

---

## 1. What a DSL is

A **DSL** is a function — typically shipped as a Layer 5 function and dispatched to by `LoadProject` — that:

1. Reads input describing a pipeline (a script file, a directory tree, an in-memory data structure, etc.).
2. Interprets that input according to its own format/language.
3. Dynamically `Invoke`s and wires together the functions that input describes.
4. Exposes the result as its own output channels — a "module" the caller can subscribe to.

A DSL is **not** a privileged construct. Mechanically it is a `gip` function whose internal behavior happens to be "compile a script and run its lambda." From the runtime's point of view it is no different from `CSharpCompile` or `Glob`.

### 1.1 Conformance contract

A conforming DSL MUST:

1. **Materialize a function.** The DSL's job is to produce a Layer 0 function instance whose inputs and outputs are the loaded module's declared parameters and exports. The DSL itself is a function; the module it produces is also a function.
2. **Accept parameter channels by name.** Whatever a loaded module declares as inputs MUST be bindable by name through the `LoadProject` call that loaded it (Layer 5 §3.7). A DSL MUST NOT introduce ambient parameter mechanisms (no global property bag, no environment-variable side-channel).
3. **Resolve external references through the standard chain.** Any "import a function from elsewhere" facility (such as the C# DSL's `#r` directive — §2.3) MUST go through the orchestrator's `resolve` (Layer 2 §3.1, Layer 4 §5). DSLs MUST NOT define their own parallel resolution mechanism.
4. **Project schemas at the surface.** The DSL surface MUST expose each resolved function's `FunctionMetadata` (Layer 1 §3) to the author in whatever form is natural for the language — typed proxies, dynamic accessors, generated stubs. The same projection MUST be available at design time and at runtime (§1.2).
5. **Treat literals as constant channels.** Where the DSL accepts a channel, it MUST also accept a literal value of the channel's payload type and lift it to a constant channel automatically. Authors MUST NOT have to write boilerplate to wrap literals.
6. **Have no module-level identity beyond file location.** A loaded module's identity is the absolute path of the file the DSL read it from (or an equivalent canonical address for non-file inputs). The DSL MUST NOT require an in-script name declaration that could disagree with the file path.

### 1.2 Design-time / runtime symmetry

A DSL MUST behave equivalently in two modes:

- **Runtime.** The DSL function is `Invoke`d as part of a live pipeline; it materializes its subgraph and forwards its exports.
- **Design time.** An IDE companion (in the same DSL implementation) processes the same input ahead of any user keystroke and feeds the resolved schemas into the host language's tooling so the author gets IntelliSense, refactoring, and go-to-definition on functions they have not written themselves.

Both modes MUST go through the same resolver and the same schema-projection logic. There MUST NOT be a separate type-stub file or schema-generation step that the author has to keep in sync.

### 1.3 No new operations or transports

A DSL MUST NOT introduce new Layer 0 operations or new Layer 3 transports. Anything it does is built out of `Invoke`, `Subscribe`, and metadata over whatever transports the host already serves.

---

## 2. Reference DSL: C\#

The reference DSL ships as a function whose input is a script file with extension `.gibcs`. It uses C# script syntax extended with a small set of conventions.

### 2.1 File extension

`.gibcs` is deliberately distinct from `.cs`. A `.gibcs` file is **not** a freestanding C# source file: it is processed by the C# DSL function, which compiles it in a controlled environment with a specific `using static` surface and entry-point convention. The distinct extension keeps general-purpose C# tooling, project systems, and language servers from treating it as ordinary C# code.

### 2.2 Module entry point

A `.gibcs` file MUST end with `return Module(...);` where `Module` is supplied by the DSL's `using static Gib.Dsl;` surface. The `Module` call takes a single configuration lambda; the lambda's parameter list — beyond a leading `m` for the module builder — declares the module's inputs:

```csharp
return Module((m,
    ValueSignal<string>      tfm,
    ValueSignal<string>      config,
    ValueSignal<AbsoluteDir> outDir) =>
{
    // ...
});
```

Each non-`m` parameter is a typed channel handle the parent must supply when it loads this module via `LoadProject` (Layer 5 §3.7). This satisfies §1.1 rule 2 and §1.1 rule 6 simultaneously: the lambda's signature *is* the module's input declaration, and the file's path *is* its identity.

### 2.3 `#r` — importing remote functions

The C# DSL extends C# script's `#r` directive to accept logical URIs (Layer 0 §3.2). For each `#r "<uri>"`, the DSL preprocessor MUST:

1. Hand the URI to the orchestrator's `resolve` (Layer 4 §5). The output is a `FunctionRef` (Layer 0 §1.4) whose URI is now an endpoint URI.
2. Fetch the resolved function's `FunctionMetadata` via `GetFunctionMetadata` (Layer 0 §2.3) and project it into a typed proxy class visible to the script's compilation context.

This satisfies §1.1 rules 3 and 4. Additionally, every other public type in the resolved package MUST be visible to the script under its normal namespace, the way an ordinary `#r` reference works in C# scripting. This collapses Gradle's `plugins { … }` / `buildscript { dependencies { … } }` split into a single mechanism (one `#r` brings in both "things you can `Add` as functions" and "library types you can use as values").

### 2.4 Channel handles look like properties

A function's channels MUST be exposed on its typed proxy as ordinary C# properties. Wiring two channels together is assignment:

```csharp
var src  = m.Add<Glob>(g => { g.Root = "."; g.Pattern = "**/*.cs"; });
var copy = m.Add<CopyTree>(c =>
{
    c.DestinationRoot = "obj/copies";
    c.Files           = src.Files;          // channel-to-channel
});
```

Behind the property assignment is a `Subscribe`/`Invoke` wiring at the runtime level. Authors do not see signals individually; they see a channel handle that *looks* like a value.

Literal-to-channel lifting (§1.1 rule 5) is implicit: `c.DestinationRoot = "obj/copies"` lifts the string into a constant `ValueSignal<AbsoluteDir>`.

### 2.5 Loading other modules

`LoadProject` (Layer 5 §3.7) is exposed as a normal addable function. There is no separate `Include` keyword:

```csharp
var widgets = m.Add<LoadProject>(p =>
{
    p.Path          = "../Acme.Widgets";
    p.tfm           = tfm;
    p.config        = config;
    p.outDir        = outDir;
    p.frameworkRefs = frameworkRefs;
});
```

The loaded module's exports are `widgets.Assembly`, `widgets.PdbFiles`, etc. — channels of the same shape as any other.

### 2.6 Exports

`m.Export("Name", channel)` declares an export. The set of exports a module declares is the set of channels callers (and `LoadProject`) see as its outputs. There is no ambient-export mechanism.

### 2.7 No CLI awareness

A `.gibcs` script MUST NOT reference the CLI directly. CLI arguments are bound to the **root** module's lambda parameters by the loader at startup (per the description in the README's "How the CLI gets in" section); from any script's point of view, its inputs are just its declared parameter channels, and the source of those channels is the parent's (or, for the root, the loader's) concern. This satisfies §1.1 rule 2.

---

## 3. Other DSLs

Other DSLs are entirely admissible and use whatever conventions are natural for their language:

- A YAML DSL might use a top-level `uses:` list for `#r`-equivalent imports and a `module:` block for the lambda.
- An XML DSL might use `<Import Uri="…" />` elements and `<Module>` root elements.
- A Lisp DSL might use `(use "…")` and `(module …)`.
- A typed-builder API in C# (without a separate script file) might be nothing but a method on a builder type.

Whichever form they take, the conformance contract of §1 applies: they MUST resolve external references through the standard chain, project schemas at the surface, support design-time / runtime symmetry, and avoid introducing ambient state.

---

## 4. Status

This revision pins only the DSL conformance contract (§1). The C# DSL described in §2 is a reference example; its file extension, surface API, and directive syntax may evolve as the prototype stabilizes. A future revision will promote the stable C# DSL surface to normative status alongside the function library (Layer 5).
