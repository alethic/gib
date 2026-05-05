# Layer 0 — Core Model

> **Status:** normative.
> **Depends on:** nothing.
> **Consumed by:** every other layer.

This document defines the abstract data model and behavioral contract of `gip`. It is deliberately transport-free and encoding-free: it talks only about *what* the system is, not *how* its bytes travel. Layer 1 pins down the concrete signal type system and the encoding; Layer 2 defines what it means to *be* a host serving this model; Layer 3 documents pin those operations onto specific wire protocols.

---

## 1. Entities

`gip` defines exactly four kinds of entity. Every implementation MUST recognize these and only these as the addressable primitives of the model.

### 1.1 Function

A **function** is a long-running reactive computation that consumes input channels and produces output channels.

- A function has a *type*, identified by a URI (see §3) and described by a `FunctionMetadata` (Layer 1).
- A function has zero or more *instances*. Each instance is a separate running computation with its own input and output channels.
- A function instance is created by an `Invoke` operation (§4.1) and lives until its liveness conditions (§5) are no longer satisfied.
- A function MAY internally instantiate other functions ("child functions") via `Invoke`. Child functions are not part of their parent's identity; they are independent instances that the parent happens to reference.

A function is **not** a one-shot call. It is a process. A given `Invoke` against a function URI does not "compute a value and return"; it instantiates the function, wires its inputs and outputs to the channels described in the call, and the instance then runs for as long as the graph keeps it alive.

### 1.2 Channel

A **channel** is a typed, ordered stream of *signals* (Layer 1) flowing from one function instance to its subscribers.

- Every channel is owned by exactly one host: the host on which the producing function instance runs.
- Every channel has a stable URI (§3) on its owning host.
- Every channel has a single, fixed signal type, declared by the producing function's `FunctionMetadata`.
- The channel is event-sourced: signals describe *changes over time*, and a subscriber that reads the retained history followed by live signals MUST end up in the same logical state as a subscriber that has been attached the whole time (§6).
- A channel is *not* the implicit return value of a call. It is an independent, URI-addressable resource. Callers pass channel URIs *into* `Invoke` as inputs and receive channel URIs *out* of `Invoke` as outputs; channels can be allocated, passed around, and connected without ever being routed through the function whose outputs they happen to be.

### 1.3 Signal

A **signal** is a single typed message published on a channel. Layer 1 defines the type system that constrains what signals may look like; Layer 0 only requires that:

- Every signal belongs to exactly one channel.
- Every signal has a position in that channel's order, monotonic with respect to publication.
- Signals are immutable once published.

### 1.4 `FunctionRef`

A **`FunctionRef`** is an opaque reference to a function. Structurally it is a thin wrapper around a single URI. It carries no schema, no type information, and no transport state — it is purely an address.

`FunctionRef` exists as a distinct entity (rather than just "a URI") so that resolver chains (Layer 2 §`resolve`) can flow `FunctionRef` values through `ValueSignal<FunctionRef>` channels without conflating "a logical name" with "a string." A `FunctionRef` whose URI is a *logical* URI (§3.2) has not yet been resolved; a `FunctionRef` whose URI is an *endpoint* URI (§3.1) is directly dialable.

---

## 2. Operations

`gip` defines exactly three operation families. Every implementation MUST support these and only these as the externally observable behaviors of a host.

### 2.1 `Invoke`

`Invoke(functionUri, inputs) → outputs`

- **Input:** a function endpoint URI; a (possibly empty) collection of named inputs, each of which is either a channel URI to bind or an inline signal value to lift into a constant channel.
- **Output:** a collection of named output channel URIs corresponding to the function's declared outputs.
- **Semantics:** instantiate the function, wire its declared inputs to the supplied channel URIs (or to constant channels for inline values), allocate channels for its declared outputs, and return their URIs to the caller. The instance begins running and continues until its liveness conditions (§5) are no longer satisfied.

The spec does not require `Invoke` to be idempotent, and callers MUST NOT assume it is. Whether two `Invoke`s with structurally identical inputs produce two independent instances or share a single underlying instance is an implementation choice: a host MAY deduplicate calls and return URIs that refer to an existing instance's output channels, provided that doing so preserves every liveness, history, and failure guarantee this specification makes to each caller individually. Reuse that a caller wants to *guarantee* is something it arranges explicitly, by holding onto an existing channel URI and passing it around, rather than by relying on `Invoke` to dedupe on its behalf.

### 2.2 `Subscribe`

`Subscribe(channelUri) → stream of signals`

- **Input:** a channel endpoint URI.
- **Output:** a stream of signals delivered in channel order, beginning at the beginning of the channel's currently retained history (§6.2) and continuing until the subscriber disconnects or the channel is released (§5).
- **Semantics:** attach the caller as a subscriber to the named channel. The owning host MUST deliver the channel's currently retained history followed by every live signal published thereafter, in order, until the subscription ends.

### 2.3 Metadata

`GetFunctionMetadata(functionUri) → FunctionMetadata`
`GetChannelMetadata(channelUri) → ChannelMetadata`

- **Input:** a function or channel endpoint URI.
- **Output:** the corresponding Layer 1 `FunctionMetadata` or `ChannelMetadata` descriptor.
- **Semantics:** introspect the named entity. Metadata operations MUST NOT have side effects on the function instance or channel they describe and MUST NOT count as references for liveness purposes.

A `FunctionRef` carries no schema. Anything a caller wants to know *about* the function it refers to is fetched via `GetFunctionMetadata` against that `FunctionRef`'s endpoint URI.

---

## 3. URIs

`gip` distinguishes two kinds of URIs. The distinction is part of the model, not a transport detail.

### 3.1 Endpoint URI

An **endpoint URI** identifies a concrete addressable thing — a function or a channel — on a specific host, reachable over a specific transport. It is *physical*: it names both a transport and a destination on that transport, and it can be dialed directly without further resolution.

The scheme of an endpoint URI MUST be one a Layer 3 transport binding claims. The path MUST identify either a function or a channel on the host designated by the URI's authority.

Endpoint URIs are the only URIs that the operations of §2 act on directly. Note, however, that a caller in possession of a URI generally cannot tell from the URI alone whether it is already an endpoint URI or still a logical one; the resolver chain (Layer 2) is defined so that resolving an already-resolved endpoint URI is a no-op, and callers are expected to resolve any URI they did not themselves obtain from a successful resolution before using it with the operations of §2.

### 3.2 Logical URI

A **logical URI** identifies a function (or, less commonly, a channel) by name rather than by physical address. It says nothing about which host serves it or which transport reaches it. The canonical scheme is `gib:`, but any non-transport scheme may serve as a logical URI for resolver purposes.

A logical URI is not directly dialable. It MUST be resolved (Layer 2) into an endpoint URI before any operation can be performed against it.

### 3.3 Stability

Endpoint URIs are situational: the same logical URI MAY resolve to different endpoint URIs in different processes, on different machines, or at different times, and callers MUST NOT assume that an endpoint URI obtained in one context is meaningful in another. Because a caller generally cannot distinguish a logical URI from an endpoint URI by inspection, any URI not known to have just been resolved SHOULD be passed through the resolver chain before use; resolving an already-resolved URI is defined to be a no-op.

---

## 4. Identity, equality, and aliasing

- Two `FunctionRef`s are equal iff their URIs are equal as URIs.
- Two channel endpoint URIs that compare equal as URIs identify the same channel and MUST yield the same signal stream when subscribed.
- Two function endpoint URIs that compare equal as URIs identify the same function on the same host. `Invoke`s against them are equivalent in addressing but, as stated in §2.1, do not alias each other's instances.
- A function instance has no externally visible URI of its own beyond the URIs of its output channels. Holding a reference to an instance, in spec terms, means holding either the `Invoke` operation that created it open (§5) or a `Subscribe` against one of its outputs.

---

## 5. Liveness

The lifetime of every function instance and every channel is determined entirely by the operations that hold it open. This specification defines no separate ownership protocol, no leases, no reference messages, and no `Close` or `Delete` operation; everything an implementation needs in order to manage liveness is expected to fall out of the transport's existing notion of "this operation is still in progress."

What "holding an operation open" actually looks like is transport-specific. On HTTP/2 it is literally an in-flight stream; other transports may need their own keepalive, lease-renewal, or session-tracking mechanism to project the same idea onto a wire that does not natively keep long-lived calls open. Layer 3 transport bindings are responsible for defining that projection; Layer 0 only requires that *some* such projection exist and that it accurately reflects whether the operation is still in progress from the caller's point of view.

### 5.1 Function instance liveness

A function instance is live as long as the `Invoke` operation that created it is still in progress. When the caller cancels or completes the `Invoke`, the instance is no longer guaranteed to be live and SHOULD stop within a reasonable time. The spec does not require instantaneous shutdown; an implementation MAY allow an instance to wind down, flush in-flight work, or remain briefly resident, but callers MUST NOT rely on it continuing to make progress past that point.

Once stopped, an instance SHOULD stop publishing signals to its output channels within a reasonable time. Subscribers MUST be prepared for the corresponding channels to no longer receive new signals from this instance and, eventually, to be released (§5.2).

### 5.2 Channel liveness

A channel is guaranteed to be live as long as at least one of the following holds:

1. The function instance that produces it is live (§5.1).
2. At least one `Subscribe` operation against its URI is in progress.

When neither holds, the spec makes no further guarantees about the channel: it MAY remain reachable for some time, or it MAY be released by its owning host at the host's convenience. Callers MUST NOT depend on a channel still being reachable after every operation referencing it has ended.

### 5.3 Host implications

A host MUST honor liveness as observed through its transport. Concretely:

- The transport MUST give the host a way to detect that an `Invoke` or `Subscribe` operation has ended (cancellation, disconnection, completion).
- The host MUST treat such an event as the corresponding reference being dropped for liveness purposes; once no held-open operation references a function instance or channel, the host is free to release it at its convenience.

Layer 3 transport bindings define how cancellation, disconnection, and completion are surfaced on their wire.

---

## 6. Channel history and compaction

### 6.1 Sender-owned history

The owning host of a channel is the authoritative source for that channel's history. It MUST retain enough history that any new `Subscribe` against the channel, replayed from the beginning of that retained history, reaches the same logical state as a subscriber that has been attached continuously.

The sender (the producing function, via the owning host) MAY *compact* history at any time, replacing an earlier prefix of signals with a shorter sequence that folds to the same current logical state. The contract is:

> A subscriber that reads the retained prefix and then follows live signals MUST end up in the same logical state as a subscriber that has been attached the whole time.

Compaction is not a separate operation. It is an internal action of the owning host, observable to subscribers only as the absence of signals they were not promised to receive. Subscribers MUST be written to react to signals as they arrive, not to a fixed historical position.

### 6.2 Subscribe replay

A `Subscribe` has no caller-selectable starting position. Every `Subscribe` begins at the beginning of the channel's currently retained history (as of the moment the subscription is established) and proceeds forward through that retained prefix and then through live signals in order.

The retained history a new subscriber sees is whatever the owning host currently retains after applying its compaction policy (§6.1). Two subscriptions established at different times against the same channel MAY therefore see different retained prefixes, but both MUST reach the same logical current state as a continuously attached subscriber once they have processed everything the host delivers.

### 6.3 Ordering

Within a channel, signals are totally ordered. Across channels, no ordering is implied.

---

## 7. Failure

Functions are expected to behave like ordinary functions in any reasonable language: they can succeed and they can fail. `gip` models failure as a thrown exception with the following invariants.

### 7.1 Exceptions are terminal

An unrecovered exception thrown by a function instance is terminal for that instance in the same sense that the natural completion of `Invoke` is terminal: the instance is on its way out. The instance MAY still wind down in an implementation-defined way — flushing buffered work, emitting any signals it had already produced, releasing resources — but it MUST NOT be treated as continuing to operate normally, and the implementation is under no obligation to keep it reachable beyond what §5 already requires for held-open operations.

### 7.2 Delivery to the caller

The exception MUST be delivered to the caller through the `Invoke` relationship that created the instance. Because `Invoke` is a long-running operation that does not return until the instance has terminated (§2.1), this is uniform: the `Invoke` operation completes by surfacing the exception to the caller in place of a normal completion. Any output channel URIs that the operation may have already exposed to the caller during its lifetime are subject to the ordinary liveness rules of §5; the implementation is not required to keep them reachable past the failure.

### 7.3 Cross-host propagation

When the failed instance and its caller live on different hosts, the owning host MUST surface the exception to the calling host through the in-flight `Invoke` or `Subscribe` operation (whichever is still live), with enough fidelity for the calling host to re-raise an equivalent exception in its own runtime. Layer 1 defines the exception payload's type system; Layer 3 transport bindings define how the payload is framed on their wire.

---

## 8. Conformance

A Layer 0 conforming implementation MUST:

1. Recognize functions, channels, signals, and `FunctionRef`s as the only addressable primitives.
2. Support exactly the operations in §2 and no other externally observable operations whose semantics overlap with them.
3. Distinguish endpoint URIs from logical URIs as defined in §3 and refuse non-endpoint URIs at operation boundaries.
4. Honor the liveness rules of §5 against whatever transport(s) it serves.
5. Honor the history and compaction contract of §6.
6. Honor the failure model of §7.

A Layer 0 implementation MAY add internal mechanisms (caches, indexes, compaction policies, scheduling) provided they are not externally observable except through the operations and liveness behaviors defined here.
