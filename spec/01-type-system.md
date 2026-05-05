# Layer 1 � Type System

> **Status:** normative.
> **Depends on:** Layer 0 (Core Model).
> **Consumed by:** Layers 2, 3, 4, 5, 6.

This document defines the type system used to describe functions and the signals that flow on their channels, and the normative encoding of those types on the wire. Layer 0 establishes that every channel has a fixed signal type and that every function has a `FunctionMetadata`; this layer says what those things are.

---

## 1. Design principles

The type system is intentionally small. It exists to satisfy four requirements:

1. **Channels carry signals; signals are encodable values.** The only thing the spec itself guarantees about a channel is that it carries an ordered stream of signals, and that each signal is a value of some type that can be encoded on the wire (�2). Everything else � what the signals *mean*, how they fold into state, whether they form a CRDT � is an agreement between the two endpoints of the channel.
2. **Standard shapes are conventions, not a closed universe.** This layer defines a small set of **standard signal shapes** (�3) � `ValueSignal<T>`, `SetSignal<T>`, `SequenceSignal<T>`, `ListSignal<T>`, `MapSignal<K,V>` � that capture commonly understood conflict-free replicated datatype patterns for the operations build pipelines most often need. They are normative *as definitions* (an implementation that claims to speak `SetSignal<T>` MUST behave the way �3.2 says), but they are NOT the only signal types a channel may carry. Functions MAY define and use entirely new signal types of their own; peers that do not understand those types simply cannot consume the channel meaningfully, the same way they cannot consume an unknown payload type.
3. **Description before consumption.** A `FunctionMetadata` fully describes a function's inputs, outputs, and signal types. A caller MUST be able to interact with a function it has never seen before by retrieving its `FunctionMetadata` (Layer 0 �2.3) and acting on the description alone.
4. **One normative encoding.** Protobuf is the normative wire encoding. Other encodings MAY exist (notably for human-readable debugging) but MUST be losslessly convertible to and from the Protobuf form.

---

## 2. Encoding

### 2.1 Normative encoding: Protobuf

The normative wire encoding for every signal and every `FunctionMetadata` is **Protocol Buffers (proto3)**.

- Every signal that flows on a channel MUST be a Protobuf message. Equivalently, every signal type referenced by a `FunctionMetadata` MUST resolve to a Protobuf message type.
- Each standard shape in �3 MUST be expressible as a Protobuf `oneof` over its vocabulary, with the payload field(s) typed as the channel's payload type(s) (`T`, or `K` and `V` for `MapSignal`). The `.proto` definitions for the standard shapes are maintained in the reference implementation and incorporated by reference.
- Non-standard signal types (�3.7) satisfy this requirement by being Protobuf messages of the function author's design; the spec imposes no further structural constraint on them.
- A `FunctionMetadata` MUST be expressible as a Protobuf message and MUST round-trip losslessly through that representation.

The exact `.proto` definitions are maintained in the reference implementation and incorporated by reference. Future revisions of this layer MAY pin specific Protobuf versions or codegen options; the present revision requires only proto3 compatibility.

#### 2.1.1 Canonical names of the standard shapes

The standard signal shapes defined in �3 are published under the Protobuf package `gib.v1` and have the following fully-qualified message names:

| Shape (�3)              | Fully-qualified Protobuf name |
| ----------------------- | ----------------------------- |
| `ValueSignal<T>`        | `gib.v1.ValueSignal`          |
| `SetSignal<T>`          | `gib.v1.SetSignal`            |
| `SequenceSignal<T>`     | `gib.v1.SequenceSignal`       |
| `ListSignal<T>`         | `gib.v1.ListSignal`           |
| `MapSignal<K, V>`       | `gib.v1.MapSignal`            |

These names are normative: a Layer 1 conforming implementation MUST recognize them as the identifiers of the standard shapes when they appear in a signal type descriptor (�4.1), regardless of how the shape's payload type parameters (`T`, `K`, `V`) are themselves bound.

The mechanism by which payload type parameters are bound to a generic standard shape (e.g. expressing "`ValueSignal` of `gib.v1.StringValue`") is defined alongside the `.proto` definitions in the reference implementation. Future revisions of this layer MAY publish additional standard shapes under the same `gib.v<n>` package convention.

### 2.2 Alternative encodings

Implementations MAY support additional encodings (for example, a JSON projection for human inspection or debug tooling). Any such encoding MUST:

- Be losslessly convertible to and from the Protobuf form.
- Be selected only via the negotiation mechanism of the active Layer 3 transport (e.g. `Accept`/`Content-Type` on HTTP/2). The encoding MUST NOT be implied by URI scheme, path, or any other channel- or function-level attribute.

A peer that does not advertise support for an alternative encoding MUST receive Protobuf.

### 2.3 Self-description and exceptions

Exception payloads (Layer 0 �7) are themselves Protobuf messages. Their schema is fixed by this layer (carried in the same `.proto` definitions as `FunctionMetadata`) and is independent of any user-defined payload type. An exception payload MUST carry at least:

- A type identifier (string).
- A human-readable message (string).
- An optional structured cause chain.
- An optional, transport-portable diagnostic blob (stack trace or equivalent).

Layer 3 transport bindings define how the exception payload is delivered on their wire.

---

## 3. Standard signal shapes

This section defines the **standard signal shapes** the spec ships out of the box. Each one captures a commonly understood conflict-free replicated datatype (CRDT) pattern that build pipelines reach for repeatedly. They are normative *as definitions*: an implementation that claims to speak one of these shapes MUST behave the way that shape's section says.

They are NOT a closed universe of possible signal types. A channel's signal type MAY be one of these standard shapes parameterized by a payload type `T`, or it MAY be any other type the function's author chooses, subject only to the encodability requirement in �2. Peers that do not understand a non-standard signal type cannot consume the channel meaningfully, the same way they cannot consume an unknown payload type � but the spec does not forbid such channels from existing.

### 3.1 `ValueSignal<T>`

A channel of `ValueSignal<T>` represents one logical value of type `T` that may be replaced over time.

The shape's signal vocabulary:

- `Set(T value)` � the channel's current logical value becomes `value`.
- `Reset` � the channel has no current logical value (it is "empty" or "unset").
- `Freeze` / `Resume` � framing markers (�3.6).

Late-joiner contract: a subscriber starting from the current logical state MUST receive either nothing (channel is unset and has never been set) or a single `Set` carrying the current value. A subscriber starting from the beginning of retained history MAY receive any sender-compacted prefix (�Layer 0 �6.1) consistent with that contract.

### 3.2 `SetSignal<T>`

A channel of `SetSignal<T>` represents an unordered set of `T` values whose membership evolves.

The shape's signal vocabulary:

- `Add(T item)` � add `item` to the set.
- `AddMany(T[] items)` � add all `items` to the set.
- `Remove(T item)` � remove `item` from the set.
- `RemoveMany(T[] items)` � remove all `items` from the set.
- `Clear` � the set is now empty.
- `Freeze` / `Resume` � framing markers (�3.6).

Late-joiner contract: a subscriber starting from the current logical state MUST receive a sequence of signals � typically `Clear` followed by an `AddMany` of the current contents, or an equivalent � that yields the current set membership.

`SetSignal<T>` is unordered: implementations MUST NOT rely on the order of `Add`/`Remove` signals to convey meaning.

### 3.3 `SequenceSignal<T>`

A channel of `SequenceSignal<T>` represents an ordered, append-style sequence of `T` values.

The shape's signal vocabulary:

- `Append(T item)` � append `item` to the end of the sequence.
- `AppendMany(T[] items)` � append all `items` to the end, in order.
- `Clear` � the sequence is now empty.
- `Freeze` / `Resume` � framing markers (�3.6).

Late-joiner contract: a subscriber starting from the current logical state MUST receive a sequence of signals that yields the current sequence contents in the correct order.

`SequenceSignal<T>` does not define removal. A sequence is append-only between `Clear`s; senders that need to revoke earlier items MUST use `Clear` and re-append the desired suffix.

### 3.4 `ListSignal<T>`

A channel of `ListSignal<T>` represents an ordered list of `T` values whose contents evolve by insertion, removal, and replacement at arbitrary positions. Unlike `SequenceSignal<T>` (�3.3), which is append-only between `Clear`s, `ListSignal<T>` allows the sender to mutate any position in the list at any time.

The shape's signal vocabulary:

- `Insert(int index, T item)` � insert `item` at position `index`, shifting existing elements at and after `index` one position to the right. `index` MUST be in `[0, length]`.
- `InsertMany(int index, T[] items)` � insert `items` at position `index`, in order, shifting existing elements at and after `index` to the right by `items.Length`. `index` MUST be in `[0, length]`.
- `RemoveAt(int index)` � remove the element at position `index`, shifting later elements one position to the left. `index` MUST be in `[0, length)`.
- `RemoveRange(int index, int count)` � remove `count` consecutive elements starting at `index`. `index` and `count` MUST satisfy `index + count <= length`.
- `Replace(int index, T item)` � replace the element at position `index` with `item`. `index` MUST be in `[0, length)`.
- `Clear` � the list is now empty.
- `Freeze` / `Resume` � framing markers (�3.6).

Late-joiner contract: a subscriber starting from the current logical state MUST receive a sequence of signals � typically `Clear` followed by an `InsertMany(0, currentContents)`, or an equivalent � that yields the current list contents in the correct order.

`ListSignal<T>` is ordered: positions are significant, and every signal that takes an `index` is interpreted against the list state implied by all preceding signals on the channel (modulo framing, �3.6). Implementations MUST apply signals in the order they are received.

Index validity is the sender's responsibility. A signal whose `index` (or `index + count`) falls outside the bounds defined above is a protocol error; receivers MAY surface it as such rather than attempting to interpret it.

### 3.5 `MapSignal<K, V>`

A channel of `MapSignal<K, V>` represents an unordered map (a key-keyed dictionary) from `K` to `V` whose entries evolve over time. Each key has at most one current value; setting an existing key replaces the prior value for that key.

The shape's signal vocabulary:

- `Set(K key, V value)` � the entry for `key` becomes `value` (insert if absent, replace if present).
- `SetMany(Entry<K, V>[] entries)` � apply `Set` for each entry, in order. If two entries in the batch share a key, the later one wins.
- `Remove(K key)` � remove the entry for `key`, if any.
- `RemoveMany(K[] keys)` � remove the entries for all `keys`.
- `Clear` � the map is now empty.
- `Freeze` / `Resume` � framing markers (�3.6).

Late-joiner contract: a subscriber starting from the current logical state MUST receive a sequence of signals � typically `Clear` followed by a `SetMany` of the current entries, or an equivalent � that yields the current map contents.

`MapSignal<K, V>` is unordered with respect to its entries: implementations MUST NOT rely on the order in which `Set`/`Remove` signals for distinct keys arrive to convey meaning. Order between operations on the *same* key is significant (later operations supersede earlier ones), and order within a `SetMany` batch is significant for resolving intra-batch key collisions as described above.

`K` MUST be a payload type whose Protobuf encoding admits a stable equality relation; two key values whose serialized forms are equal MUST be treated as the same key.

### 3.6 Framing markers (`Freeze` / `Resume`)

Every shape includes two framing signals:

- `Freeze` � the sender is about to emit a batch of related changes; subscribers SHOULD defer applying further signals until `Resume` is observed.
- `Resume` � the batch is complete; subscribers SHOULD apply everything received between the matched `Freeze` and `Resume` atomically.

`Freeze` and `Resume` MUST be balanced within a channel and MUST NOT nest. A `Resume` without a preceding unmatched `Freeze` is a protocol error.

Framing is advisory for correctness: a subscriber that ignores `Freeze`/`Resume` and applies signals individually MUST still arrive at the same logical state, just possibly through transient intermediate states the sender preferred to hide.

### 3.7 Non-standard signal types

Nothing in this layer requires a channel's signal type to be one of the standard shapes above. Functions MAY define their own signal types � for example a richer hierarchy describing structural changes to an ECMA-335 metadata table, byte-range updates to a PE file, or a domain-specific CRDT not covered by �3.1��3.5 � and expose channels carrying them.

When they do, the following still apply unconditionally:

- The signal type MUST be encodable per �2.
- The function's `FunctionMetadata` MUST describe the channel's signal type accurately enough for a peer to retrieve and decode it (�4.1).
- Late-joiner behavior, history retention, and compaction remain governed by Layer 0 �6 and are the sender's responsibility to define for that signal type.

A future revision of this layer MAY promote additional shapes to standard status. Doing so is purely additive: it gives more peers a guaranteed mutual understanding of those shapes, but it does not change what was already legal.

---

## 4. `FunctionMetadata`

A `FunctionMetadata` describes a single function type. It MUST contain at least the following:

- **`inputs`** � an ordered list of named input declarations, each consisting of:
  - `name` (string, unique within the schema)
  - `type` (a signal type descriptor; see �4.1) � identifies either a standard shape (�3) parameterized by its payload type(s), or a non-standard signal type (�3.7)
  - `required` (boolean; if false, the function tolerates the input being unbound)
- **`outputs`** � an ordered list of named output declarations, each with the same structure as inputs except that `required` is omitted (every declared output is allocated by `Invoke`).
- **`documentation`** � optional human-readable description.

A `FunctionMetadata` is itself a value: it has no identity beyond the function it describes, and two functions with structurally equal schemas are interchangeable from a metadata point of view (though not from an instance-identity point of view; see Layer 0 �2.1).

### 4.1 Signal type descriptors

A signal type descriptor identifies the Protobuf message type carried as the signals on a channel. The identifier is the **fully-qualified Protobuf message name** (e.g. `gib.v1.ValueSignal`), as defined by the Protobuf language specification � the canonical name composed of the message type's `package` and any enclosing message names, joined by `.`. For the standard shapes in �3, the descriptor names the standard shape's Protobuf message (parameterized by the payload type(s) the shape requires � `T` for `ValueSignal`/`SetSignal`/`SequenceSignal`/`ListSignal`, and `K`/`V` for `MapSignal`); the canonical names for the standard shapes are listed in �2.1.1. For non-standard signal types (�3.7), the descriptor names whatever Protobuf message the function's author defined, by its fully-qualified name.

Descriptors MAY additionally be expressed in the `type.googleapis.com/<fully.qualified.Name>` URL form used by `google.protobuf.Any`'s `type_url`; the two forms are equivalent and interchangeable.

In either case the descriptor MUST be sufficient for a peer to either:

1. Look up the message type in a known descriptor pool (e.g. one populated from a `google.protobuf.FileDescriptorSet`) by its fully-qualified name, or
2. Receive an inline `FileDescriptorProto` (or equivalent) from the host alongside the schema.

Implementations MUST support both modes. Layer 3 transport bindings MAY add caching of descriptor pools across calls.

### 4.2 Schema retrieval

Schemas are retrieved via `GetFunctionMetadata` (Layer 0 �2.3). A host MUST be able to answer `GetFunctionMetadata` for any function endpoint URI it serves, without instantiating the function.

---

## 5. Conformance

A Layer 1 conforming implementation MUST:

1. Recognize the standard signal shapes defined in �3 and, for any channel whose declared signal type is one of those shapes, reject signals that do not belong to that shape's vocabulary.
2. Permit channels whose declared signal type is not one of the standard shapes (�3.7), provided the signal type is encodable (�2) and described in the `FunctionMetadata` (�4.1).
3. Honor the late-joiner contracts of �3.1, �3.2, �3.3, �3.4, �3.5 in cooperation with the owning host's compaction (Layer 0 �6) for any channel that uses one of those standard shapes.
4. Honor the framing semantics of �3.6 wherever the standard shapes are used.
5. Produce and consume `FunctionMetadata` values structured as described in �4.
6. Support Protobuf as the normative encoding (�2.1) for every signal and schema it serializes.
7. If it offers an alternative encoding, ensure it is losslessly convertible to Protobuf and is selected only by transport-level negotiation (�2.2).
8. Encode exceptions according to �2.3.

A Layer 1 implementation MAY add helper APIs, generated code, and tooling around these contracts provided the externally observable types, signals, and schemas remain those defined here.
