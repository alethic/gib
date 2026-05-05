# Layer 2 — Function Host

> **Status:** normative.
> **Depends on:** Layer 0 (Core Model), Layer 1 (Type System).
> **Consumed by:** Layers 3 (transport bindings), 4 (plugin orchestration).

This document defines what it means to *be* a Function Host: the conformance requirements every host MUST satisfy regardless of the transport(s) it speaks. Layer 0 defines the abstract model; this layer defines the obligations a process takes on when it claims to serve that model.

It also defines the **Orchestrator Host**: a host that owns running pipelines and (typically) coordinates other hosts, but is, at the spec level, just a Function Host with additional responsibilities.

---

## 1. What a Function Host is

A **Function Host** is a process that serves URIs for some set of function types, function instances, and channels, and that answers Layer 0's three operation families (`Invoke`, `Subscribe`, metadata) for those URIs over at least one Layer 3 transport.

A host is identified to the outside world by its **base endpoint URI**: a URI naming the transport and authority at which the host is reachable. Every endpoint URI a host serves MUST be a path-extension of that base URI, and every URI a host hands out (channel URIs, function instance URIs, well-known URIs) MUST itself be an endpoint URI rooted at that base.

There is no notion of "the" Function Host. A pipeline MAY span any number of hosts, related to each other only through URIs and the operations of Layer 0.

---

## 2. Conformance requirements

A Function Host MUST satisfy every requirement in this section.

### 2.1 At least one transport

A host MUST serve its operations over at least one Layer 3 transport binding. The set of transports a host speaks is not part of its identity; the same host MAY serve the same URIs over multiple transports concurrently, in which case each transport's binding produces equivalent behavior for the same operation against the same URI.

### 2.2 Endpoint exposure for functions

For every function type the host serves, the host MUST expose an endpoint URI at which:

- `Invoke` (Layer 0 §2.1) instantiates the function.
- `GetFunctionMetadata` (Layer 0 §2.3) returns the function's `FunctionMetadata` (Layer 1 §3) without instantiating it.

The shape of a function endpoint URI (how the function type's identity is encoded into a path, query, or other component beneath the base URI) is defined by the Layer 3 transport binding the host serves it over. A host MUST keep the URI it hands out for a given function type stable for as long as that function type remains available on this host.

### 2.3 Endpoint exposure for channels

For every channel the host owns, the host MUST expose a channel endpoint URI at which:

- `Subscribe` (Layer 0 §2.2) attaches a subscriber.
- `GetChannelMetadata` (Layer 0 §2.3) returns the channel's `ChannelMetadata` (Layer 1 §3.1).

Channel endpoint URIs MUST be stable for the lifetime of the channel and MUST NOT be reused after the channel is released (Layer 0 §3.3, §5.2).

### 2.4 Liveness honoring

A host MUST honor Layer 0 §5 against its transport(s):

- The host MUST detect when an `Invoke` operation ends (cancellation, completion, transport disconnect) and MUST treat that event as the function instance's reference being dropped.
- The host MUST detect when a `Subscribe` operation ends and MUST treat that event as that subscription's reference being dropped.
- The host MUST release function instances and channels promptly once no held-open operation references them.

The host MUST NOT introduce its own out-of-band keep-alive or lease mechanism that overrides transport-observable liveness. Implementations MAY perform best-effort detection optimizations (for example, transport-level pings) provided the externally observable behavior matches §5 of Layer 0.

### 2.5 History and compaction

A host MUST act as the authoritative source of history (Layer 0 §6.1) for every channel it owns and MUST honor the late-joiner contracts of Layer 1 §2.1–§2.3 against whatever compaction policy it applies. A host MUST NOT delegate history responsibility to subscribers.

### 2.6 Failure delivery

A host MUST surface function failures according to Layer 0 §7 and Layer 1 §4.3. Specifically:

- Failures during `Invoke` MUST cause `Invoke` itself to fail with an exception payload.
- Failures after `Invoke` has returned MUST terminate every open `Subscribe` against the affected instance's outputs with an exception payload distinguishable from normal end-of-stream.

### 2.7 Encoding

A host MUST support Protobuf (Layer 1 §4.1) as a usable encoding on every transport it serves. A host MAY additionally support alternative encodings (Layer 1 §4.2) selectable by transport-level negotiation; it MUST NOT make any alternative encoding the *only* available one.

### 2.8 Self-description via well-known functions

A host MUST expose the well-known functions defined in §3 below at fixed paths beneath its base endpoint URI. The well-known function set is part of host conformance, not a separate protocol.

---

## 3. Well-known host functions

A host's self-description is exposed as **functions on the host itself**, not as a separate RPC vocabulary. Every host MUST reserve the path prefix `.well-known/gip/` beneath its base endpoint URI for this purpose, and MUST serve, for each well-known function defined here, an ordinary function endpoint URI of the form `<base>/.well-known/gip/<name>`.

These functions are reached the same way every other function is reached: their schemas are retrieved via `GetFunctionMetadata`, and they are invoked via `Invoke`, over the host's normal transport(s).

The present revision of this layer defines exactly one well-known function. Future revisions MAY add more (for example, enumeration, manifest, health, graceful shutdown); a host MUST tolerate `GetFunctionMetadata` against a `.well-known/gip/` URI it does not implement by failing the metadata operation cleanly, not by 404ing in some transport-specific way that callers cannot interpret.

### 3.1 `resolve`

Every host MUST serve a function at `<base>/.well-known/gip/resolve` with the following schema:

- **Input:** one input named `ref`, of shape `Value`, with payload type `FunctionRef`.
- **Output:** one output named `resolved`, of shape `Value`, with payload type `FunctionRef`.

`resolve` is the host's contribution to the resolver chain. Because both sides are channels, resolution is dynamic and reactive: the caller updates the input value when it wants a different URI resolved, and `resolve` emits a new `FunctionRef` whenever its answer changes.

A host's `resolve` implementation MUST behave according to one of three rules for any given input `FunctionRef`:

1. **Resolve directly.** If the URI in the input names something this host already serves — most obviously its own well-known functions, but also any function type the host is willing to instantiate — `resolve` MUST emit a `FunctionRef` whose URI is a directly dialable endpoint URI on this same host. That `FunctionRef` is the final answer for that input, and updates to the input cause re-resolution as appropriate.

2. **Chain to another `resolve`.** If the URI is one this host does not itself serve, and the host has another `resolve` to chain into (typically the orchestrator's; see §4.3), `resolve` MUST `Invoke` that downstream `resolve` with the same `FunctionRef` as input and **pipe the downstream `resolve`'s output channel straight through as its own output**. It MUST NOT emit an intermediate `FunctionRef` pointing at the next resolver. From the caller's point of view, only the final, directly-dialable `FunctionRef` ever appears on the output channel.

3. **Pass through unchanged.** If the URI is one this host does not serve and the host has no further `resolve` to chain into (it is at the end of the chain), `resolve` MUST emit the input `FunctionRef` back unchanged. Callers treat such a pass-through as a directly-dialable endpoint URI (Layer 0 §3.1) and dial it; if the URI's scheme is not one any Layer 3 binding can dial, the subsequent operation simply fails the way any unreachable endpoint would.

Resolution MUST be per-`FunctionRef`. Resolving a batch of URIs is just invoking `resolve` once per URI; `resolve` itself only ever deals with one reference at a time.

---

## 4. Orchestrator Hosts

An **Orchestrator Host** is a Function Host that additionally takes responsibility for owning one or more pipelines: holding the references that keep their root functions alive, deciding where sub-functions get instantiated, and managing the lifecycle of any plugin hosts (Layer 4) it launches.

Orchestrator Hosts are not a new entity in the URI graph. They are Function Hosts plus extra responsibilities. Every conformance requirement of §2 and §3 applies to an Orchestrator Host unchanged.

### 4.1 Embedding versus delegation

An Orchestrator Host MAY:

- **Embed a Function Host in-process.** The trivial deployment is a single executable whose orchestration logic and Function Host behavior share a process. URIs the embedded host owns resolve in-process; the orchestrator MAY still dial out to other hosts for URIs it does not own.
- **Delegate entirely.** A thin orchestrator with no embedded host is permitted; it holds references that keep the pipeline alive and dials out to other Function Hosts for everything.
- **Mix.** Most realistic deployments will: cheap glue functions run in the embedded host, while expensive or specialized functions run on dedicated Function Hosts the orchestrator dials out to.

The choice MUST NOT be visible to callers other than as the URIs the orchestrator hands out.

### 4.2 Pipeline ownership

A pipeline lives as long as the orchestrator keeps holding references to its root function's outputs (and as long as connected clients keep their subscriptions open). When the orchestrator drops those references and the last client disconnects, the pipeline winds down naturally on whatever Function Hosts it was using, by the ordinary liveness rules of Layer 0 §5. Orchestrators MUST NOT introduce additional pipeline-level lifetime mechanisms beyond holding operations open.

### 4.3 The orchestrator's `resolve` is the chain root

An Orchestrator Host's `resolve` (§3.1) is the natural root of the resolver chain for every host it manages. When a child Function Host's `resolve` chains (rule 2 of §3.1), it typically chains into the orchestrator's `resolve`.

To make this work without a separate discovery protocol, an orchestrator that launches a child host (Layer 4) MUST pass that child its own base endpoint URI as a launch parameter. The child host derives `<orchestrator-base>/.well-known/gip/resolve` from that URI and invokes it as the next link in the chain.

The orchestrator's `resolve` itself is just a `resolve` like any other: it answers directly for URIs it serves, chains for URIs it does not (typically into remote plugin providers; see Layer 4), and passes through at the end of its chain.

### 4.4 No new transports

An Orchestrator Host MUST NOT introduce orchestration-specific transports or operations. Everything an orchestrator does to a host it manages — launching it (Layer 4), resolving through it, invoking functions on it, observing channels on it — MUST happen via Layer 0 operations over Layer 3 transports.

---

## 5. Conformance summary

A Layer 2 conforming Function Host MUST:

1. Have a stable base endpoint URI (§1).
2. Serve its operations over at least one Layer 3 transport (§2.1).
3. Expose function and channel endpoint URIs as defined in §2.2 and §2.3.
4. Honor liveness as observed via the transport (§2.4).
5. Honor history and compaction as the channel owner (§2.5).
6. Surface failures per §2.6.
7. Support Protobuf as a usable encoding on every transport it serves (§2.7).
8. Expose at least the `resolve` well-known function at `<base>/.well-known/gip/resolve`, with the schema and behavior defined in §3.1.

A Layer 2 conforming Orchestrator Host MUST additionally:

9. Honor §4.2 (pipeline lifetime falls out of held-open operations).
10. When it launches child hosts (Layer 4), hand each child its own base endpoint URI so that the child's `resolve` can chain into the orchestrator's (§4.3).
11. Avoid introducing orchestration-specific transports or operations (§4.4).
