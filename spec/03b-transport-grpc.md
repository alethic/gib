# Layer 3b — gRPC Transport

> **Status:** normative (optional binding).
> **Depends on:** Layer 0, Layer 1, Layer 2.
> **Parity reference:** Layer 3a (HTTP/2).

This document binds the abstract operations of Layer 0 to **gRPC**. It is an optional binding; an implementation MAY support it in addition to (or instead of) HTTP/2, but Layer 3a remains the normative parity benchmark.

The gRPC binding exists because gRPC's server-streaming RPCs are an exceptionally natural fit for `Subscribe`, and because many ecosystems already have first-class gRPC tooling. Mechanically, gRPC runs over HTTP/2 itself, so the held-open-stream liveness model of Layer 3a applies essentially unchanged; this document only specifies what is different.

---

## 1. URI scheme

The gRPC binding uses the `grpc://` and `grpcs://` URI schemes (the latter for TLS). The authority and base path follow Layer 2 §1: every endpoint URI a host serves MUST be a path-extension of its base.

The gRPC binding does NOT define separate URIs for functions vs. channels at the URI-scheme level; the path layout convention from Layer 3a §1 applies unchanged. The gRPC service routes requests by URI path, not by service/method names.

---

## 2. Service definition

A conforming gRPC binding MUST expose exactly one service whose methods correspond to Layer 0's operations:

- `rpc Invoke(InvokeRequest) returns (stream InvokeResponse)` — server-streaming.
- `rpc Subscribe(SubscribeRequest) returns (stream Signal)` — server-streaming.
- `rpc GetFunctionMetadata(FunctionMetadataRequest) returns (FunctionMetadataResponse)` — unary.
- `rpc GetChannelMetadata(ChannelMetadataRequest) returns (ChannelMetadataResponse)` — unary.

Every request message carries the target endpoint URI as a field (typically the first field). The service implementation dispatches on that URI.

The exact `.proto` definitions are maintained alongside Layer 1's `.proto` files and incorporated by reference.

---

## 3. Operation bindings

### 3.1 `Invoke`

`Invoke` is a server-streaming RPC. The single client message is the `InvokeRequest` carrying the function endpoint URI and inputs (Layer 1 encoding). The first server-streamed message is the `InvokeResponse` carrying the named output channel URIs; the stream then **stays open** for the lifetime of the function instance, with no further messages, exactly as in Layer 3a §2.1.

Cancelling the call (gRPC's standard client cancellation) MUST be treated by the host as the `Invoke` reference being dropped (Layer 0 §5).

### 3.2 `Subscribe`

`Subscribe` is a server-streaming RPC. The single client message is the `SubscribeRequest` carrying the channel endpoint URI. Each server-streamed message is one Protobuf `Signal` in the channel's declared signal type. The subscription always begins at the beginning of the channel's currently retained history (Layer 0 §6.2).

Cancelling the call MUST be treated by the host as that subscription's reference being dropped.

### 3.3 Metadata

`GetFunctionMetadata` and `GetChannelMetadata` are ordinary unary RPCs and MUST NOT have side effects on the named entity (Layer 0 §2.3).

---

## 4. Encoding negotiation

gRPC's native content type is Protobuf, and the gRPC binding REQUIRES Protobuf (Layer 1 §4.1). Alternative encodings are out of scope for this binding; clients that want them MUST use the HTTP/2 binding (Layer 3a §3.3).

This is a deliberate simplification: gRPC's strength is its codec-agnostic framing of typed messages, and re-introducing per-call media-type negotiation would gain little over Layer 3a.

---

## 5. Failure mapping

### 5.1 Failure during `Invoke`

If a function fails before its output channel URIs have been produced, the server MUST end the `Invoke` stream with a non-`OK` gRPC status. The status MUST carry the Protobuf exception payload (Layer 1 §4.3) in a status detail of the corresponding message type. A status code of `INTERNAL` is appropriate for an unexpected function failure; `INVALID_ARGUMENT` for a request-shape problem; other codes per gRPC convention as applicable.

### 5.2 Failure during the running phase

If a function fails after `Invoke` has returned its initial output-URIs message, the host MUST:

1. Release the function instance per Layer 0 §5.
2. End every open `Subscribe` stream against the instance's outputs with a non-`OK` gRPC status carrying the Protobuf exception payload.

The `Invoke`'s own already-open stream MAY also be ended with a non-`OK` status carrying the exception, but this is redundant (the subscribers carry the failure) and not required.

### 5.3 Subscribe-only failures

A `Subscribe` failure unrelated to its producing function MUST end the stream with a non-`OK` status carrying the exception payload, per Layer 0 §7.

---

## 6. Liveness

The held-open-stream model of Layer 3a §5 applies unchanged: the gRPC stream of an `Invoke` is the function instance's reference, and the gRPC stream of a `Subscribe` is one of the channel's references. Standard gRPC client cancellation, deadline expiry, and connection loss MUST all be treated by the host as the corresponding reference being dropped.

gRPC keepalive (the standard HTTP/2 `PING`-based mechanism) MAY be used by either side to detect dead peers; it MUST NOT be used to keep a reference alive past the point at which the operation it represents has ended.

---

## 7. Conformance

A Layer 3b conforming binding MUST:

1. Expose the service in §2 over gRPC.
2. Bind the operations per §3.
3. Use Protobuf as the only encoding (§4).
4. Map failures per §5.
5. Honor liveness per §6 and Layer 3a §5.

A host that serves both Layer 3a and Layer 3b MUST yield equivalent observable behavior for the same operation against the same URI, modulo the encoding restriction of §4.
