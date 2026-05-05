# `gip` Specification

This directory contains the formal specification of `gip` — the function/channel/signal substrate that `gib` is built on.

The specification is deliberately layered. Each document depends only on the layers below it and is consumed by the layers above. A second-language implementation can be built by reading these documents in order, without reference to the C# code in this repository.

## Layers

| # | Document | Scope |
|---|----------|-------|
| 0 | [`00-core-model.md`](00-core-model.md) | Abstract data model and behavioral contract: functions, channels, signals, URIs, `Invoke`/`Subscribe`/metadata, liveness, history, failure. No wire format, no transport. |
| 1 | [`01-type-system.md`](01-type-system.md) | The signal type system (`ValueSignal<T>`, `SetSignal<T>`, `SequenceSignal<T>`, framing markers), the `FunctionMetadata` description, and Protobuf as the normative encoding. |
| 2 | [`02-function-host.md`](02-function-host.md) | What it means to *be* a Function Host. Conformance requirements: at least one transport, the `.well-known/gip/resolve` function, endpoint exposure, liveness honoring, metadata. Defines Orchestrator Hosts. |
| 3 | Function Transport Specifications (one per transport) | |
| 3a | [`03a-transport-http2.md`](03a-transport-http2.md) | **Normative reference transport.** `gip` over HTTP/2: `POST` = `Invoke`, `GET` = `Subscribe`, content negotiation, request lifetime as liveness, error mapping. |
| 3b | [`03b-transport-grpc.md`](03b-transport-grpc.md) | `gip` over gRPC. Demonstrates parity with the HTTP/2 reference. |
| 3c | [`03c-transport-inproc.md`](03c-transport-inproc.md) | In-process transport. Degenerate binding for embedding. |
| 4 | [`04-plugin-orchestration.md`](04-plugin-orchestration.md) | Host manifests, plugin directory layout, launch & handshake protocol, resolver chaining rules, remote plugin providers. Built on Layer 2 + at least one Layer 3 binding. |
| 5 | [`05-gib-functions.md`](05-gib-functions.md) | The `gib` build-function library. Catalog of `CSharpCompile`, `Glob`, `LoadProject`, `XUnitRun`, etc., and their schemas. Not part of `gip`. |
| 6 | [`06-dsls.md`](06-dsls.md) | DSL specifications (one document per DSL: C# `*.gibcs`, YAML, …). How source text maps to Layer 0 graph operations. Not part of `gip`. |

## Conformance

An implementation claiming `gip` conformance MUST satisfy Layers 0, 1, 2, and at least one Layer 3 transport. Layer 3a (HTTP/2) is the reference against which other transports are checked for behavioral parity; an implementation supporting only Layer 3a is the recommended baseline for interoperability.

Layers 4–6 are independently versioned. An orchestrator targeting Layer 4 MUST implement at least one Layer 3 transport and conform to Layer 2.

## Document conventions

- The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** are to be interpreted as described in [RFC 2119](https://www.rfc-editor.org/rfc/rfc2119) and [RFC 8174](https://www.rfc-editor.org/rfc/rfc8174).
- Italicized terms on first use are defined in the layer where they appear; subsequent layers cite the defining layer rather than redefining them.
- Code examples are illustrative unless explicitly marked normative.
