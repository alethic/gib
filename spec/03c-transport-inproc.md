# Layer 3c — In-Process Transport

> **Status:** normative (optional binding).
> **Depends on:** Layer 0, Layer 1, Layer 2.
> **Parity reference:** Layer 3a (HTTP/2).

This document binds the abstract operations of Layer 0 to **direct in-process calls**. It is the trivial transport: when the caller and the host live in the same process, no wire is needed, and the spec's operations reduce to ordinary method calls and in-memory observations.

The in-process binding exists for two reasons:

1. **Performance.** Trivial deployments (a single orchestrator with an embedded Function Host — Layer 2 §4.1) should not pay for serialization, framing, or socket I/O on every `Invoke`.
2. **Reference behavior.** It is the simplest possible binding to implement and serves as a reference for testing higher-layer logic without involving any wire protocol.

It is a conformance requirement that the in-process binding produce **the same observable behavior** as Layer 3a for every operation. Anything that would be visible to a remote peer over HTTP/2 MUST be visible to an in-process peer through the in-process binding.

---

## 1. URI scheme

The in-process binding uses the `inproc:` URI scheme. An in-process URI is opaque to anyone outside the process that owns it; it MUST NOT appear in any context where it could be dialed by a peer that does not share memory with the owning host.

The recommended URI shape is:

```
inproc:<host-id>/functions/<...>
inproc:<host-id>/channels/<...>
inproc:<host-id>/.well-known/gip/<name>
```

where `<host-id>` is a process-local identifier the binding uses to route the URI to the correct host instance. A process MAY contain multiple in-process hosts; the `<host-id>` distinguishes them.

A host that serves the in-process binding MAY also serve other transports concurrently; in that case the same logical function or channel will have multiple endpoint URIs (one per transport), and the host MUST keep their behavior consistent.

---

## 2. Operation bindings

The in-process binding implements Layer 0's operations as direct method calls on host-side objects, dispatched by URI.

### 2.1 `Invoke`

`Invoke` is implemented as a method call that:

1. Resolves the function endpoint URI to a function-type implementation on the host.
2. Allocates output channel objects for that function's declared outputs.
3. Wires the function's input bindings to the supplied input channel references (or to constant channels for inline values).
4. Returns the output channel URIs to the caller.

The caller holds an "invocation handle" representing the in-flight `Invoke`. The function instance lives until that handle is disposed. Disposal MUST be treated as the `Invoke` reference being dropped (Layer 0 §5).

The invocation handle is the in-process equivalent of Layer 3a's held-open `POST` response stream.

### 2.2 `Subscribe`

`Subscribe` is implemented as a method call that:

1. Resolves the channel endpoint URI to a channel object on the host.
2. Returns a "subscription handle" representing the in-flight subscription, plus a way for the caller to consume signals (an observable, an async sequence, or equivalent).

The channel stays alive while the subscription handle is held (Layer 0 §5.2). Disposing the handle MUST be treated as that subscription's reference being dropped.

The subscription handle is the in-process equivalent of Layer 3a's held-open `GET` response stream.

### 2.3 Metadata

`GetFunctionMetadata` and `GetChannelMetadata` are ordinary synchronous method calls returning the corresponding Layer 1 descriptors. They MUST NOT have side effects on the named entity and MUST NOT count as references for liveness.

---

## 3. Encoding

The in-process binding does not serialize. Signals, schemas, and exception payloads cross the binding as in-memory objects of the appropriate Protobuf-generated types (or runtime-equivalent representations).

A binding implementation MAY pass references to mutable Protobuf message instances directly between caller and host **only if** it guarantees that neither side mutates the message after the call boundary. If that guarantee cannot be made (for example, because the caller and host are written in different runtimes sharing the same process via FFI), the binding MUST defensively copy.

The point of this rule is that the *observable* behavior of the in-process binding MUST match the wire bindings, where mutation after send is impossible by construction.

---

## 4. Failure mapping

### 4.1 Failure during `Invoke`

If a function fails before its output channels have been allocated, the `Invoke` method call MUST throw. The thrown exception MUST be either the original function exception or a wrapper that exposes its type, message, and cause faithfully. A binding MAY surface the original `Exception` object directly when caller and host are in the same runtime.

### 4.2 Failure during the running phase

If a function fails after `Invoke` has returned, the host MUST:

1. Release the function instance per Layer 0 §5.
2. Cause every active subscription handle against the instance's output channels to deliver an error to its consumer, distinguishable from normal end-of-stream.

The exact mechanism (an `OnError` callback, a thrown exception from the async sequence, a faulted task) is binding-implementation-defined; what is required is that the consumer can tell the channel ended abnormally and can retrieve the exception payload.

---

## 5. Liveness

The held-open-handle model is the direct in-process analog of Layer 3a §5:

| Layer 0 reference | In-process representation |
|---|---|
| The producing function instance's reference to its outputs | The invocation handle returned by `Invoke` |
| A subscriber's reference to a channel | The subscription handle returned by `Subscribe` |
| Reference dropped | The handle is disposed (or finalized as a fallback) |

A binding MUST treat handle disposal as immediate reference release. A binding MAY rely on garbage collection / finalization as a fallback when handles are leaked, but MUST NOT use it as the primary lifetime mechanism: deterministic disposal is required for the liveness rules of Layer 0 §5 to behave the way they would over HTTP/2.

---

## 6. Behavioral parity

A host that serves both Layer 3a and Layer 3c for the same logical functions MUST yield indistinguishable behavior to callers, modulo:

- Latency and throughput characteristics.
- The form of the URI (in-process URIs are not dialable from outside the process).
- The concrete shape of the exception object (an in-process binding MAY surface the original runtime exception; the wire binding MUST surface a reconstructed one from the Protobuf payload).

In particular, history, compaction, late-joiner contracts, framing, failure delivery, and liveness MUST behave identically.

---

## 7. Conformance

A Layer 3c conforming binding MUST:

1. Use `inproc:` URIs as defined in §1.
2. Implement Layer 0 operations as method calls per §2.
3. Honor in-memory message safety per §3.
4. Map failures per §4.
5. Honor liveness via handle disposal per §5.
6. Maintain behavioral parity with Layer 3a per §6.

A Layer 3c implementation MAY skip serialization, framing, and HTTP/2 entirely, but MUST NOT skip any externally observable Layer 0 or Layer 2 obligation.
