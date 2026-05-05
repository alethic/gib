# Layer 4 — Plugin Orchestration

> **Status:** normative.
> **Depends on:** Layer 0, Layer 1, Layer 2, at least one Layer 3 transport binding.
> **Consumed by:** Layer 5 (gib functions), Layer 6 (DSLs).

This document defines how an Orchestrator Host (Layer 2 §4) discovers, launches, and manages additional Function Hosts as **plugins**, and how it composes their `resolve` functions into the resolver chain. It also defines the optional **remote plugin provider** mechanism by which an orchestrator can fetch plugins it does not yet have installed.

Nothing in this layer adds new operations or transports. Everything an orchestrator does to a plugin host happens via Layer 0 operations over Layer 3 transports; this layer specifies the conventions that make those operations compose into a working plugin system.

---

## 1. The plugin directory

An orchestrator MUST have a **plugin directory**: a designated location on disk where additional Function Hosts are installed. The location is implementation-defined (typical locations: `~/.gib/plugins`, `%LOCALAPPDATA%\gib\plugins`, a path supplied at launch); what is required is that the orchestrator scans some well-known directory and treats each entry it finds as a candidate plugin host.

An orchestrator MUST scan its plugin directory at startup and SHOULD rescan on demand thereafter (for example, when explicitly asked, or when a `resolve` lookup misses). It MUST NOT execute any code from a plugin during scanning; only the plugin's manifest (§2) is read.

Adding a plugin is a file-system operation: drop the bundle into the directory. Removing a plugin is a file-system operation: delete the bundle. There is no separate registration API.

---

## 2. Plugin layout

A plugin is a self-contained **bundle**: a directory (or directory-like archive) containing at least:

- An **executable** to run when the plugin host is launched. The exact form (native binary, script with shebang, packaged container, managed assembly + launcher) is implementation-defined.
- A **host manifest** describing the plugin.

### 2.1 Host manifest

The host manifest is a small, machine-readable document with at least the following fields:

- **`uriClaims`** — a list of URI scheme/prefix patterns this host claims to serve. Used by the orchestrator's `resolve` to decide which `FunctionRef`s belong to this plugin. The URI scheme of a claim also determines the Layer 3 transport the plugin will speak (e.g. `http2://` vs. `grpc://`); the manifest does not separately declare transports.
- **`launch`** — the launch invocation: the executable to run (relative to the bundle) and any fixed arguments. MAY include placeholders for orchestrator-supplied launch parameters (§3.1).
- **`options`** *(optional)* — declared launch-time options the plugin accepts (named, typed). Layer 5 may consume these for plugins it ships.

The exact serialization (JSON, YAML, TOML, Protobuf) is implementation-defined but MUST be self-describing enough that an orchestrator can read the fields above without running code from the plugin.

### 2.2 Identity

A plugin's identity, for routing-table purposes, is its bundle location in the plugin directory (§1). An orchestrator MUST tolerate multiple bundles with overlapping `uriClaims` being installed concurrently; ties are resolved by orchestrator policy (for example, most-specific-claim-wins, with explicit pinning supported).

---

## 3. Launch protocol

When the orchestrator first needs a plugin host — because a `FunctionRef` resolved through the orchestrator's `resolve` matched the plugin's `uriClaims` and an `Invoke` is about to be performed against a URI on it — the orchestrator launches the plugin's executable as a child process. *Subscribing* to a channel MUST NOT trigger a launch (Layer 0 §5.2: a channel URI is only reachable while its owning host is already up); only `Invoke` does.

### 3.1 Launch parameters

The orchestrator MUST pass the following to the launching plugin via implementation-defined means (command-line arguments, environment variables, an args file):

- **Orchestrator base endpoint URI.** The orchestrator's own base endpoint URI (Layer 2 §1). The plugin MUST store this and use it to derive `<orchestrator-base>/.well-known/gip/resolve` as the next link in its resolver chain (Layer 2 §3.1, rule 2; §4.3).

The orchestrator MAY additionally pass:

- **Manifest-declared options** *(if any).* Values for any `options` the manifest declared.
- Any other implementation-defined parameters (for example, a working directory, bind hints, or an authentication credential). None of these are required by this revision of the spec; future revisions MAY promote some of them to required.

The orchestrator does NOT tell the plugin which Layer 3 transport to bind. The plugin chooses the transport itself (its choice will normally match the URI scheme of one of its `uriClaims`, §2.1) and reports its bound base endpoint URI back via the handshake (§3.2). The URI scheme of that base endpoint URI is what tells the orchestrator which transport to dial.

### 3.2 Handshake

Once the plugin process has bound its transport(s), it MUST publish its **base endpoint URI** back to the orchestrator. The minimum form is:

- A single line written to standard output containing the URI, followed by a newline.

A plugin MAY additionally support writing to a known file path supplied at launch, for environments where stdout is unsuitable. Whichever mechanism is used, the orchestrator MUST be able to read the URI before it sends any operation to the plugin.

The handshake carries no schema information. Schemas are retrieved per Layer 0 §2.3 from each function's own endpoint, the same way any other client retrieves them.

### 3.3 Post-handshake routing

Once the orchestrator has the plugin's base endpoint URI, it adds the plugin to its routing table: any `FunctionRef` whose URI matches the plugin's `uriClaims` is now answered by chaining into `<plugin-base>/.well-known/gip/resolve` (Layer 2 §3.1).

From this point on, the plugin host is just another peer Function Host. The orchestrator dials it via the agreed transport using ordinary Layer 0 operations.

---

## 4. Lifetime of plugin hosts

### 4.1 Liveness

A plugin host's lifetime is governed by Layer 0's liveness rules, applied to the references the orchestrator (and, transitively, its callers) hold against the plugin:

- The plugin host stays alive as long as at least one held-open `Invoke` references any URI on it.
- When no held-open operation references any URI on the plugin host, the plugin host MAY release its resources and exit; this decision belongs to the plugin host, not to the orchestrator.

The orchestrator does NOT close operations against a plugin to make it shut down. Operation completion is the host's decision (Layer 0 §5). The orchestrator's role is observational: it watches for the plugin process to exit (or for its transport connections to drop) and updates its routing table accordingly (§4.2).

The orchestrator MAY, however, proactively terminate plugin processes it has launched. The canonical mechanism is the OS-level termination signal for the platform — `SIGTERM` on POSIX, `CTRL_BREAK_EVENT` / `TerminateProcess` on Windows, or whatever equivalent the host OS provides. The orchestrator SHOULD use this mechanism on its own shutdown, and MAY use it at other times (for example, to evict a misbehaving plugin). A plugin host that does not exit within an orchestrator-defined grace period after the signal MAY be force-killed.

A future revision of this layer MAY define a well-known `shutdown` function (Layer 2 §3) for in-band, graceful shutdown requests; until then, the OS termination signal is the only shutdown mechanism the orchestrator has.

### 4.2 Crash recovery

If a plugin host crashes (process exits abnormally, transport disconnects unexpectedly), the orchestrator MUST:

1. Drop the plugin from the routing table.
2. On the next URI lookup that would route to this plugin, relaunch it from the manifest (§3) as if for the first time.

The orchestrator does not track operations that were in flight against the plugin. Those operations are observed by their own callers via the Layer 3 transport (a dropped stream surfaces as a transport-level failure per Layer 0 §7); recovering from them is the caller's concern, not the orchestrator's.

A plugin host that crashes repeatedly MAY be marked unhealthy by the orchestrator, with relaunches throttled or suppressed; the policy is implementation-defined.

### 4.3 Uninstallation

If a plugin's bundle is removed from the plugin directory between scans, the orchestrator MUST drop it from the routing table on the next scan. Operations already in flight against it MAY continue until they complete naturally; new operations MUST NOT be routed to it.

---

## 5. The orchestrator's `resolve`

The orchestrator's well-known `resolve` (Layer 2 §3.1) is the chain root for every plugin host. Its responsibilities, beyond the abstract rules of Layer 2 §3.1, are:

1. **Match.** For an input `FunctionRef`, consult the routing table built from §1 (local plugins) and from prior remote-provider responses (§6). If a `uriClaims` pattern matches, the request is destined for that plugin.
2. **Launch on demand.** If the matched plugin is not yet running, launch it per §3 before chaining.
3. **Chain.** Invoke the matched plugin's `resolve` and pipe its output through, per Layer 2 §3.1 rule 2.
4. **Fall through.** If no local plugin matches, optionally consult remote plugin providers (§6). If they too produce no match, pass the input `FunctionRef` through unchanged per Layer 2 §3.1 rule 3 — the URI is then assumed to be a directly-dialable endpoint URI (Layer 2 §3.1 final paragraph; "Unmanaged remote endpoints" in the README).

The orchestrator MUST NOT short-circuit the chain by emitting an intermediate `FunctionRef` pointing at the next resolver; the only `FunctionRef` ever delivered on its output channel is the final, dialable one (Layer 2 §3.1 rule 2).

---

## 6. Remote plugin providers *(optional)*

This entire section is optional. An orchestrator is conformant without implementing any of it; the rules below apply only to orchestrators that choose to offer remote plugin providers.

The orchestrator MAY consult one or more **remote plugin providers** for URIs that no installed plugin claims, before falling through to pass-through (§5 step 4). A remote plugin provider is, from the orchestrator's point of view, an ordinary `resolve`-compatible service reachable over a Layer 3 transport at a configured base URI.

### 6.1 Provider behavior

A provider MUST conform to the `resolve` schema of Layer 2 §3.1. Its job is to take a `FunctionRef` whose URI uses some scheme/prefix and answer with a `FunctionRef` whose URI describes the plugin that knows how to serve it — typically a `gib:plugin/...`-style URI under which the orchestrator already knows how to install bundles.

### 6.2 Installation

When a provider answers with a plugin-bundle URI, the orchestrator MUST:

1. Download the bundle to a local cache that participates in the next plugin-directory scan (or directly into the plugin directory).
2. Read its manifest per §2.
3. From this point, treat the plugin as installed for all purposes (including launch on demand per §3).

The next time the same URI scheme is resolved, the orchestrator MUST answer from the local routing table without going back to the provider.

### 6.3 Configuration and policy

The set of providers an orchestrator consults MUST be configurable. Implementations MUST support:

- **Adding** providers (private repositories, mirrors).
- **Replacing** the default provider list.
- **Disabling** remote lookup entirely (offline / pinned deployments).

CI environments and reproducible builds typically pin or empty the provider list so resolution is fully deterministic against what is already on disk.

### 6.4 Trust

Because consulting a remote provider can result in code being downloaded and later launched as a child process, providers MUST be authenticated using whatever mechanism the chosen Layer 3 transport supports, and downloaded bundles MUST be subject to whatever signature/integrity checks the orchestrator is configured to require. Auto-install from the network is a per-deployment **policy**, not a default behavior of the spec.

An orchestrator MUST refuse to launch any plugin whose bundle fails the configured integrity checks.

---

## 7. Conformance

A Layer 4 conforming Orchestrator Host MUST:

1. Maintain a plugin directory and scan it per §1.
2. Recognize plugin manifests with at least the fields in §2.1 and route by `uriClaims` per §2.2.
3. Launch plugin hosts on demand only as the result of an `Invoke` — never of a `Subscribe` (§3, Layer 0 §5.2).
4. Pass at least the launch parameters in §3.1, including its own base endpoint URI (Layer 2 §4.3).
5. Accept the handshake form in §3.2 (stdout line; optionally also a file path).
6. Honor liveness and crash-recovery rules per §4.
7. Implement its `resolve` per §5, including matching, on-demand launch, chaining, and pass-through.
8. If it offers remote plugin providers, honor §6 — including configurability (§6.3) and trust (§6.4).
