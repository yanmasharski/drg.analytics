# DRG Analytics — Architecture

## Overview

The DRG Analytics system provides a provider-agnostic abstraction for event tracking. Game code works exclusively against interfaces; concrete SDK integrations live in separate packages and are injected at startup. The same game logic runs against real analytics backends (Firebase, GameAnalytics, etc.) or mock implementations without any changes.

---

## Layer diagram

```
Game code
    │
    ▼
IAnalyticsGateway  (entry point — Track events)
    │
    ├── IAnalyticsEvent  (event data carrier: Name + Parameters)
    │
    └── IAnalyticsEventHandler  (optional middleware chain, registered per gateway)

Core implementations (drg.analytics runtime)
    ├── AnalyticsGatewayBase      — handler chain + abstract TrackDefault hook
    ├── AnalyticsGatewayComposite — fan-out: forwards every event to all registered gateways
    ├── AnalyticsGatewayMemory    — records events in a list; for tests and debug tooling
    └── AnalyticsGatewayFile      — appends events to a log file; for QA pipelines

Provider packages (separate repos / UPM packages)
    ├── drg.analytics.firebase      — AnalyticsGatewayFirebase
    ├── drg.analytics.gameanalytics — AnalyticsGatewayGameAnalytics
    ├── drg.analytics.unity         — AnalyticsGatewayUnity
    ├── drg.analytics.amplitude     — AnalyticsGatewayAmplitude
    ├── drg.analytics.bytebrew      — AnalyticsGatewayByteBrew
    └── drg.analytics.appmetrica    — AnalyticsGatewayAppMetrica
```

---

## Core abstractions (`DRG.Analytics` assembly)

### `AnalyticsGatewayBase`
Abstract base class for all provider gateway implementations. Maintains a list of `IAnalyticsEventHandler` instances.

When `Track` is called:
1. Iterates handlers in registration order — first handler that returns `true` short-circuits.
2. If no handler consumed the event, calls `TrackDefault`.

Provider packages subclass this and implement `TrackDefault` with SDK-specific routing. Placed in `DRG.Analytics` so provider packages only need to reference the single abstractions assembly.

### `IAnalyticsGateway`
Entry point for all game code. Game code should never hold a reference to a concrete provider.

```csharp
void Track(IAnalyticsEvent @event);
```

Use the `AnalyticsGatewayExtensions.Track(string eventName)` overload for quick name-only events.

### `IAnalyticsEvent`
Read-only contract for event data. Implementations carry a name and a parameter dictionary.

```csharp
string Name { get; }
IReadOnlyDictionary<string, object> Parameters { get; }
```

### `IAnalyticsEventHandler`
Middleware that can intercept events inside a gateway before the provider default runs.

```csharp
bool TryHandle(IAnalyticsEvent @event);
```

Return `true` to consume the event (gateway default is skipped). Return `false` to pass it on to the next handler or the gateway default. Handlers are evaluated in registration order.

### `AnalyticsEvent`
Default `IAnalyticsEvent` implementation and recommended base class for typed events.

```csharp
// Ad-hoc, no subclassing needed:
new AnalyticsEvent("level_start", new() { ["level"] = 1 })

// Typed subclass:
public sealed class EventLevelStart : AnalyticsEvent
{
    public EventLevelStart(int level) : base("level_start") => Set("level", level);
}
```

Null or empty-string parameter values are automatically replaced with `"nullOrEmpty"` and a warning is logged. `AnalyticsEvent.Logger` must be set at startup to enable warnings.

---

## Core implementations (`DRG.Analytics.Runtime` assembly)

### `AnalyticsGatewayComposite`
Single entry point that fans out every event to all registered provider gateways. Extends `AnalyticsGatewayBase`, so handlers attached at the composite level run once globally — before any individual provider sees the event.

```csharp
var analytics = new AnalyticsGatewayComposite();
analytics.Add(new AnalyticsGatewayFirebase(firebase));
analytics.Add(new AnalyticsGatewayGameAnalytics());
analytics.AddHandler(new DebugLoggingHandler());

analytics.Track(new EventLevelStart(5));  // reaches all providers
```

### `AnalyticsGatewayMemory`
Records every tracked event in `IReadOnlyList<IAnalyticsEvent> Events`. Use in unit tests and debug tooling to assert which events were sent without a real SDK.

### `AnalyticsGatewayFile`
Appends every tracked event to a log file as TSV lines:
```
ISO-8601-UTC-timestamp    event_name    {key=value, key=value}
```
Intended for debug builds and QA pipelines — not for production analytics.

---

## Provider package pattern

Every provider package follows the same structure:

```
Runtime/Impl/
    AnalyticsGatewayXxx.cs   — subclasses AnalyticsGatewayBase, implements TrackDefault
```

### `AnalyticsGatewayXxx`

```csharp
public sealed class AnalyticsGatewayXxx : AnalyticsGatewayBase
{
    public AnalyticsGatewayXxx(/* SDK deps */) { /* store deps, init if needed */ }

    protected override void TrackDefault(IAnalyticsEvent @event)
    {
        switch (@event)
        {
            case EventLevelStart e:
                XxxSdk.LogLevelStart(e.Level);
                break;

            default:
                // Generic fallback: forward name + all parameters
                XxxSdk.LogCustomEvent(@event.Name, @event.Parameters);
                break;
        }
    }
}
```

Rules:
- Always include a `default` branch that forwards unknown events generically.
- Handle initialization guards inside the gateway — if the SDK is not ready, silently drop or buffer.
- Provider packages depend only on `DRG.Analytics` (abstractions). They do **not** require `DRG.Analytics.Runtime`.

---

## Assembly dependencies

`com.drg.analytics` ships two assemblies:

```
DRG.Analytics  (abstractions + provider base — IAnalyticsGateway, IAnalyticsEvent,
                IAnalyticsEventHandler, AnalyticsEvent, AnalyticsGatewayBase,
                AnalyticsGatewayExtensions)
    └── DRG.Core

DRG.Analytics.Runtime  (built-in implementations — AnalyticsGatewayComposite,
                         AnalyticsGatewayMemory, AnalyticsGatewayFile)
    ├── DRG.Analytics
    └── DRG.Core
```

Provider packages depend only on `DRG.Analytics`:

```
drg.analytics.firebase      (DRG.Analytics.Firebase)
    ├── DRG.Analytics
    ├── DRG.Firebase
    └── Firebase.Analytics.dll  (Firebase SDK)

drg.analytics.gameanalytics (DRG.Analytics.GameAnalytics)
    ├── DRG.Analytics
    └── GameAnalyticsSDK  (Game Analytics SDK)

drg.analytics.unity         (DRG.Analytics.Unity)
    ├── DRG.Analytics
    └── Unity.Services.Analytics  (Unity Analytics SDK)

drg.analytics.amplitude     (DRG.Analytics.Amplitude)
    ├── DRG.Analytics
    └── amplitude-unity  (Amplitude SDK)

drg.analytics.bytebrew      (DRG.Analytics.ByteBrew)
    ├── DRG.Analytics
    └── ByteBrewSDK  (ByteBrew SDK)

drg.analytics.appmetrica    (DRG.Analytics.AppMetrica)
    ├── DRG.Analytics
    └── AppMetrica  (AppMetrica SDK)
```

Bootstrap code that wires up `AnalyticsGatewayComposite` is the only place that needs both `DRG.Analytics` and `DRG.Analytics.Runtime`.

---

## Bootstrap example

```csharp
// 1. Create composite gateway
var analytics = new AnalyticsGatewayComposite();

// 2. Register providers
analytics.Add(new AnalyticsGatewayFirebase(firebase));
analytics.Add(new AnalyticsGatewayGameAnalytics());
// analytics.Add(new AnalyticsGatewayAmplitude(apiKey));

// 3. (Optional) global middleware — runs once before every provider
analytics.AddHandler(new DebugLoggingHandler());

// 4. Register service
serviceLocator.Register<IAnalyticsGateway>(analytics);
```

Game code tracks events:

```csharp
// Typed event (recommended):
_analytics.Track(new EventLevelStart(level: 5));

// Ad-hoc event:
_analytics.Track(new AnalyticsEvent("tutorial_step", new() { ["step"] = "intro" }));

// Name-only event (extension):
_analytics.Track("game_paused");
```

---

## Design decisions

| Decision | Rationale |
|----------|-----------|
| `IAnalyticsGateway` is a single `Track` method | Analytics senders need only one operation — tracking. Init, flush, and consent are SDK concerns handled inside each gateway. |
| `AnalyticsGatewayBase` + handler chain, not a pipeline | Handlers are opt-in per-gateway customisation points. Most gateways use none; the abstraction has zero overhead when unused. |
| `AnalyticsGatewayComposite` at the top, not inside each gateway | A single fan-out point keeps provider gateways simple. Global enrichment or suppression handlers attach once at the composite, not repeated in each provider. |
| `AnalyticsEvent.Logger` is a static property | Logging is a cross-cutting concern for event authors (subclass constructors). A static makes it accessible without threading `ILogger` through every event subclass. |
| Separate `DRG.Analytics` and `DRG.Analytics.Runtime` assemblies | Game code and provider packages can depend on abstractions without pulling in composite/file/memory implementations. |
| Providers subclass `AnalyticsGatewayBase`, not implement `IAnalyticsGateway` directly | This gives every provider the handler chain for free, consistent middleware behaviour across all backends. |
| No `Initialize` / `Flush` on `IAnalyticsGateway` | Lifecycle management is SDK-specific and belongs inside each gateway constructor and finalizer. A shared flush API would require every provider to implement it even if the SDK handles it automatically. |
