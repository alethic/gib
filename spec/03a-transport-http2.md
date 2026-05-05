# Layer 3a — HTTP/2 Transport (Normative Reference)

> **Status:** normative. **Reference transport.**
> **Depends on:** Layer 0 (Core Model), Layer 1 (Type System), Layer 2 (Function Host).
> **Consumed by:** Layer 4 (plugin orchestration); referenced by Layer 3b/3c as the parity benchmark.

This document binds the abstract operations of Layer 0 to **HTTP/2**. It is the *reference* transport: an implementation supporting only this binding is the recommended baseline for `gip` interoperability, and other Layer 3 bindings are checked for behavioral parity against it.

The choice of HTTP/2 is deliberate. The Layer 0 liveness model requires many concurrent long-lived requests against a single host (one per live `Invoke`, one per live `Subscribe`); HTTP/2's multiplexed streams over a single TCP connection are exactly what is needed to make this practical. HTTP/1.1 is **not** a conforming carrier.

---

## 1. URI scheme and shape

The HTTP/2 binding uses standard `http://` and `https://` URIs. `https://` is RECOMMENDED for any host reachable across a trust boundary; `http://` is permitted for in-machine use (loopback, Unix domain sockets via `http+unix://`, named pipes via `http+npipe://`).

The host's base endpoint URI (Layer 2 §1) is the URI's scheme + authority + base path. The base is definitional: it is the prefix the host uses for the URIs it issues (function endpoints, channel endpoints, well-known endpoints) and the anchor at which its own `.well-known/gip/` set lives. The spec does not constrain what else may live on the same authority. A single process MAY serve any number of unrelated `gip` bases on the same authority (each with its own well-known set), and MAY serve non-`gip` resources at sibling paths; from `gip`'s point of view those are simply different hosts, or not hosts at all.

The only path layout this binding mandates beneath the base is the `.well-known/gip/` prefix reserved by Layer 2 §3.

---

## 2. Operation bindings

### 2.1 `Invoke` is `POST` to the function endpoint

A Layer 0 `Invoke(functionUri, inputs)` is performed as an HTTP/2 `POST` request to `functionUri`.

- **Request body** — a single Protobuf message (Layer 1 §4.1) containing the named inputs: for each input the function declares, either a channel URI (string) to bind, or an inline signal value lifted into a constant channel.
- **Response** — the response is **streamed** as a sequence of Protobuf messages. The first message is the named output channel URIs that the host has just allocated. The response stream then **stays open** for the entire lifetime of the function instance, and MAY carry further messages defined by this binding (for example, a terminating exception payload per §4.2).

No further messages are required on the stream during normal operation; in the steady state the stream exists solely to keep the request open. The host MUST NOT close the response while the instance is live. The client closes the response (cancelling the HTTP/2 stream) when it wants the instance to stop.

This is the key invariant of the HTTP/2 binding:

> The in-flight HTTP/2 stream of an `Invoke` *is* the reference that keeps the function instance alive. There is no separate liveness protocol layered on top.

When the client cancels the stream, completes the request, or the underlying connection is lost, the host MUST treat the `Invoke`'s reference as dropped and apply Layer 0 §5.

### 2.2 `Subscribe` is `GET` to the channel endpoint

A Layer 0 `Subscribe(channelUri)` is performed as an HTTP/2 `GET` request to `channelUri`.

- **Request** — no body and no binding-defined headers beyond standard HTTP. The subscription always begins at the beginning of the channel's currently retained history (Layer 0 §6.2).
- **Response** — a long-lived streaming response. Each frame in the response body is one Protobuf signal message (or a small batch of them; framing rules below) in the channel's declared signal type.

The response stream stays open until either the subscriber cancels it or the channel is released (Layer 0 §5.2). When the host releases a channel for normal liveness reasons (no more references), open `Subscribe` streams MUST end with a normal end-of-stream marker. When the channel is terminated abnormally (Layer 0 §7.2), open `Subscribe` streams MUST end with an exception payload (§4 below).

> The in-flight HTTP/2 stream of a `Subscribe` is one of the references that keeps the channel alive (Layer 0 §5.2 condition 2).

### 2.3 Metadata is `GET` to `<resource-uri>/metadata`

A Layer 0 `GetFunctionMetadata(functionUri)` or `GetChannelMetadata(channelUri)` is performed as an HTTP/2 `GET` to a sibling URI of the form `<resource-uri>/metadata`.

The `Accept` header is required on every metadata `GET`: it selects the encoding of the metadata response (§3). The response body is a single message — a `FunctionMetadata` for a function, or a `ChannelMetadata` for a channel — encoded per the negotiated `Accept`. Metadata responses are NOT streamed and MUST NOT keep any reference alive (Layer 0 §2.3).

The resource URI itself (without `/metadata`) only ever performs the live operation. On a channel URI that means `GET` is `Subscribe` (§2.2). On a function URI that means `POST` is `Invoke` (§2.1); a `GET` against a function URI is also permitted as a convenience form of `Invoke` for functions that take no inputs, with no request body and the same streamed response shape as the `POST` form.

---

## 3. Encoding negotiation

### 3.1 No encoding hint in URIs

Nothing in any URI specifies the encoding on the wire. URIs name the host, the resource, and (via scheme) the transport; encoding is selected per-request via HTTP's standard `Accept` and `Content-Type` headers.

### 3.2 Protobuf is the baseline

Every HTTP/2-binding host MUST accept and produce Protobuf (Layer 1 §4.1) for every request body and response body.

The Protobuf media type is `application/x-gip-protobuf` (provisional; the exact spelling is to be pinned alongside the `.proto` definitions of Layer 1 §4.1). A request without an explicit `Accept` is treated as `Accept: application/x-gip-protobuf`. A request without an explicit `Content-Type` on a body is treated as `Content-Type: application/x-gip-protobuf`.

### 3.3 Alternative encodings

A host MAY additionally accept and produce alternative encodings (Layer 1 §4.2). The metadata response media type pair, when an alternative encoding is offered, is parameterized:

- `application/x-gip-protobuf; kind=metadata` — the Protobuf form of a metadata response (default).
- `application/x-gip-json; kind=metadata` — a JSON projection (RECOMMENDED if any JSON projection is offered; not required).

A host MUST honor the standard HTTP content-negotiation failure modes and MUST NOT silently substitute an encoding the client did not accept:

- If the request carries an `Accept` whose media types the host cannot satisfy for the addressed resource, the host MUST fail the request with `406 Not Acceptable`.
- If the request carries a `Content-Type` the host does not support for the addressed resource, the host MUST fail the request with `415 Unsupported Media Type`.

A host that supports only the Protobuf baseline therefore rejects requests for non-Protobuf media types per the rules above; it MUST NOT downgrade the response to Protobuf when the client's `Accept` excluded it.

---

## 4. Failure mapping

### 4.1 Failure during `Invoke`

If a function fails before its output channel URIs have been produced (Layer 0 §7.2), the `POST` MUST fail:

- **Status code** — a `5xx` for an internal/unexpected failure of the function or host; a `4xx` for a request-shape problem the client could correct.
- **Body** — a single Protobuf exception payload (Layer 1 §4.3), encoded per the negotiated `Content-Type`.

There are no output channel URIs to subscribe to in this case; the failed `POST` is the entire interaction.

### 4.2 Failure during the running phase

If a function fails after `Invoke` has returned (i.e. after the initial output-URIs message has been written to the response stream), the host can no longer signal failure by failing the `POST` itself: the response status and headers have already been sent. The `POST`'s response stream is still open — it remains so for the lifetime of the instance (§2.1) — and the function's output channels remain the live carriers of further state. The host MUST:

1. Release the function instance per Layer 0 §5.1 and §5.2.
2. Terminate the `POST`'s response stream with an **exception trailer**: an HTTP/2 trailing frame carrying the Protobuf exception payload (Layer 1 §4.3).
3. Terminate every open `Subscribe` `GET` against any of the instance's output channels with an exception trailer of the same form.

The trailer MUST be distinguishable from a normal end-of-stream so subscribers can surface the exception to their callers (Layer 0 §7.2). A host that cannot deliver trailers (for example, a degraded HTTP/2 implementation) MUST instead close the stream with an HTTP/2 `RST_STREAM` carrying a binding-defined error code, and MAY supplement that with a final framed exception message immediately preceding the reset; this is a permitted fallback but not the recommended form.

### 4.3 Failure during a `Subscribe` other than function-driven termination

If the host fails to serve a `Subscribe` for reasons unrelated to the producing function, the host MUST end the stream with an exception trailer per §4.2. The exception's type identifier (Layer 1 §4.3) MUST identify the cause unambiguously.

---

## 5. Liveness via held-open streams

To restate, because it is the heart of the binding:

| Layer 0 reference | HTTP/2 representation |
|---|---|
| The producing function instance's reference to its output channels | The `POST` response stream of the `Invoke` that created the instance |
| A subscriber's reference to a channel | The `GET` response stream of that `Subscribe` |
| Reference dropped | Stream cancelled, completed, or connection lost |

A host MUST observe these stream events and apply Layer 0 §5 immediately. A host MAY use HTTP/2 `PING` frames or other transport-level liveness checks to detect dead peers more aggressively, provided the externally observable behavior matches Layer 0.

A host MUST NOT introduce a separate "keep this instance alive" RPC, header, or sentinel. The stream is the reference.

---

## 6. Connection management

A client MAY multiplex any number of `Invoke`s, `Subscribe`s, and metadata requests over a single HTTP/2 connection. Hosts MUST accept connection reuse and MUST NOT impose per-connection limits below what HTTP/2's `SETTINGS_MAX_CONCURRENT_STREAMS` already governs.

A host MUST tolerate a peer setting `SETTINGS_MAX_CONCURRENT_STREAMS` to a value smaller than the number of concurrent live operations the peer would otherwise want; the peer is responsible for opening additional connections in that case.

Idle connections (no streams active) MAY be closed by either side at any time; this MUST NOT affect the liveness of any function or channel that no longer has open streams against it (it is already eligible for release per Layer 0 §5).

---

## 7. Conformance

A Layer 3a conforming binding MUST:

1. Use HTTP/2 (RFC 9113) as its transport. HTTP/1.1 is NOT a conforming carrier.
2. Bind `Invoke`, `Subscribe`, and metadata operations as defined in §2.
3. Negotiate encoding per §3, with Protobuf as the baseline.
4. Map failures per §4.
5. Honor liveness via stream lifetime per §5.
6. Tolerate connection multiplexing per §6.

A Layer 3a conforming host (a Layer 2 host that serves this binding) inherits every requirement of Layer 2 §5. The HTTP/2 binding is the recommended way to discharge those requirements and the benchmark against which other transports demonstrate parity.
