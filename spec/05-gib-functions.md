# Layer 5 — `gib` Function Library

> **Status:** informative (this revision); names and shapes are not yet pinned.
> **Depends on:** Layer 0, Layer 1, Layer 2; in practice consumed via Layer 4.
> **Consumed by:** Layer 6 (DSLs).

This document catalogs the **build-flavored functions** that `gib` ships on top of `gip`. Nothing here introduces new runtime primitives: every entry is, mechanically, a `gip` function with a `FunctionMetadata` (Layer 1 §3) and channels that follow the signal shapes of Layer 1 §2.

This revision is **informative**. It enumerates the categories of functions `gib` is expected to provide and sketches their shapes, but it does not normatively pin function URIs, schemas, or behavior. A future revision will promote the stable subset to normative status; until then, ship implementations are the source of truth and this document is a roadmap.

---

## 1. Conventions

### 1.1 URIs

`gib`-shipped functions live under the `gib:builtin/...` logical URI prefix. Their orchestrator-resolved endpoint URIs depend on which Function Host serves them in a given deployment (Layer 0 §3.3).

### 1.2 Naming

- Function type names are PascalCase: `Glob`, `CSharpCompile`, `XUnitRun`.
- Channel names on a function are PascalCase: `Sources`, `Diagnostics`, `Assembly`.
- Payload type names are PascalCase: `AbsoluteFile`, `Diagnostic`, `TestResult`.

### 1.3 Shape conventions

When a function describes "a single value that may change," its channel uses `ValueSignal<T>` (Layer 1 §2.1). When it describes "an unordered set whose membership evolves," it uses `SetSignal<T>` (§2.2). When it describes "an ordered, append-style stream," it uses `SequenceSignal<T>` (§2.3). These are conventions; specific functions MAY use richer payload types when the semantics call for it.

---

## 2. Common payload types

The function library is built on a small shared vocabulary of payload types. These are themselves Protobuf message types (Layer 1 §4.1) shipped in a base schema package.

- **`AbsoluteFile`** — an absolute path to a file on the host file system.
- **`AbsoluteDir`** — an absolute path to a directory.
- **`RelativeFile`** — a path relative to a declared root.
- **`RelativeDir`** — a directory path relative to a declared root.
- **`Diagnostic`** — a structured diagnostic (severity, code, message, span).
- **`TestResult`** — outcome of a test run (passed/failed counts, per-test results).
- **`LogLine`** — a single line of structured log output (level, timestamp, message).

Additional payload types (richer file-content descriptors, ECMA-335 metadata projections, byte-range deltas, etc.) are introduced by the function categories that need them.

---

## 3. Function categories

### 3.1 File-system sources

Functions that observe the file system and emit channels describing what is present.

- **`Glob`** — watches a root directory for files matching a pattern; emits a `SetSignal<RelativeFile>` of current matches.
  - Inputs: `Root: ValueSignal<AbsoluteDir>`, `Pattern: ValueSignal<string>`.
  - Outputs: `Files: SetSignal<RelativeFile>`.
- **`DirectoryWatch`** — watches a directory tree; emits structural deltas (file added, file removed, file content changed).
- **`FileContent`** — reads file content for each file in an input set; emits the content as a parallel channel.

### 3.2 File-system sinks

Functions that consume channels and write to the file system.

- **`CopyTree`** — copies an input set of files into a destination directory; the destination tracks the input set.
  - Inputs: `Files: SetSignal<RelativeFile>`, `SourceRoot: ValueSignal<AbsoluteDir>`, `DestinationRoot: ValueSignal<AbsoluteDir>`.
  - Outputs: a `SetSignal<AbsoluteFile>` describing the resulting destination files (so downstream functions can react to them).
- **`WriteFile`** — writes a `SequenceSignal<LogLine>` (or `SequenceSignal<string>`) to a file.

### 3.3 Compilers and code generators

Functions that consume source channels and produce artifact channels. These are the largest single category of `gib` functions and the place ecosystem-specific packages will plug in.

- **`CSharpCompile`** — invokes Roslyn against an input source set and reference set; emits the resulting assembly, PDB, and diagnostics as channels.
  - Inputs: `Sources: SetSignal<AbsoluteFile>`, `References: SetSignal<AbsoluteFile>`, `AssemblyName: ValueSignal<string>`, `TargetFramework: ValueSignal<string>`, `Configuration: ValueSignal<string>`, `OutputDir: ValueSignal<AbsoluteDir>`.
  - Outputs: `Assembly: ValueSignal<AbsoluteFile>`, `PdbFiles: SetSignal<AbsoluteFile>`, `Diagnostics: SequenceSignal<Diagnostic>`, `Log: SequenceSignal<LogLine>`.
- **`DotnetSdkRefs`** — given a target framework, emits the corresponding reference-assembly set as a `SetSignal<AbsoluteFile>`.
- *(Additional language-specific compile functions ship in their own packages and are pulled in via Layer 4 / DSL imports.)*

### 3.4 Test runners

- **`XUnitRun`** — runs xUnit against an assembly; emits a `ValueSignal<TestResult>` and a per-test `SequenceSignal`.
  - Inputs: `Assembly: ValueSignal<AbsoluteFile>`, `Filter: ValueSignal<string>`.
  - Outputs: `Result: ValueSignal<TestResult>`, `Log: SequenceSignal<LogLine>`.

### 3.5 Packaging

- **`ZipPack`** — packs an input file set into a zip archive at a specified output path.
  - Inputs: `Inputs: SetSignal<AbsoluteFile>`, `OutputFile: ValueSignal<AbsoluteFile>`.
  - Outputs: `Archive: ValueSignal<AbsoluteFile>`.

### 3.6 Combinators

Functions that operate purely on channels — no file-system or external side effects — to express conditions, joins, and transformations declaratively.

- **`When`** — gates a `then` channel on a predicate evaluated against a `gate` channel. While the predicate holds, `When` forwards `then` as its output; otherwise its output is empty.
- **`Concat`** — concatenates `ValueSignal<string>`s into a single derived `ValueSignal<string>`.
- **Set/sequence operators** — set union (`+` in the DSL), filtering, projection, and similar combinators expressed as functions on channels.

The combinator family is intentionally small; richer expressions are built by composing combinators rather than adding new ones.

### 3.7 DSL loader

- **`LoadProject`** — loads a project file and dispatches it to a registered DSL processor (Layer 6).
  - Inputs: `Path: ValueSignal<AbsoluteFile>` plus any parameter channels declared by the loaded module.
  - Outputs: the loaded module's exports, surfaced as the `LoadProject`'s outputs.

`LoadProject` is the bridge between Layer 5 and Layer 6: it is itself a Layer 5 function, but its job is to invoke a Layer 6 DSL processor.

---

## 4. Status

Every entry above is informative. Function URIs, exact schemas, and behavioral details will be pinned in a future normative revision once the prototype implementation has stabilized them. Until then, implementations of these functions ship with their own `FunctionMetadata` (Layer 1 §3) and that schema is the authoritative description.

DSL surfaces (Layer 6) consume these schemas via the spec's metadata operation (Layer 0 §2.3); they do not depend on this document for typing information.
